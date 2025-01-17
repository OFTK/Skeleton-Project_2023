import azure.functions as func


def main(req: func.HttpRequest, connectionInfo) -> func.HttpResponse:
    return func.HttpResponse(
        connectionInfo,
        status_code=200,
        headers={
            'Content-type': 'application/json'
        }
    )