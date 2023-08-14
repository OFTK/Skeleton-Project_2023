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
    send_addbaby_request("levi", "levi", "e46930f5-c36d-47a3-8fcf-b68b5216aced", localurl_base+"/api/addbaby")

if __name__ == '__main__':
    main()
