import requests

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
    localurl_base = "http://localhost:7071"
    remoteurl_base = "https://ilovemybaby.azurewebsites.net"
    update_baby_status("family", "nimrod", 37.12345, 38.12435, remoteurl_base+"/api/updatebabystatus")

if __name__ == '__main__':
    main()
