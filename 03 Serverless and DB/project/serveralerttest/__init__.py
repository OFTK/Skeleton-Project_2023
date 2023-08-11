import logging
import logging
import requests
import azure.functions as func
import json


def main(req: func.HttpRequest, signalRMessages: func.Out[str]) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        req_body = req.get_json()
    except ValueError:
        return func.HttpResponse(
                "request body must be json",
                status_code=400
            )
    
    # example alert_list:
    # [
    #     {
    #         "babyid": "1",
    #         "babyname": "ofek",
    #         "alert reason": "baby status not updated for 5 minutes"
    #     },
    #     {
    #         "babyid": "2",
    #         "babyname": "nimrod",
    #         "alert reason": "baby status not updated for 5 minutes"
    #     }
    # ]
    alert_list = req_body

    signalRMessages.set(json.dumps({
        'target': 'babyalert',
        'arguments': req_body
    }))

    return func.HttpResponse(
        f"sending signalr message with contents: {req_body}",
        status_code=200
    )
