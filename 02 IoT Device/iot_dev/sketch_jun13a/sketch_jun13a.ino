#include <Wire.h>
#include <WiFi.h>
#include <stdlib.h>
#include <BLEUtils.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLE2902.h>
#include <HTTPClient.h>

#include "SparkFunBME280.h"

BME280 mySensor;

float temp = 0;
float humidity = 0;

bool has_wifi_creds = false;

#define MAX_SSID_LEN 50
#define MAX_PASS_LEN 50
#define MAX_URL_LEN 200

char ssid[MAX_SSID_LEN + 1] = {0};
char pass[MAX_PASS_LEN + 1] = {0};
char azure_update_sens_url[MAX_URL_LEN + 1] = {0};

#define SERVICE_UUID        "71933006-db61-4c41-bfaa-d374279efb65"
#define TEMP_CHAR_UUID      "f96c20eb-05c7-4c31-803b-03428eae9aa2"
#define HUMD_CHAR_UUID      "2d4fa781-cf1c-4ea1-9427-14951f794d80"
#define WIFI_CHAR_UUID      "28919cc6-36d5-11ee-be56-0242ac120002"

enum WIFI_OPS {
  WIFI_OPS_SSID = 0,
  WIFI_OPS_PASS = 1,
  WIFI_OPS_URL = 2
};

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
    if (param->write.len == 0) return;

    // First byte indicates the operation
    switch((WIFI_OPS)param->write.value[0]) {
        case WIFI_OPS_SSID:
          if (param->write.len-1 > MAX_SSID_LEN) break; // TODO : Can print an error...
          memcpy(ssid, param->write.value + 1, param->write.len-1);
          break;
        case WIFI_OPS_PASS:
          if (param->write.len-1 > MAX_PASS_LEN) break; // TODO : Can print an error...
          memcpy(pass, param->write.value + 1, param->write.len-1);
          break;
        case WIFI_OPS_URL:
          if (param->write.len-1 > MAX_URL_LEN) break; // TODO : Can print an error...
          memcpy(azure_update_sens_url, param->write.value + 1, param->write.len-1);
          break;
        default:
          char buff[30] = {0};
          sprintf(buff, "Error! recv wifi op val: %d\0", param->write.value[0]);
          Serial.println(buff);
          break;
    }

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

void setup()
{
  Serial.begin(115200);

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