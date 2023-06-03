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

def update_baby_status(family: str, babyname: str, longtitude: float, latitude: float, url: str):
    data = {
        'family': f'{family}',
        'babyname': f'{babyname}',
        'longtitude': f'{longtitude}',
        'latitude': f'{latitude}'
    }

    print(data)
    response = requests.post(url=url, json=data)
    print(response, response.text)


def main():
    localurl_base = "http://localhost:7071/api/"
    remoteurl_base = "" #TODO
    # send_addbaby_request("family", "ofek", "asdasas", localocalurl_base++"addbaby")
    update_baby_status("family", "ofek", 37.12345, 38.12435, localurl_base+"updatebabystatus")

main()