# Where Is My Tinoki 
an IoT system to monitor the whereabouts of your tinokis, and see if their caretaker leaves them for too long.

in ech folder, there is a README file for that part of the project.

# Mobile App

### General Information:



This app collects data continously from surrounding BabyTags, it collects the data using BLE (Bluetooth Low Energy) and uploads it to the serverless DB (if the device is not connected to Wifi).
Also, it may connect a BabyTag to Wifi, see instructions below on how to do so.

### Ofek: TODO

### Connection with the IoT Device

Establishing a Wi-Fi connection for a BabyTag involves transmitting Wi-Fi credentials (SSID, Password) securely via the mobile app. These credentials needs to be given manually, to do so, navigate to the dedicated page using the "Change BabyTag Wifi" button.
After credentials are given, in order to connect a BabyTag, you need to scan it's QR Code, do so by navigating to the dedicated page using the "Scan for QR" button.
If the BabyTag is recognised as a new one by the system, you would be asked to register a new name for this new baby. The password is transmitted securely (Using AES128 symmetric encryption) to the BabyTag.

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


# Serverless and DB

## Storage account

### general information

The URL to the table endpoint is: <https://skeletonwebjobsstorage.table.core.windows.net/>

### storage design

| column name   | type      | description |
| ---           | ---       | --- |
| PartitionKey  | string    | FAMILY NAME (for now it will be "*family*") |
| RowKey        | string    | the name eof the baby |
| babyid        | string    | device ID |
| lastupdate    | DateTime  | time of the last status. |
| latitude      | double    | the latitude |
| longtitude    | double    | the longtitude |

There is also a timeStamp, its the creation time of the DB entity, and i couldn't manage to modify it.

###second storage tabale- "families"
this table stores all family names of the system users.
the paritiom key is identical for all families for convinience of accesing the table while the row key is a unique family name.

DateTime is datetime of format iso 8601. To create that in python i used:

```python
from datetime import datetime

current_time_in_iso_format = datetime.now().isoformat() # isoformat() returns a string of the desired format
```

#### example

![Alt text](./db_example.jpg?raw=true "sample from the DB")

### function API

for every function there is a simple python client in the test folder

#### addbaby

no requirements for the header, by the body needs to contain a json of the following structure:

```json
{
    "family": "<FAMILY NAME>",
    "babyname": "<BABY NAME>",
    "babyid": "<BABY ID>"
}
```

#### deletebaby

no requirements for the header, by the body needs to contain a json of the following structure:

```json
{
    "family": "<FAMILY NAME>",
    "babyname": "<BABY NAME>",
    "babyid": "<BABY ID>"
}

#### udpatebabystatus

POST request
no requirements for the header, by the body needs to contain a json of the following structure:

```json
{
        "family": "family",
        "babyname": "ofek",
        "details": 
        {
            "location": "SOME LOCATION STRING",
            "temprature": 25.5,
            "humidity": 50.0
        }
    }
```

#### babytagupdate

POST request
no requirements for the header, by the body needs to contain a json of the following structure:

```json
{
        "babyid": "7de3fab7-0d49-43b3-9eae-c2ae07ef3439",
        "details": 
        {
            "temprature": 25.5,
            "humidity": 50.0
        }
    }
```

the function returns 200 OK if the operation succedded, 400 if the input structure is bad (with a reason in the body), and 500 for some internal server error.

#### getfamilystatus

GET request
one header parameter which is: ```family=[FAMILY NAME]``` e.g.: <http://localhost:7071/api/getfamilystatus?family=cohen>

response is a json with the following structure:

```json
{
    "family": "family", 
    "status":[
        {
            "babyname": "nimrod", 
            "babyid": "82933006-db71-4c41-bfaa-d374279efb66", 
            "lastupdate": "2023-08-11T20:19:34.874480", 
            "details": "{\"location\": \"SOME LOCATION STRING\", \"temprature\": 25.5, \"humidity\": 50.0}"
        },
        {
            "babyname": "ofek", 
            "babyid": "71933006-db61-4c41-bfaa-d374279efb65", 
            "lastupdate": "2023-08-11T20:20:23.806746", 
            "details": "{\"location\": \"SOME LOCATION STRING\", \"temprature\": 25.5, \"humidity\": 50.0}"
        }
        {
            "babyname": "nitzan", 
            "babyid": "7de3fab7-0d49-43b3-9eae-c2ae07ef3439", 
            "lastupdate": null, 
            "details": null
        }
    ]
}
```

The null fields in nitzan's last update is in case the baby was added, but an update was never sent.

#### getallfamilies

GET request
no header parameter needed.
 <http://localhost:7071/api/ getallfamilies>

response is a json with the following structure:
{family_names: (5) ['cohen', 'family', 'kaplan', 'levi', 'toledo']}


#### serveralert

timer trigger - triggers every other minute (" 0 * /2 * * * * ").
samples family and sends signalR message to a hub named "babyalert" with argument field of the following scheme:

```json
[
    {
        "babyid": "1",
        "babyname": "ofek",
        "alertreason": "baby status not updated for 5 minutes"
    },
    {
        "babyid": "2",
        "babyname": "nimrod",
        "alertreason": "temprature is too high"
    }
]
```

#### serveralerttest

http trigger. gets json from push request body, and sends it as the argument field in a signalr message to a hub names "babyalert".



# Web App Overview

The web app has been created using the React framework with the Axios library.This web app is multi-page.

## Pages:

*1. Admin*
The Admin page stands as a control center, for system managers. administrators gain access to a  oversee all registered families. This includes the ability to monitor family activities, receive real-time SignalR alerts, and deleting and adding babies with valid UUIDs.

*2. User*
The User page is designed for individual families within the system. Users are granted selective access, allowing them to view only their own family's information. In addition, users can add new babies with valid UUID. The User page also show pop up updates through SignalR, keeping users informed about the latest updates about their family's babies.

*3. About*
A page that presents the project and our vision..

*4. Sign Up*
A page in which visitors of the web app can leave contact information and sign up to our service.


# Azure services:

the web app acceses the Azure storage tables to apdate and extract information.
it uses Azure function apps and signalR.
we also use Microsoft Azure authenticatiom to allow different premitions for different people.