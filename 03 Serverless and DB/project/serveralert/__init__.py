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
    timediff = datetime.now() - datetime.fromisoformat(status.get("lastupdate"))
    if timediff > timedelta(minutes=1):
        return f"baby status not updated for {timediff}"

    # if everything is ok - return None (no alert)
    else: return None


def main(mytimer: func.TimerRequest, signalRMessages: func.Out[str]) -> None:

    utc_timestamp = datetime.now().isoformat()
    logging.info('invoked serveralert at %s', utc_timestamp)
    if mytimer.past_due:
        logging.warning('The timer is past due, might wanna check what happened')

    # read family status from database - query getfamilystatus pyton function
    family = "family"
    base_url = "http://localhost:7170"
    # base_url = "https://skeletonfunctionapp.azurewebsites.net"
    getfamilystatus_url = base_url + "/api/getfamilystatus"
    family_status = request_getfamilystatus(family, getfamilystatus_url)('status')

    # create a data structure of all babies that are in danger
    alert_list = []
    for baby in family_status:
        reason = alert_condition(baby)
        if reason: # return string isn't null
            alert = {}
            alert["babyid"] = baby.get("babyid")
            alert["babyname"] = baby.get("babyname")
            alert["alert reason"] = reason
        alert_list.append(alert)

    # send signalr to all of the family members
    signalRMessages.set(json.dumps({
        'target': 'babyalert',
        'alerts': alert_list
    }))
