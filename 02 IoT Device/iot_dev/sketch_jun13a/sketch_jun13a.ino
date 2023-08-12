#include <Wire.h>
#include <WiFi.h>
#include <stdlib.h>
#include <BLEUtils.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLE2902.h>
#include <HTTPClient.h>
#include <sys/random.h>

#include "SparkFunBME280.h"
#include "AESLib.h"

enum WIFI_OPS {
  WIFI_OPS_SSID = 0,
  WIFI_OPS_PASS = 1,
  WIFI_OPS_URL = 2
};

enum WIFI_STATE_MACHINE_OPS {
  WIFI_SM_OPS_NOT_SET = 0,
  WIFI_SM_GET_LEN = 1,
  WIFI_SM_OPS_DATA_TRANSMISSION = 2
};

// Encryption globals
AESLib aes;

byte aes_sync[N_BLOCK] = { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA }; // Random on every startup
byte aes_key[N_BLOCK] = { 0xa1, 0x95, 0x1f, 0x50, 0xe5, 0x66, 0x8b, 0xb7, 0x23, 0xd4, 0xfa, 0x8a, 0xb3, 0x5a, 0xef, 0x14 }; // Constant for every iot dev

// Sensor globals
BME280 mySensor;

float temp = 0;
float humidity = 0;

// Wifi globals

bool has_wifi_creds = false;

#define MAX_SSID_LEN 50
#define MAX_PASS_LEN 50
#define MAX_URL_LEN 200

char ssid[MAX_SSID_LEN + 1] = {0};
char pass[MAX_PASS_LEN + 1] = {0};
char azure_update_sens_url[MAX_URL_LEN + 1] = {0};

// Wifi state machine globals
WIFI_STATE_MACHINE_OPS state_machine_op = WIFI_SM_OPS_NOT_SET;
WIFI_OPS save_op = WIFI_OPS_SSID;
int incoming_length = 0;
char* sm_buff_ptr = 0;
int sm_buff_offset = 0;
bool is_encrypted_data = false;

// BLE globals

#define SERVICE_UUID        "71933006-db61-4c41-bfaa-d374279efb65"
#define TEMP_CHAR_UUID      "f96c20eb-05c7-4c31-803b-03428eae9aa2"
#define HUMD_CHAR_UUID      "2d4fa781-cf1c-4ea1-9427-14951f794d80"
#define WIFI_CHAR_UUID      "28919cc6-36d5-11ee-be56-0242ac120002"
#define SYNC_CHAR_UUID      "3b4fa77b-bb0b-4b12-8ee6-913382a4f2a0"

class MyServerCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) {
      pServer->startAdvertising(); // restart advertising
    };

    void onDisconnect(BLEServer* pServer) {
      pServer->startAdvertising(); // restart advertising
    }
};

class TempCallbacks: public BLECharacteristicCallbacks {
    void onRead(BLECharacteristic *pCharacteristic) {
      temp = mySensor.readTempC();
      pCharacteristic->setValue(temp);
      // char buff[20] = {0};
      // sprintf(buff, "Read Temp: %.2f\0", temp);
      // Serial.println(buff); (DEBUG)
    }
};

class HumdCallbacks: public BLECharacteristicCallbacks {
    void onRead(BLECharacteristic *pCharacteristic) {
      humidity = mySensor.readFloatHumidity();
      pCharacteristic->setValue(humidity);
      // char buff[30] = {0};
      // sprintf(buff, "Read Humidity: %.2f\0", humidity);
      // Serial.println(buff); (DEBUG)
    }
};

