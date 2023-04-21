import os
import json
import azure.functions as func

from typing import List
from azure.data.tables import TableClient


def main(events: List[func.EventHubEvent]) -> func.HttpResponse:

    connection_string = os.getenv("AzureWebJobsStorage")

    for event in events:
        try:
            with TableClient.from_connection_string(connection_string, table_name="countertable") as table:
                entity = table.get_entity("counters", "counter_0")
                counter_value = entity["value"]
                new_counter = int(counter_value) + 1
                entity["value"] = new_counter
                table.update_entity(entity)
                return func.HttpResponse(
                    json.dumps({
                        'prev-counter':counter_value,
                        'current-counter':new_counter
                    }), status_code=200)
        except:
            return func.HttpResponse(f"Something went wrong", status_code=500)
        
