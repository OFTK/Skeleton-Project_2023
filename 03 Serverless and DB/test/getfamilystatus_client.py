import requests

def send_getfamilystatus_request(family: str, url: str):
    request_url =  f"{url}"
    PARAMS = {'family': family}
    print(f"sending get request to: {request_url} with params f{PARAMS}")
    response = requests.get(url=url, params=PARAMS)
    print(response, response.text)


def main():
    localurl_base = "http://localhost:7071"
    remoteurl_base = "https://ilovemybaby.azurewebsites.net"
    send_getfamilystatus_request("tikotzky", f"{remoteurl_base}/api/getfamilystatus")

if __name__ == '__main__':
    main()
