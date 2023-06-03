# Storage account
## general information
The URL to the table endpoint is: https://skeletonwebjobsstorage.table.core.windows.net/

## storage design
* partition key <string>:   FAMILY NAME (for now it will be "*family*")
* row key <string>:         BABY NAME
* babyid <string>:          device ID
* timestamp <DateTime>:     time of the last status
* latitude <double>:        the latitude
* longtitude <double>:      the longtitude

# function API
