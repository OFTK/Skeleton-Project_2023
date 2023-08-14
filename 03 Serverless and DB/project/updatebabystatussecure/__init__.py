import os
import json
import logging
from datetime import datetime, timedelta
import azure.functions as func
from azure.data.tables import TableClient
import jwt

# get last name from full name
def get_last_name(full_name):
    return full_name.split(" ")[-1]

# get username from jwt token in x-ms-token-aad-id-token header
def get_last_name(header_value):
    # decode jwt in x-ms-token-aad-id-token header to get user name without validating signature (verify=False)
    jwt_payload = jwt.decode(header_value, options={"verify_signature": False})
    # get last name from jwt payload
    last_name = jwt_payload['name'].split(' ')[-1].lower()
    return last_name


def main(req: func.HttpRequest, signalRMessages: func.Out[str]) -> func.HttpResponse:
    logging.info('running main of udpatebabystatussecure')

    # parse request
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

    try: req_body = req.get_json()
    except ValueError:
        logging.info(f'problem parsing the following request as json:\n{req}')
        return func.HttpResponse(
             "request body must be json",
             status_code=400
        )
    try:
        babyname = req_body.get('babyname').lower()
        details = req_body.get('details')
    except:
        return func.HttpResponse(
             "request must contain family, babyname, longtitude and latitude",
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
        location = details.get('location')
        temprature = details.get('temprature')
        humidity = details.get('humidity')
    except:
        return func.HttpResponse(
             "details must contain location, temprature and humidity",
             status_code=400
        )
    try:
        temprature = float(temprature)
        humidity = float(humidity)
    except:
        return func.HttpResponse(
             "temprature and humidity must be floats",
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
            entity['lastupdate'] = update_time
            entity['details'] = json.dumps(details)
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
