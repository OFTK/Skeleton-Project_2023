import logging
import json


import azure.functions as func


# def main(req: func.HttpRequest) -> func.HttpResponse:
#     logging.info('Python HTTP trigger function processed a request.')
#     return func.HttpResponse(f"this function will return the counter")

def main(req: func.HttpRequest, counterTable) -> func.HttpResponse:
    listOfTableRowsAsJSON = json.loads(counterTable)
    # Since our table has only one counter, lets read first entry
    val = listOfTableRowsAsJSON[0]['value']
    return func.HttpResponse(f"Crreunt counter value is {val}", status_code=200)