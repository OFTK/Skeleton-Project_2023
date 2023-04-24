import azure.functions as func

from azure.iot.hub import IoTHubRegistryManager

CONNECTION_STRING = "HostName=SkelIotHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=WjlurNH7xSmPRFiPk2Vc2XOjAnMfQOyEIZP8eX1FUxw="
DEVICE_ID = "SkeletonDevice"


def main(req: func.HttpRequest) -> func.HttpResponse:
    registry_manager = IoTHubRegistryManager(CONNECTION_STRING)
    data = "1"
    registry_manager.send_c2d_message(DEVICE_ID, data)
