import azure.functions as func
from azure.data.tables import TableClient
import os
import json
from datetime import datetime
import logging


def main(req: func.HttpRequest,
         ) -> func.HttpResponse:
    connection_string = os.getenv("AzureWebJobsStorage")
    new_counter = req.params.get('counter')
    if not new_counter:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            new_counter = req_body.get('counter')

    if new_counter:
        try:
            with TableClient.from_connection_string(connection_string, table_name="counterTable") as table:
                entity = table.get_entity("counters", "counter_0")
                entity["value"] = new_counter
                table.update_entity(entity)
                return func.HttpResponse(f"New counter Value {new_counter}", status_code=200)
        except Exception as e:
            logging.error(e)
            return func.HttpResponse(f"Something went wrong", status_code=500)

    else:
        return func.HttpResponse(
             "please send new counter value",
             status_code=200
        )

