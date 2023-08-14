import os
import json
import logging
from datetime import datetime, timedelta
import azure.functions as func
from azure.data.tables import TableClient
import jwt

# get username from jwt token in x-ms-token-aad-id-token header
def get_last_name(header_value):
    # decode jwt in x-ms-token-aad-id-token header to get user name without validating signature (verify=False)
    jwt_payload = jwt.decode(header_value, options={"verify_signature": False})
    # get last name from jwt payload
    last_name = jwt_payload['name'].split(' ')[-1].lower()
    return last_name

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.warning('running main of getfamilystatussecure')
    logging.error('running main of getfamilystatussecure')

    try:
        # decode jwt in x-ms-token-aad-id-token header to get user name
        family = get_last_name(req.headers['x-ms-token-aad-id-token'])
    except Exception as e:
        logging.error(e)
        # put all request headers and value in a string called headers
        headers = ""
        for header in req.headers:
            headers += f"{header}: {req.headers[header]}\n"
        
        # return error response
        return func.HttpResponse(
            f"request must have header x-ms-token-aad-id-token family name in it\n\n{headers}",
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
