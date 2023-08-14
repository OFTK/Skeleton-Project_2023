import os
import json
import logging
import azure.functions as func
from azure.data.tables import TableClient
from uuid import UUID

# check for valid UUID
def is_valid_uuid(uuid_to_test, version=4):
    try:
        uuid_obj = UUID(uuid_to_test, version=version)
    except ValueError:
        return False
    return str(uuid_obj) == uuid_to_test

# return true if string contains only letters, numbers, spaces, or dashes
def isname(s: str) -> bool:
    for char in s:
        if not (char.isalnum() or char.isspace() or char == '-'):
            return False
    return True

def main(req: func.HttpRequest, signalRMessages: func.Out[str]) -> func.HttpResponse:
    logging.info('running main of deletebaby')

    # parse request
    try:
        req_body = req.get_json()
    except ValueError:
        logging.info(f'problem parsing the following request as json:\n{req}')
        return func.HttpResponse(
            "request body must be json",
            status_code=400
        )
    try:
        family = req_body.get('family').lower()
        babyid = req_body.get('babyid').lower()
    except:
        return func.HttpResponse(
            "request must contain family and babyid",
            status_code=400
        )

    # check that all names don't contain illegal chars
    if not is_valid_uuid(babyid):
        return func.HttpResponse(
            "babyid must be a valid UUID",
            status_code=400
        )

    # authenticate & authorize
    # TODO

    # connect to database
    logging.info(f"validating request to delete baby {babyid} from {family} family")
    connection_string = os.getenv("AzureWebJobsStorage")
    try:
        with TableClient.from_connection_string(connection_string, table_name="project") as table:
            # check if babyid exists
            get_family_babies_query = f"PartitionKey eq '{family}' and babyid eq '{babyid}'"
            entities = table.query_entities(get_family_babies_query)
            baby_name = None
            for entity in entities:
                baby_name = entity["RowKey"]
                logging.info(f"found baby with name {baby_name}")
                break

            if baby_name is None:
                return func.HttpResponse(
                    f"baby with babyid {babyid} does not exist in the {family} family",
                    status_code=404
                )

            # delete baby from database
            table.delete_entity(partition_key=family, row_key=baby_name)

            # update all clients via signalr
            signalRMessages.set(json.dumps({'target': f'{family}', 'arguments': [f"{baby_name}"]}))

            # return success
            return func.HttpResponse(
                f"successfully deleted baby {baby_name} from the {family} family",
                status_code=200
            )

    # in case of internal server error
    except Exception as e:
        logging.error(e)
        return func.HttpResponse(
            "server internal error",
            status_code=500
        )
