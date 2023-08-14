import requests

def send_addbaby_request(family: str, babyname: str, babyid: str, url: str):
    data = {
        'family': f'{family}',
        'babyname': f'{babyname}',
        'babyid': f'{babyid}'
    }

    print(data)
    response = requests.post(url=url, json=data)
    print(response, response.text)


def main():
    localurl_base = "http://localhost:7071"
    remoteurl_base = "https://ilovemybaby.azurewebsites.net"
    send_addbaby_request("levi", "sivan", "71933006-db61-4c41-bfaa-d374279efb65", localurl_base+"/api/addbaby")

if __name__ == '__main__':
    main()