class WifiCallbacks: public BLECharacteristicCallbacks {
  virtual void onWrite(BLECharacteristic* pCharacteristic, esp_ble_gatts_cb_param_t* param)
  {
    char buff[100] = {0};
    sprintf(buff, "Write incoming! %s\0", param->write.value);
    Serial.println(buff);

    switch(state_machine_op) {
      case WIFI_SM_OPS_NOT_SET: 
        {
          if (param->write.len != 1) {
            // TODO: This is an error, rais an alert?
          } else {
            save_op = (WIFI_OPS)param->write.value[0];
            state_machine_op = WIFI_SM_GET_LEN;
          }
          break;
        }
        case WIFI_SM_GET_LEN: 
        {
          if (param->write.len != 1) {
            // TODO: This is an error, rais an alert?
          } else {
            incoming_length = param->write.value[0];

            switch (save_op) {
              case WIFI_OPS_SSID:
              {
                if (incoming_length > MAX_SSID_LEN) {
                  // TODO: This is an error, rais an alert?
                  state_machine_op = WIFI_SM_OPS_NOT_SET;
                } else {
                  sm_buff_ptr = ssid;
                  sm_buff_offset = 0;
                  is_encrypted_data = false;
                }
                break;
              }
              case WIFI_OPS_PASS:
              {
                if (incoming_length > MAX_PASS_LEN || incoming_length % N_BLOCK != 0) {
                  // TODO: This is an error, rais an alert?
                  state_machine_op = WIFI_SM_OPS_NOT_SET;
                } else {
                  sm_buff_ptr = pass;
                  sm_buff_offset = 0;
                  is_encrypted_data = true;
                }
                break;
              }
              case WIFI_OPS_URL:
              {
                if (incoming_length > MAX_URL_LEN) {
                  // TODO: This is an error, rais an alert?
                  state_machine_op = WIFI_SM_OPS_NOT_SET;
                } else {
                  sm_buff_ptr = azure_update_sens_url;
                  sm_buff_offset = 0;
                  is_encrypted_data = false;
                }
                break;
              }
              default:
              {
                // This is an error! raise an alert?
                state_machine_op = WIFI_SM_OPS_NOT_SET;
                break;
              }
            }
          }
          break;
        }
        case WIFI_SM_OPS_DATA_TRANSMISSION: 
        {
          if (param->write.len != 4) {
            // TODO: This is an error, raise an alert?
          } else {
            memcpy(sm_buff_ptr + sm_buff_offset, param->write.value, 4);
            sm_buff_offset += 4;

            if (sm_buff_offset >= incoming_length) {
              if (is_encrypted_data) { // If data is encrypted, decrypting it...
                aes.decrypt((byte*)sm_buff_ptr, incoming_length, (byte*)sm_buff_ptr, aes_key, 128, aes_sync);
              }

              // Finished the protocol...
              state_machine_op = WIFI_SM_OPS_NOT_SET;
              incoming_length = 0;
              sm_buff_ptr = 0;
              sm_buff_offset = 0;
              is_encrypted_data = 0;
            }
          }

          break;
        }
    }
























  //   if (param->write.len == 0) return;



  //   // First byte indicates the operation
  //   switch((WIFI_OPS)param->write.value[0]) {
  //       case WIFI_OPS_SSID: 
  //       {
  //         if (param->write.len-1 > MAX_SSID_LEN) break;
  //         memcpy(ssid, param->write.value + 1, param->write.len-1);
  //         break;
  //       }
  //       case WIFI_OPS_PASS: // This value is encrypted, we need to decrypt it 
  //       {
  //         if (param->write.len-1 > MAX_PASS_LEN) break;
  //         //memcpy(pass, param->write.value + 1, param->write.len-1);
  //         aes.decrypt(param->write.value + 1, param->write.len - 1, (byte*)pass, aes_key, 128, aes_sync);
  //         char buff[200] = {0};
  //         sprintf(buff, "pass: %s\0", pass);
  //         Serial.println(buff);
  //         break;
  //       }
  //       case WIFI_OPS_URL:
  //       {
  //         if (param->write.len-1 > MAX_URL_LEN) break;
  //         memcpy(azure_update_sens_url, param->write.value + 1, param->write.len-1);
  //         break;
  //       }
  //       default:
  //       {
  //         // char buff[30] = {0};
  //         // sprintf(buff, "Error! recv wifi op val: %d\0", param->write.value[0]);
  //         // Serial.println(buff);
  //         break;
  //       }
  //   }

    if (ssid[0] != '\0' && pass[0] != '\0' && azure_update_sens_url[0] != '\0') {
        pCharacteristic->setValue(ssid);
        has_wifi_creds = true;
    }
  }
};

