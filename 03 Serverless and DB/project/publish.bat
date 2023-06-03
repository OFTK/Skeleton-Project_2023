:: publish serverless and DB

:: deploy storage account
az storage table create --name project --connection-string DefaultEndpointsProtocol=https;AccountName=skeletonwebjobsstorage;AccountKey=mruuYxVNkKXUmXQnSZXiE+duyQd5Ez6I7MS88Jf0LjSmGsFNvm9LwGAy/lLFWTZ3HojulpK+MlHV+AStZqL4aw==;EndpointSuffix=core.windows.net --fail-on-exist

:: deploy functions
func azure functionapp publish skeletonfunctionapp --publish-local-settings
func azure functionapp logstream skeletonfunctionapp --browser

