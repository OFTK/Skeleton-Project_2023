from typing import List
import logging

import azure.functions as func
from azure.data.tables import TableClient

import os 



def main(events: List[func.EventHubEvent]):
    connection_string = os.getenv("AzureWebJobsStorage")
    for event in events:
        logging.info('Python EventHub trigger processed an event: %s',event.get_body().decode('utf-8'))
        try:
            with TableClient.from_connection_string(connection_string, table_name="countertable") as table:
                entity = table.get_entity("counters", "counter_0")
                counter_value = entity["value"]
                entity["value"] = int(counter_value)+1
                table.update_entity(entity)
        except:
            logging.info('something terrible happened trying to access storage')
