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
    localurl_base = "http://localhost:7071/api/"
    remoteurl_base = "" #TODO
    send_addbaby_request("family", "ofek", "asdasas", localurl_base+"addbaby")

if __name__ == '__main__':
    main()
