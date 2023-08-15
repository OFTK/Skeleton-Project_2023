# IoT Device

### General Information:

The IoT Device serves as a versatile BLE server while also offering connectivity through Wi-Fi to the internet. This device is equipped with a BLE service which is assigned the unique identifier (UUID) of each baby. This UUID is acquired by scanning the dedicated QR Code accompanying the babytag. The service is openly accessible for connection.

#### BLE Data Points:

The IoT Device broadcasts two essential "baby status" parameters via BLE: temperature and humidity. Each of these parameters is encapsulated within a BLE characteristic, both sharing a universal UUID across all BabyTag devices:

- Temperature UUID: "f96c20eb-05c7-4c31-803b-03428eae9aa2"
- Humidity UUID: "2d4fa781-cf1c-4ea1-9427-14951f794d80"

Data readings are sourced from a BME280 sensor, with the device initiating sampling with every BLE read event.

### Wi-Fi Connectivity:

Establishing a Wi-Fi connection for the BabyTag involves transmitting Wi-Fi credentials (SSID, Password) via the mobile app. These credentials are relayed through BLE characteristics, each distinguished by a unique UUID:

- SSID UUID: "28919cc6-36d5-11ee-be56-0242ac120002"
- Password UUID: "02350feb-8302-4ff7-8f04-9e07f69d73df"

The SSID is conveyed as plaintext, while the password is securely transferred in ciphertext. Refer to the mobile app's README.md for comprehensive instructions on how to transmit those.

#### Security Measures:

To ensure the privacy and integrity of sensitive data exchanged between the BabyTag and the mobile app, security measures are implemented. Sensitive data, (in the current version consists of only the Wi-Fi password) undergoes AES128 encryption. For encryption purposes, the device generates a 16-byte true random string referred to as "sync," accessible via a dedicated BLE characteristic:

- Sync UUID: "3b4fa77b-bb0b-4b12-8ee6-913382a4f2a0"

Additionally, a symmetric key, essential for AES128 encryption, is shared between the IoT device and the mobile app. The mobile app retrieves this key by scanning the QR Code associated with the device.

While connected to Wi-Fi, the BabyTag ensures the secure transmission of all "baby status" parameters through HTTPS.

For more details and comprehensive usage guidelines, please refer to the mobile app's documentation.
