# where is my tinoki 
an IoT system to monitor the whereabouts of your tinokis, and see if their caretaker leaves them for too long.

## version
this is the beta version, that implements the basic flow of seeing live updates of the user's babies location.

## [Mobile App](https://github.com/OFTK/Skeleton-Project_2023/tree/master/01%20Mobile%20App)
implements 3 flows:
- seeing live updates of the user babies. When adding another baby a signalr message causes the view to refresh and the baby to be added.
- seeing alerts (that are raised via a signalr message), for when a baby is unhandled for too long.
- in the background - sending updates on babies that are within bluetooth range.

More details in the README file inside the [Mobile App](https://github.com/OFTK/Skeleton-Project_2023/tree/master/01%20Mobile%20App) folder.

### Setup
In order to build and run the app in Visual Studio 2019
* Install [Visual 2019 Studio Community Edition](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&rel=16&src=myvs&utm_medium=microsoft&utm_source=my.visualstudio.com&utm_campaign=download&utm_content=vs+community+2019)
* Install [.NET SDK version 5.0](https://download.visualstudio.microsoft.com/download/pr/14ccbee3-e812-4068-af47-1631444310d1/3b8da657b99d28f1ae754294c9a8f426/dotnet-sdk-5.0.408-win-x64.exe
)

## [IoT Device](https://github.com/OFTK/Skeleton-Project_2023/tree/master/02%20IoT%20Device)
In this exercise, we are going to learn about IoT Hub, the device SDK and how to send data from the device to the cloud when the device user is pressing a button.

## [Serverless and DB](https://github.com/OFTK/Skeleton-Project_2023/tree/master/03%20Serverless%20and%20DB)
More details in the README inside the [Serverless and DB](https://github.com/OFTK/Skeleton-Project_2023/tree/master/03%20Serverless%20and%20DB) folder.

## [WebApp](https://github.com/OFTK/Skeleton-Project_2023/tree/master/04%20Web%20App)
In this exercise, we are going to create a web app, connect it to our IoT Hub and send messages from it to specific devices, which in turn will increase the counter.
