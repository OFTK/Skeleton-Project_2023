import os
import azure.functions as func
from azure.data.tables import TableClient
import json
import logging

def main(req: func.HttpRequest) -> func.HttpResponse:
    connection_string = os.getenv("AzureWebJobsStorage")
    try:
        table = TableClient.from_connection_string(connection_string, table_name="countertable")
        try:
            entity = table.get_entity("counters", "counter_0")
            val = entity["value"]
            return func.HttpResponse(f"Current counter value is {val}", status_code=200)
        except:
            return func.HttpResponse(f"problem parsing table", status_code=500)
    except:
        return func.HttpResponse(f"not able to load table as json", status_code=500)
