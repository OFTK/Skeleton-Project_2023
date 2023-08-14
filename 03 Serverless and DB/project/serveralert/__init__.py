import logging
import requests
from datetime import datetime, timedelta, timezone
import azure.functions as func
import json

def request_getfamilystatus(family: str, url: str) -> dict:
    request_url =  f"{url}"
    PARAMS = {'family': family}
    response = requests.get(url=url, params=PARAMS)
    return response.json()

def alert_condition(status: dict) -> str:
    # check if update was too long ago
    if (not (status.get("lastupdate") is None)):
        timediff = datetime.now() - datetime.fromisoformat(status.get("lastupdate"))
        if timediff > timedelta(minutes=5):
            return f"baby status not updated for {timediff}"

    # if everything is ok - return None (no alert)
    return None


def main(mytimer: func.TimerRequest, signalRMessages: func.Out[str]) -> None:

    utc_timestamp = datetime.now().isoformat()
    logging.info('invoked serveralert at %s', utc_timestamp)
    if mytimer.past_due:
        logging.warning('The timer is past due, might wanna check what happened')

    # read family status from database - query getfamilystatus pyton function
    family = "family"
    # base_url = "http://localhost:7170"
    base_url = "https://ilovemybaby.azurewebsites.net"
    getfamilystatus_url = base_url + "/api/getfamilystatus"
    family_status = request_getfamilystatus(family, getfamilystatus_url)

    # create a data structure of all babies that are in danger
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
    alert_list = []
    for baby in family_status.get("status"):
        reason = alert_condition(baby)
        if reason: # return string isn't null
            alert = {}
            alert["babyid"] = baby.get("babyid")
            alert["babyname"] = baby.get("babyname")
            alert["alertreason"] = reason
            alert_list.append(alert)

    # send signalr to all of the family members
    logging.info('sending signalr message with contents: %s', alert_list)
    signalRMessages.set(json.dumps({
        'target': 'babyalert',
        'arguments': [
            {'alert_list': alert_list}
        ]
    }))
    #example for arguments field:
    # [
    #     {
    #         "alert_list": [
    #             {
    #                 "babyid": "1",
    #                 "babyname": "ofek",   
    #                 "alertreason": "baby status not updated for 5 minutes"
    #             },
    #             {
    #                 "babyid": "2",
    #                 "babyname": "nimrod",
    #                 "alertreason": "baby status not updated for 5 minutes"
    #             }
    #         ]
    #     }
    # ]
