#include <Wire.h>
#include <WiFi.h>
#include <stdlib.h>
#include <BLEUtils.h>
#include <BLEDevice.h>
#include <BLEServer.h>

#include "SparkFunBME280.h"

BME280 mySensor;

float temp = 0;
float humidity = 0;

#define WIFI_SSID           "Home"
#define WIFI_PASS           "097452430"

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
      char buff[20] = {0};
      sprintf(buff, "Read Temp: %.2f\0", temp);
      Serial.println(buff);
    }
};

class HumdCallbacks: public BLECharacteristicCallbacks {
    void onRead(BLECharacteristic *pCharacteristic) {
      humidity = mySensor.readFloatHumidity();
      pCharacteristic->setValue(humidity);
      char buff[30] = {0};
      sprintf(buff, "Read Humidity: %.2f\0", humidity);
      Serial.println(buff);
    }
};

void setup()
{
  Serial.begin(115200);

  // Wifi setup
  Serial.println("Connecting...");

  WiFi.begin(WIFI_SSID, WIFI_PASS);
  
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  
  Serial.println("WiFi connected");


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
  
  delay(50);
}