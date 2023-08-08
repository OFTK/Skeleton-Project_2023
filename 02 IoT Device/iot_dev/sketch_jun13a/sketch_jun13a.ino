#include <Wire.h>
#include <WiFi.h>
#include <stdlib.h>
#include <BLEUtils.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <HTTPClient.h>

#include "SparkFunBME280.h"

BME280 mySensor;

float temp = 0;
float humidity = 0;

bool has_wifi_creds = true;

char* ssid = "Home";
char* pass = "097452430";
char* azure_update_sens_url = "http://Azure/update-sensor";
// TODO : Those three should be updated by the web app, moreover they should be set to null at first

#define SERVICE_UUID        "71933006-db61-4c41-bfaa-d374279efb65"
#define TEMP_CHAR_UUID      "f96c20eb-05c7-4c31-803b-03428eae9aa2"
#define HUMD_CHAR_UUID      "2d4fa781-cf1c-4ea1-9427-14951f794d80"

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

void setup()
{
  Serial.begin(115200);

  // BLE setup
  BLEDevice::init("TINOKIBLE");
  BLEServer *pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  BLEService *pService = pServer->createService(SERVICE_UUID);

  // Making temperature point
  BLECharacteristic *pCharacteristic = pService->createCharacteristic(
                                         TEMP_CHAR_UUID,
                                         BLECharacteristic::PROPERTY_READ
                                       );

  pCharacteristic->setCallbacks(new TempCallbacks());
  
  pCharacteristic->setValue(temp);
  
  // Making humidity point
  pCharacteristic = pService->createCharacteristic(
                                         HUMD_CHAR_UUID,
                                         BLECharacteristic::PROPERTY_READ
                                       );

  pCharacteristic->setCallbacks(new HumdCallbacks());
  pCharacteristic->setValue(humidity);
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
}

void loop()
{
  if (has_wifi_creds) { // If got wifi_creds, trying to publish data via wifi
    if (WiFi.status() != WL_CONNECTED) { // Trying to connect
      WiFi.begin(ssid, pass);
    } else { // If connected, publishing data
      WiFiClient client;
      HTTPClient http;
      char sens_data_str[100] = {'\0'};

       http.begin(client, azure_update_sens_url);
       
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
      /*int httpResponseCode =*/ http.POST(sens_data_str);

      http.end();
    }
  }

  delay(60); // TODO : When everything works, run this every half a minute
}