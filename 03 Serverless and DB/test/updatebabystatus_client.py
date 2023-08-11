import requests

def update_baby_status(family: str, babyname: str, url: str):
    # construct a request body
    data = {
        "family": f"{family}",
        "babyname": f"{babyname}",
        "details": {
            "location": "SOME LOCATION STRING",
            "temprature": 25.5,
            "humidity": 50.0
        }
    }

    print(data)
    response = requests.post(url=url, json=data)
    print(response, response.text)


def main():
    localurl_base = "http://localhost:7071"
    remoteurl_base = "https://ilovemybaby.azurewebsites.net"
    update_baby_status("family", "ofek", remoteurl_base+"/api/updatebabystatus")

if __name__ == '__main__':
    main()
