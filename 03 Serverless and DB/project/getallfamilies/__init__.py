import os
import json
import logging
from datetime import datetime, timedelta
import azure.functions as func
from azure.data.tables import TableClient

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('running main of getfamilystatus')

    # parse requestimport os
import json
import logging
from datetime import datetime, timedelta
import azure.functions as func
from azure.data.tables import TableClient

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('running main of getfamilystatus')

    # authenticate & authorize
    # TODO

    # connect to database
    connection_string = os.getenv("AzureWebJobsStorage")
    try:
        with TableClient.from_connection_string(connection_string, table_name="families") as table:
            
            logging.info("connected to table")
            # get the status of all babies in the family from the DB
            get_families_query = f"PartitionKey eq 'families'"
            entities = table.query_entities(get_families_query)

            # put the status in a nice json format
            family_name_list = []
            for entity in entities:
                logging.info(f"got the name of {entity['RowKey']}  ")
                family_name_list.append(entity['RowKey'])

            response = json.dumps({ 
                "family_names": family_name_list
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

    try:
        families = req.params.get('families').lower()
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
        with TableClient.from_connection_string(connection_string, table_name="families") as table:
            
            
            # get the status of all babies in the family from the DB
            get_family_babies_query = f"PartitionKey eq '{families}'"
            entities = table.query_entities(get_family_babies_query)

            # put the status in a nice json format
            baby_status_list = []
            for entity in entities:
                try:
                    logging.info(f"got the last status of {entity['RowKey']} {entity['PartitionKey']} from {entity['lastupdate']}")
                    baby_status_list.append({
                        'familyname': entity['RowKey'],
                    })
                except:
                    logging.info("no families to show")
                    

            
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
