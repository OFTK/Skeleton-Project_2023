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

// enum WIFI_OPS {
//   WIFI_OPS_SSID = 0,
//   WIFI_OPS_PASS = 1,
//   WIFI_OPS_URL = 2
// };

// enum WIFI_STATE_MACHINE_OPS {
//   WIFI_SM_OPS_NOT_SET = 0,
//   WIFI_SM_GET_LEN = 1,
//   WIFI_SM_OPS_DATA_TRANSMISSION = 2
// };

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

char ssid[MAX_SSID_LEN + 1] = {0};
char pass[MAX_PASS_LEN + 1] = {0};

BLECharacteristic *pWifiSSIDChar;

// BLE globals

#define SERVICE_UUID        "71933006-db61-4c41-bfaa-d374279efb65"
#define TEMP_CHAR_UUID      "f96c20eb-05c7-4c31-803b-03428eae9aa2"
#define HUMD_CHAR_UUID      "2d4fa781-cf1c-4ea1-9427-14951f794d80"
#define SYNC_CHAR_UUID      "3b4fa77b-bb0b-4b12-8ee6-913382a4f2a0"

#define WIFI_SSID_CHAR_UUID      "28919cc6-36d5-11ee-be56-0242ac120002"
#define WIFI_PASS_CHAR_UUID      "02350feb-8302-4ff7-8f04-9e07f69d73df"

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

class WifiSSIDCallbacks: public BLECharacteristicCallbacks {
  virtual void onWrite(BLECharacteristic* pCharacteristic, esp_ble_gatts_cb_param_t* param)
  {
    // Not encrypted, just copy...
    if (param->write.len <= MAX_SSID_LEN) {
      memcpy(ssid, param->write.value, param->write.len);
    }

    char bla[40] = {0};
    sprintf(bla, "Got ssid: %s\0", ssid);
    Serial.println(bla);

    char buff[4] = {0};
    pCharacteristic->setValue(buff);

    if (ssid[0] != '\0' && pass[0] != '\0') has_wifi_creds = true;
  }
};

class WifiPassCallbacks: public BLECharacteristicCallbacks {
  virtual void onWrite(BLECharacteristic* pCharacteristic, esp_ble_gatts_cb_param_t* param)
  {
    char buff[40] = {0};

    // Encrypted, decrypt and copy...
    if (param->write.len <= MAX_PASS_LEN) {
      uint16_t i = aes.decrypt(param->write.value, param->write.len, (byte*)pass, aes_key, 128, aes_sync);
      
      sprintf(buff, "Got pass, len %d\0", i);
      Serial.println(buff);

      for (; i < param->write.len; i++) {
        pass[i] = '\0';
      }

      if (ssid[0] != '\0' && pass[0] != '\0') has_wifi_creds = true;
    }

    sprintf(buff, "Got pass: %s\0", pass);
    Serial.println(buff);
  }
};

void wifi_disconnected(WiFiEvent_t event, WiFiEventInfo_t info) { // On disconnection event, ask the user to send creds again
  has_wifi_creds = false;
  memset(ssid, 0, sizeof(ssid));
  memset(pass, 0, sizeof(pass));
  pWifiSSIDChar->setValue(ssid);
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

  // Setting sync point (for AES128 iv)
  pCharacteristic = pService->createCharacteristic(
                                          SYNC_CHAR_UUID,
                                          BLECharacteristic::PROPERTY_READ
                                        );

  char aes_temp[N_BLOCK + 1];
  memcpy(aes_temp, aes_sync, N_BLOCK);
  aes_temp[N_BLOCK] = '\0';
  pCharacteristic->setValue(aes_temp);

  // Setting points to get wifi creds from
  pCharacteristic = pService->createCharacteristic(
                                          WIFI_SSID_CHAR_UUID,
                                          BLECharacteristic::PROPERTY_WRITE | 
                                          BLECharacteristic::PROPERTY_READ
                                        );

  pCharacteristic->setCallbacks(new WifiSSIDCallbacks());
  pWifiSSIDChar = pCharacteristic;

  pCharacteristic = pService->createCharacteristic(
                                          WIFI_PASS_CHAR_UUID,
                                          BLECharacteristic::PROPERTY_WRITE
                                        );

  pCharacteristic->setCallbacks(new WifiPassCallbacks());

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
  char buff[30] = {0};
  if (has_wifi_creds) { // If got wifi_creds, trying to publish data via wifi

      sprintf(buff, "Has wifi creds!\0", humidity);
      Serial.println(buff);

    if (WiFi.status() != WL_CONNECTED && WiFi.status() != WL_IDLE_STATUS) { // Trying to connect
        WiFi.begin(ssid, pass);
        delay(1000);  
    } else if (WiFi.status() == WL_CONNECTED) { // If connected, publishing data    
      pWifiSSIDChar->setValue(ssid);

      sprintf(buff, "Connected to wifi!\0");
      Serial.println(buff);

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