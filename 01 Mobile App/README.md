# Mobile App

### General Information:

Ofek: TODO

This app collects data continously from surrounding BabyTags, it collects the data using BLE (Bluetooth Low Energy) and uploads it to the serverless DB (if the device is not connected to Wifi).
Also, it may connect a BabyTag to Wifi, see instructions below on how to do so.

### Ofek: TODO

### Connection with the IoT Device

Establishing a Wi-Fi connection for a BabyTag involves transmitting Wi-Fi credentials (SSID, Password) securely via the mobile app. These credentials needs to be given manually, to do so, navigate to the dedicated page using the "Change BabyTag Wifi" button.
After credentials are given, in order to connect a BabyTag, you need to scan it's QR Code, do so by navigating to the dedicated page using the "Scan for QR" button.
If the BabyTag is recognised as a new one by the system, you would be asked to register a new name for this new baby. The password is transmitted securely (Using AES128 symmetric encryption) to the BabyTag.