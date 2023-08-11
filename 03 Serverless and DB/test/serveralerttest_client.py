import requests
import json

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

def send_serveralerttest_request(signalr_message_input: json, url: str):
    print(f"sending alert message")
    response = requests.post(url=url, json=signalr_message_input)
    print(response, response.text)


def main():
    localurl_base = "http://localhost:7071"
    remoteurl_base = "https://ilovemybaby.azurewebsites.net"
    alert_list = [
        {
            "babyid": "1",
            "babyname": "ofek",
            "alertreason": "this is a test alert"
        }
    ]
    signalr_message_input = [{
        'alert_list': alert_list
    }]

    # signalr_message_input:
    # [{
    #     'alert_list': [
    #         {
    #             "babyid": "1",
    #             "babyname": "ofek",
    #             "alert reason": "baby status not updated for 5 minutes"
    #         },
    #         {
    #             "babyid": "2",
    #             "babyname": "nimrod",
    #             "alert reason": "baby status not updated for 5 minutes"
    #         }
    #     ]
    # }]
    send_serveralerttest_request(signalr_message_input=signalr_message_input, url=remoteurl_base+"/api/serveralerttest")

if __name__ == '__main__':
    main()
