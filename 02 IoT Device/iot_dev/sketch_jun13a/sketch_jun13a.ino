#include <Wire.h>
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <stdlib.h>
#include <BLEUtils.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLE2902.h>
#include <HTTPClient.h>
#include <sys/random.h>

#include "SparkFunBME280.h"
#include "AESLib.h"

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
char zeroes[16] = {0};

const char azure_details_func_url[] = "https://ilovemybaby.azurewebsites.net/api/babytagupdate";

BLECharacteristic *pWifiSSIDChar;

// BLE globals

#define SERVICE_UUID        "71933006-db61-4c41-bfaa-d374279efb65"
#define TEMP_CHAR_UUID      "f96c20eb-05c7-4c31-803b-03428eae9aa2"
#define HUMD_CHAR_UUID      "2d4fa781-cf1c-4ea1-9427-14951f794d80"
#define SYNC_CHAR_UUID      "3b4fa77b-bb0b-4b12-8ee6-913382a4f2a0"

#define WIFI_SSID_CHAR_UUID      "28919cc6-36d5-11ee-be56-0242ac120002"
#define WIFI_PASS_CHAR_UUID      "02350feb-8302-4ff7-8f04-9e07f69d73df"

void aes_init() {
  // Generating random sync
  esp_fill_random(aes_sync, N_BLOCK);
  for (int i=0; i < N_BLOCK; i++) if (aes_sync[i] == 0) aes_sync[i] = 1; // We use it after as a string
  aes.set_paddingmode(paddingMode::CMS);
}

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
    }
};

class HumdCallbacks: public BLECharacteristicCallbacks {
    void onRead(BLECharacteristic *pCharacteristic) {
      humidity = mySensor.readFloatHumidity();
      pCharacteristic->setValue(humidity);
    }
};

class WifiSSIDCallbacks: public BLECharacteristicCallbacks {
  virtual void onWrite(BLECharacteristic* pCharacteristic, esp_ble_gatts_cb_param_t* param)
  {
    // Not encrypted, just copy...
    if (param->write.len <= MAX_SSID_LEN) {
      memcpy(ssid, param->write.value, param->write.len);
    }

    pCharacteristic->setValue(zeroes);

    if (ssid[0] != '\0' && pass[0] != '\0') has_wifi_creds = true;
  }
};

class WifiSyncCallbacks: public BLECharacteristicCallbacks {
    void onRead(BLECharacteristic *pCharacteristic) {
      char aes_temp[N_BLOCK + 1];
      memcpy(aes_temp, aes_sync, N_BLOCK);
      aes_temp[N_BLOCK] = '\0';

      pCharacteristic->setValue(aes_temp);
    }
};

class WifiPassCallbacks: public BLECharacteristicCallbacks {
  virtual void onWrite(BLECharacteristic* pCharacteristic, esp_ble_gatts_cb_param_t* param)
  {
    // Encrypted, decrypt and copy...
    if (param->write.len <= MAX_PASS_LEN) {
      uint16_t i = aes.decrypt(param->write.value, param->write.len, (byte*)pass, aes_key, 128, aes_sync);
      
      aes_init();
      
      for (; i < param->write.len; i++) {
        pass[i] = '\0';
      }

      if (ssid[0] != '\0' && pass[0] != '\0') has_wifi_creds = true;
    }
  }
};

void wifi_disconnected(WiFiEvent_t event, WiFiEventInfo_t info) { // On disconnection event, ask the user to send creds again
  has_wifi_creds = false;
  memset(ssid, 0, sizeof(ssid));
  memset(pass, 0, sizeof(pass));
  pWifiSSIDChar->setValue(ssid);
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

  pCharacteristic->setCallbacks(new WifiSyncCallbacks());
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
  if (has_wifi_creds) { // If got wifi_creds, trying to publish data via wifi

    if (WiFi.status() != WL_CONNECTED && WiFi.status() != WL_IDLE_STATUS) { // Trying to connect
        
        char buff[200];
        sprintf(buff, "Not connected to wifi. trying: [%s]:[%s]\0", ssid, pass);
        Serial.println(buff);

        WiFi.begin(ssid, pass);
        delay(1000);

    } else if (WiFi.status() == WL_CONNECTED) { // If connected, publishing data  
      Serial.println("Connected to wifi.\0");
      pWifiSSIDChar->setValue(ssid);

      WiFiClientSecure client;
      HTTPClient http;

      client.setInsecure();

      char sens_data_str[300] = {'\0'};
      http.begin(client, azure_details_func_url);
       
       // If you need Node-RED/server authentication, insert user and password below
      //http.setAuthorization("REPLACE_WITH_SERVER_USERNAME", "REPLACE_WITH_SERVER_PASSWORD");

      // Specify content-type header
      http.addHeader("Content-Type", "application/json");
      
      // Data to send with HTTP POST
      sprintf(sens_data_str,
      "{\"babyid\": \"%s\", \"details\": {\"temprature\": %.2f, \"humidity\": %.2f}}",
              SERVICE_UUID,
              mySensor.readTempC(),
              mySensor.readFloatHumidity()
              );

      // Send HTTP POST request
      int httpResponseCode = http.POST(sens_data_str);
      http.end();
    }
  }

  delay(1000); // TODO : When everything works, run this every half a minute
}