void wifi_disconnected(WiFiEvent_t event, WiFiEventInfo_t info) { // On disconnection event, ask the user to send creds again
  has_wifi_creds = false;
  memset(ssid, 0, sizeof(ssid));
  memset(pass, 0, sizeof(pass));
  memset(azure_update_sens_url, 0, sizeof(azure_update_sens_url));
}

void aes_init() {
  // Generating random sync
  esp_fill_random(aes_sync, N_BLOCK);
  for (int i=0; i < N_BLOCK;i++) if (aes_sync[i] == 0) aes_sync[i] = 1; // We use it after as a string
  // TODO : DEBUG REMOVE
  int i;
  for (i = 0; i < 16; i++)
  {
      if (i > 0) printf(":");
      printf("%02X", aes_sync[i]);
  }
  printf("\n");
// -------------------
  aes.set_paddingmode(paddingMode::CMS);
}

void setup()
{
  Serial.begin(115200);

  // AES setup
  aes_init();

  // BLE setup
  BLEDevice::init("TINOKIBLE");
  BLEServer *pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  BLEService *pService = pServer->createService(SERVICE_UUID);

  // Setting temperature point
  BLECharacteristic *pCharacteristic = pService->createCharacteristic(
                                         TEMP_CHAR_UUID,
                                         BLECharacteristic::PROPERTY_READ
                                       );

  pCharacteristic->setCallbacks(new TempCallbacks());
  pCharacteristic->setValue(temp);
  
  // Setting humidity point
  pCharacteristic = pService->createCharacteristic(
                                         HUMD_CHAR_UUID,
                                         BLECharacteristic::PROPERTY_READ
                                       );

  pCharacteristic->setCallbacks(new HumdCallbacks());
  pCharacteristic->setValue(humidity);

  // Setting point to get wifi creds from
  pCharacteristic = pService->createCharacteristic(
                                          WIFI_CHAR_UUID,
                                          BLECharacteristic::PROPERTY_WRITE | 
                                          BLECharacteristic::PROPERTY_READ
                                        );

  pCharacteristic->setCallbacks(new WifiCallbacks());

// Setting sync point (for AES128 iv)
  pCharacteristic = pService->createCharacteristic(
                                          SYNC_CHAR_UUID,
                                          BLECharacteristic::PROPERTY_READ
                                        );
  char aes_temp[N_BLOCK + 1];
  memcpy(aes_temp, aes_sync, N_BLOCK);
  aes_temp[N_BLOCK] = '\0';
  pCharacteristic->setValue(aes_temp);

  pService->start();

  BLEAdvertising *pAdvertising = pServer->getAdvertising();
  
  pAdvertising->start();
  Wire.begin();

  // Sensor setup
  if (mySensor.beginI2C() == false) //Begin communication over I2C
  {
    Serial.println("The sensor did not respond. Please check wiring.");
    while(1); //Freeze
  }

  // Wifi setup
  WiFi.onEvent(wifi_disconnected, ARDUINO_EVENT_WIFI_STA_DISCONNECTED);
}

void loop()
{
  if (has_wifi_creds) { // If got wifi_creds, trying to publish data via wifi
    if (WiFi.status() != WL_CONNECTED && WiFi.status() != WL_IDLE_STATUS) { // Trying to connect
        WiFi.begin(ssid, pass);
        delay(1000);  
    } else if (WiFi.status() == WL_CONNECTED) { // If connected, publishing data
      WiFiClient client;
      HTTPClient http;
      char sens_data_str[300] = {'\0'};
      /* http.begin(client, azure_update_sens_url);
       
       // If you need Node-RED/server authentication, insert user and password below
      //http.setAuthorization("REPLACE_WITH_SERVER_USERNAME", "REPLACE_WITH_SERVER_PASSWORD");

      // Specify content-type header
      http.addHeader("Content-Type", "application/x-www-form-urlencoded");
      
      // Data to send with HTTP POST
      sprintf(sens_data_str, 
              "device_id=%s&temp=%.2f&humd=%.2f\0", 
              SERVICE_UUID, 
              mySensor.readFloatHumidity(), 
              mySensor.readTempC());           

      // Send HTTP POST request
      int httpResponseCode = http.POST(sens_data_str);*/

      http.end();
    }
  }

  delay(6000); // TODO : When everything works, run this every half a minute
}