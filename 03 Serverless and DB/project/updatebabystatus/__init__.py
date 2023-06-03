import os
import json
import logging
from datetime import datetime, timedelta
import azure.functions as func
from azure.data.tables import TableClient

def main(req: func.HttpRequest, signalRMessages: func.Out[str]) -> func.HttpResponse:
    logging.info('running main of udpatebabystatus')

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
        longtitude = req_body.get('longtitude')
        latitude = req_body.get('latitude')
    except:
        return func.HttpResponse(
             "request must contain family, babyname, longtitude and latitude",
             status_code=400
        )
    try:
        longtitude = float(longtitude)
        latitude = float(latitude)
    except:
        return func.HttpResponse(
             "longtitude and latitude are supposed to be floats",
             status_code=400
        )

    # authenticate & authorize
    # TODO

    # connect to database
    logging.info(f"validating request to update status of {req_body}")
    connection_string = os.getenv("AzureWebJobsStorage")
    try:
        with TableClient.from_connection_string(connection_string, table_name="project") as table:
            
            # get entity to update
            try:
                logging.warning(f"trying to update {babyname} of house {family}")
                entity = table.get_entity(partition_key=u'{}'.format(family), row_key=u'{}'.format(babyname))
            except Exception as e:
                logging.error(e)
                return func.HttpResponse(
                    f"the baby you wish to update ({babyname} of house {family}) is not listed",
                    status_code=400
                )

            # udpate time and location
            update_time = datetime.now().isoformat()
            logging.info(f"update time is set to {update_time}")
            entity['latitude'] = latitude
            entity['longtitude'] = longtitude
            entity['lastupdate'] = update_time
            logging.warning(f"trying to update {babyname} in the DB")
            table.update_entity(entity=entity, mode='replace')
            
            # return success
            return func.HttpResponse(
                f"successfuly updated {babyname}'s status, in time {update_time}",
                status_code=200
            )

    # in case of internal server error
    except Exception as e:
        logging.error(e)
        return func.HttpResponse(
             "server internal error",
             status_code=500
        )
