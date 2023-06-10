import os
import json
import logging
from datetime import datetime, timedelta
import azure.functions as func
from azure.data.tables import TableClient

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('running main of getfamilystatus')

    # parse request
    try:
        family = req.params.get('family').lower()
    except Exception as e:
        logging.error(e)
        return func.HttpResponse(
             "request must have family name in it",
             status_code=400
        )

    # authenticate & authorize
    # TODO

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
                        'latitude': entity['latitude'],
                        'longtitude': entity['longtitude']
                    })
                except:
                    logging.info(f"got the last status of {entity['RowKey']} {entity['PartitionKey']}")
                    baby_status_list.append({
                        'babyname': entity['RowKey'],
                        'babyid': entity['babyid'],
                        'lastupdate': None,
                        'latitude': None,
                        'longtitude': None
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
