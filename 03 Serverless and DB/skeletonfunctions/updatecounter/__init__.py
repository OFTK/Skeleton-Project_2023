import azure.functions as func
from azure.data.tables import TableClient

import os 

def main(req: func.HttpRequest) -> func.HttpResponse:
    connection_string = os.getenv("AzureWebJobsStorage")
    new_counter = req.params.get('counter')
    if not new_counter:
        try: req_body = req.get_json()
        except ValueError: pass
        else: new_counter = req_body.get('counter')

    if not new_counter:
        return func.HttpResponse("counter request param must be added", status_code=200)
    else:
        try:
            with TableClient.from_connection_string(connection_string, table_name="countertable") as table:
                entity = table.get_entity("counters", "counter_0")
                counter_value = entity["value"]
                entity["value"] = new_counter
                table.update_entity(entity)
                return func.HttpResponse(f"Previous counter Value was {counter_value}, and now is {new_counter}", status_code=200)
        except:
            return func.HttpResponse(f"Something went wrong", status_code=500)

