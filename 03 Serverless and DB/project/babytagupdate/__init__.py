import os
import json
import logging
from datetime import datetime, timedelta
import azure.functions as func
from azure.data.tables import TableClient
from uuid import UUID

# check for valid UUID
def is_valid_uuid(uuid_to_test, version=4):
    """
    Check if uuid_to_test is a valid UUID.
    Parameters
    ----------
    uuid_to_test : str
    version : {1, 2, 3, 4}
    Returns
    -------
    `True` if uuid_to_test is a valid UUID, otherwise `False`.
    Examples
    --------
    >>> is_valid_uuid('c9bf9e57-1685-4c89-bafb-ff5af830be8a')
    True
    >>> is_valid_uuid('c9bf9e58')
    False
    """
    try:
        uuid_obj = UUID(uuid_to_test, version=version)
    except ValueError:
        return False
    return str(uuid_obj) == uuid_to_test


def main(req: func.HttpRequest, signalRMessages: func.Out[str]) -> func.HttpResponse:
    logging.info('running main of babytagupdate')

    # parse request
    try: req_body = req.get_json()
    except ValueError:
        logging.info(f'problem parsing the following request as json:\n{req}')
        return func.HttpResponse(
             "request body must be json",
             status_code=400
        )
    try:
        babyid = req_body.get('babyid').lower()
        details = req_body.get('details')
    except:
        return func.HttpResponse(
             "request must contain babyid and details",
             status_code=400
        )
    # make sure babyid is a valid uuid version 4
    if not is_valid_uuid(babyid): return func.HttpResponse(
        f"details must be a json string, but it's value is: {details}",
        status_code=400
        )
    try:
        details_json = json.loads(json.dumps(details))
    except ValueError:
        return func.HttpResponse(
             f"details must be a json string, but it's value is: {details}",
             status_code=400
        )
    try:
        temprature = float(details.get('temprature'))
        humidity = float(details.get('humidity'))
    except:
        return func.HttpResponse(
             "details must contain temprature and humidity, and they must be floats",
             status_code=400
        )
    
    # authenticate & authorize
    # TODO

    # connect to database
    logging.info(f"validating request to update status of {req_body}")
    connection_string = os.getenv("AzureWebJobsStorage")
    try:
        logging.warning(f"trying to update {babyid} in the DB")
        with TableClient.from_connection_string(connection_string, table_name="project") as table:
            
            # search entire table for babyid (across all families)
            logging.info(f"searching for babyid {babyid} in the DB")
            query_filter = f"babyid eq '{babyid}'"
            logging.warning(f"query filter is {query_filter}")
            entities = table.query_entities(query_filter)
            for entity in entities:
                logging.warning(f"babyid {babyid} found in the DB")
                babyname = entity['RowKey']
                family = entity['PartitionKey']
                logging.warning(f"babyname is {babyname} from family {family}")

                # udpate time and location
                update_time = datetime.now().isoformat()
                logging.info(f"update time is set to {update_time}")
                entity['lastupdate'] = update_time
                
                details_dictionary = {}
                try:
                    details_from_db = json.loads(entity['details'])
                    details_dictionary['location'] = details_from_db['location']
                except:
                    details_dictionary['location'] = "unknown"
                details_dictionary['temprature'] = temprature
                details_dictionary['humidity'] = humidity
                entity['details'] = json.dumps(details_dictionary)
                table.update_entity(entity=entity, mode='replace')
                
                # return success
                return func.HttpResponse(
                    f"successfuly updated {babyname}'s status, in time {update_time}",
                    status_code=200
                )

            # return success
            return func.HttpResponse(
                f"babyid {babyid} not found in the DB",
                status_code=200
            )
        

    # in case of internal server error
    except Exception as e:
        logging.error(e)
        return func.HttpResponse(
             "server internal error",
             status_code=500
        )
