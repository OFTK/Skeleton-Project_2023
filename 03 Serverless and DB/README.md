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
