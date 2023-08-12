import os
import json
import logging
from datetime import datetime, timedelta
import azure.functions as func
from azure.data.tables import TableClient

# get last name from full name
def get_last_name(full_name):
    return full_name.split(" ")[-1]

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('running main of getfamilystatussecure')

    try:
        # check username (x-ms-client-principal-name is provided by the identity provider)
        family = get_last_name(req.headers.get('x-ms-client-principal-name'))
    except Exception as e:
        logging.error(e)
        return func.HttpResponse(
             f"request must have x-ms-client-principal-name header in it",
             status_code=400
        )

    logging.warning(f"user {family} requested the status of the family")

    # connect to database
    connection_string = os.getenv("AzureWebJobsStorage")
    try:
        with TableClient.from_connection_string(connection_string, table_name="project") as table:
            
            # get the status of all babies in the family from the DB
            get_family_babies_query = f"PartitionKey eq '{family}'"
            entities = table.query_entities(get_family_babies_query)

            # put the status in a nice json format
            baby_status_list = []
            for entity in entities:
                try:
                    logging.info(f"got the last status of {entity['RowKey']} {entity['PartitionKey']} from {entity['lastupdate']}")
                    baby_status_list.append({
                        'babyname': entity['RowKey'],
                        'babyid': entity['babyid'],
                        'lastupdate': entity['lastupdate'],
                        'details': entity['details']
                    })
                except:
                    logging.info(f"got the last status of {entity['RowKey']} {entity['PartitionKey']}")
                    baby_status_list.append({
                        'babyname': entity['RowKey'],
                        'babyid': entity['babyid'],
                        'lastupdate': None,
                        'details': None
                    })

            
            response = json.dumps({
                "family": f"{family}",
                "status": baby_status_list
            })
            # loggin the response for debug purposes
            logging.info(response)
            
            # return the status 
            return func.HttpResponse(response, status_code=200)

    # in case of internal server error
    except Exception as e:
        logging.error(e)
        return func.HttpResponse(
             "server internal error",
             status_code=500
        )
