import os
import json
import logging
from datetime import datetime
import azure.functions as func
from azure.data.tables import TableClient

def isname(s: str) -> bool:
    for char in s:
        if (not (char.isalnum() or char.isspace())): return False
    return True

def main(req: func.HttpRequest, signalRMessages: func.Out[str]) -> func.HttpResponse:
    logging.info('running main of addbaby')

    # parse request
    try: req_body = req.get_json()
    except ValueError:
        logging.info(f'problem parsing the following request as json:\n{req}')
        return func.HttpResponse(
             "request body must be json",
             status_code=400
        )
    try:
        family = req_body.get('family').lower()
        babyname = req_body.get('babyname').lower()
        babyid = req_body.get('babyid').lower()
    except:
        return func.HttpResponse(
             "request must contain family, babyname, and babyid",
             status_code=400
        )

    # check that all names don't conain illegal chars
    if not (isname(family) and isname(babyname) and isname(babyid)):
        return func.HttpResponse(
             "request must contain family, babyname, and babyid",
             status_code=400
        )

    # authenticate & authorize
    # TODO

    # check that babyid is legal
    # TODO        

    # connect to database
    logging.info(f"validating request to add {req_body} against the DB")
    connection_string = os.getenv("AzureWebJobsStorage")
    try:
        with TableClient.from_connection_string(connection_string, table_name="project") as table:
            # check if babyname or babyid already exists
            get_family_babies_query = f"PartitionKey eq '{family}'"
            entities = table.query_entities(get_family_babies_query)
            for entity in entities:
                entity_name = entity["RowKey"]
                logging.info(f"found baby with name {entity_name}")

                # if baby name exists - return error
                if entity_name == babyname:
                    logging.warning(f"baby name in request {babyname} already exists in DB as {entity_name}")
                    return func.HttpResponse(
                        f"{babyname} is an already listed baby in the {family} family",
                        status_code=400
                    )
                # if babyid exists - return error
                if entity["babyid"] == babyid:
                    logging.warning(f"baby id in request {babyid} already exists in DB")
                    return func.HttpResponse(
                        f"baby tag {babyid} is an already exists in your account",
                        status_code=400
                    )

            # everything is ok - add baby to database
            new_baby_entity = {
                u'PartitionKey': u'{}'.format(family),
                u'RowKey': u'{}'.format(babyname),
                u'babyid': u'{}'.format(babyid),
                u'latitude': 0.0,
                u'longtitude': 0.0
            }
            
            logging.warning(f"trying to add {new_baby_entity} to the DB")
            new_entity = table.create_entity(entity=new_baby_entity)
            logging.warning(new_entity)

            # update all clients via signalr
            # TODO: when having multiple families, the message should be sent only to the relevant family
            signalRMessages.set(json.dumps({'target': f'{family}', 'arguments': [f"{babyname}"]}))

            # return success
            return func.HttpResponse(
                f"successfuly added {babyname} to the {family} family",
                status_code=200
            )

    # in case of internal server error
    except Exception as e:
        logging.error(e)
        return func.HttpResponse(
             "server internal error",
             status_code=500
        )
