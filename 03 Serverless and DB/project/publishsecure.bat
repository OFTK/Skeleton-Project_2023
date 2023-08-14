:: publish serverless and DB

:: deploy storage account
@REM az storage table create --name project --connection-string DefaultEndpointsProtocol=https;AccountName=iotworkshopb0d6;AccountKey=jotKz3BQqCMegQsAkZMpOIA0f48sicw9XnnX7UYvohDWpisGveUvwgqG/M030UXyzsKs9uwYZrej+AStGxWt4A==;EndpointSuffix=core.windows.net --fail-on-exist

:: deploy functions
func azure functionapp publish ilovemybabysecure --publish-local-settings
@REM func azure functionapp logstream ilovemybaby --browser

