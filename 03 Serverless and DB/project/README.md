# Storage account
## general information
The URL to the table endpoint is: https://skeletonwebjobsstorage.table.core.windows.net/

## storage design
* partition key <string>:   FAMILY NAME (for now it will be "*family*")
* row key <string>:         BABY NAME
* babyid <string>:          device ID
* lastupdate <DateTime>:    time of the last status
* latitude <double>:        the latitude
* longtitude <double>:      the longtitude
azure adds timeStamp automatically (the time of creation)

### example:
![Alt text](./db_example.jpg?raw=true "sample from the DB")

# function API
for every function there is a simple python client in the test folder
## addbaby
no requirements for the header, by the body needs to contain a json of the following structure:
```json
{
    'family': '<FAMILY NAME>',
    'babyname': '<BABY NAME>',
    'babyid': '<BABY ID>'
}
```

## udpatebabystatus
no requirements for the header, by the body needs to contain a json of the following structure:
```json
{
    'family': '<FAMILY NAME>',
    'babyname': '<BABY NAME>',
    'longtitude': '<LONGTITUDE OF CARETAKER'S LOCATION>',
    'latitude': '<LONGTITUDE OF CARETAKER'S LOCATION>'
}
```

