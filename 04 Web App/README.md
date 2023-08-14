# Web App Overview

The web app has been created using the React framework with the Axios library.This web app is multi-page.

## Pages:

*1. Admin*
The Admin page stands as a control center, for system managers. administrators gain access to a  oversee all registered families. This includes the ability to monitor family activities, receive real-time SignalR alerts, and deleting and adding babies with valid UUIDs.

*2. User*
The User page is designed for individual families within the system. Users are granted selective access, allowing them to view only their own family's information. In addition, users can add new babies with valid UUID. The User page also show pop up updates through SignalR, keeping users informed about the latest updates about their family's babies.

*3. About*
A page that presents the project and our vision..

*4. Sign Up*
A page in which visitors of the web app can leave contact information and sign up to our service.


# Azure services:

the web app acceses the Azure storage tables to apdate and extract information.
it uses Azure function apps and signalR.
we also use Microsoft Azure authenticatiom to allow different premitions for different people.