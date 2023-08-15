import React from "react";
import styled from "styled-components";

const ProjectContainer = styled.div`
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  padding: 2rem;
  background-color: #f7f7f7;
  min-height: 100vh;
`;

const ProjectTitle = styled.h1`
  font-size: 2.5rem;
  margin-bottom: 1.5rem;
  color: #333;
`;

const ProjectText = styled.p`
  font-size: 1.2rem;
  line-height: 1.8;
  color: #666;
  margin-bottom: 1rem;
`;

const SubHeader = styled.h2`
  font-size: 1.8rem;
  margin-top: 2rem;
  color: #333;
`;

const ListItem = styled.li`
  margin-bottom: 1rem;
  font-size: 1.1rem;
`;

const SubList = styled.ul`
  list-style-type: disc;
  margin-left: 1rem;
`;

const StrongText = styled.strong`
  color: #333;
`;

const Project = () => {
  return (
    <ProjectContainer>
      <ProjectTitle>Project Overview</ProjectTitle>
      <ProjectText>
         Where is My Tinoki
        An IoT system to monitor the whereabouts of your tinokis and see if their caretaker leaves them for too long.
        In each folder, there is a README file for that part of the project.
      </ProjectText>

      <SubHeader>Mobile App</SubHeader>
      <ProjectText>
        <StrongText>General Information:</StrongText>
        This app collects data continuously from surrounding BabyTags. It collects the data using BLE (Bluetooth Low Energy) and uploads it to the serverless DB (if the device is not connected to WiFi). It can also connect a BabyTag to WiFi; see instructions below on how to do so.
      </ProjectText>
      <ProjectText>
        <StrongText>Connection with the IoT Device:</StrongText>
        Establishing a Wi-Fi connection for a BabyTag involves transmitting Wi-Fi credentials (SSID, Password) securely via the mobile app. These credentials need to be given manually. After credentials are given, in order to connect a BabyTag, you need to scan its QR Code. If the BabyTag is recognized as a new one by the system, you would be asked to register a new name for this new baby. The password is transmitted securely (Using AES128 symmetric encryption) to the BabyTag.
      </ProjectText>

      <SubHeader>IoT Device</SubHeader>
      <ProjectText>
        <StrongText>General Information:</StrongText>
        The IoT Device serves as a versatile BLE server while also offering connectivity through Wi-Fi to the internet. This device is equipped with a BLE service which is assigned the unique identifier (UUID) of each baby. This UUID is acquired by scanning the dedicated QR Code accompanying the babytag. The service is openly accessible for connection.
      </ProjectText>
      <ProjectText>
        <StrongText>BLE Data Points:</StrongText>
        The IoT Device broadcasts two essential "baby status" parameters via BLE: temperature and humidity. Each of these parameters is encapsulated within a BLE characteristic, both sharing a universal UUID across all BabyTag devices:
        - Temperature UUID: "f96c20eb-05c7-4c31-803b-03428eae9aa2"
        - Humidity UUID: "2d4fa781-cf1c-4ea1-9427-14951f794d80"
        Data readings are sourced from a BME280 sensor, with the device initiating sampling with every BLE read event.
      </ProjectText>
      <ProjectText>
        <StrongText>Wi-Fi Connectivity:</StrongText>
        Establishing a Wi-Fi connection for the BabyTag involves transmitting Wi-Fi credentials (SSID, Password) via the mobile app. These credentials are relayed through BLE characteristics, each distinguished by a unique UUID:
        - SSID UUID: "28919cc6-36d5-11ee-be56-0242ac120002"
        - Password UUID: "02350feb-8302-4ff7-8f04-9e07f69d73df"
        The SSID is conveyed as plaintext, while the password is securely transferred in ciphertext. Refer to the mobile app's README.md for comprehensive instructions on how to transmit those.
      </ProjectText>
      <ProjectText>
        <StrongText>Security Measures:</StrongText>
        To ensure the privacy and integrity of sensitive data exchanged between the BabyTag and the mobile app, security measures are implemented. Sensitive data (in the current version consists of only the Wi-Fi password) undergoes AES128 encryption. For encryption purposes, the device generates a 16-byte true random string referred to as "sync," accessible via a dedicated BLE characteristic:
        - Sync UUID: "3b4fa77b-bb0b-4b12-8ee6-913382a4f2a0"
        Additionally, a symmetric key, essential for AES128 encryption, is shared between the IoT device and the mobile app. The mobile app retrieves this key by scanning the QR Code associated with the device.
        While connected to Wi-Fi, the BabyTag ensures the secure transmission of all "baby status" parameters through HTTPS.
        For more details and comprehensive usage guidelines, please refer to the mobile app's documentation.
      </ProjectText>

      <SubHeader>Serverless and DB</SubHeader>
      <ProjectText>
        <StrongText>Storage Account - General Information:</StrongText>
        The URL to the table endpoint is: <a href="https://skeletonwebjobsstorage.table.core.windows.net/" target="_blank" rel="noopener noreferrer">https://skeletonwebjobsstorage.table.core.windows.net/</a>
      </ProjectText>
      <ProjectText>
        <StrongText>Storage Design:</StrongText>
        first tables family name as partition key and baby name as row key. 
        the rest of the columns holds babyid last update latitude and longitude.

      </ProjectText>
      <ProjectText>
        <StrongText>Second Storage Table - "Families":</StrongText>
        This table stores all family names of the system users. The partition key is identical for all families for convenience of accessing the table while the row key is a unique family name.
        DateTime is datetime of format iso 8601. To create that in python, use:
        ```python
        from datetime import datetime
        current_time_in_iso_format = datetime.now().isoformat()
        ```
      </ProjectText>
      <ProjectText>
        <StrongText>Function API:</StrongText>
        For every function, there is a simple python client in the test folder.
      </ProjectText>

      <SubHeader>Web App Overview</SubHeader>
      <ProjectText>
        The web app has been created using the React framework with the Axios library. This web app is multi-page.
      </ProjectText>
      <ProjectText>
        <StrongText>Pages:</StrongText>
      </ProjectText>
      <SubList>
        <ListItem>
          <StrongText>Admin:</StrongText> The Admin page stands as a control center for system managers. Administrators gain access to oversee all registered families. This includes the ability to monitor family activities, receive real-time SignalR alerts, and delete and add babies with valid UUIDs.
        </ListItem>
        <ListItem>
          <StrongText>Map:</StrongText> The Map page offers a geographic display of BabyTag locations. It shows the live location of all babies connected to the system and presents their most recent "baby status" data, namely temperature and humidity levels. The map is implemented using the Google Maps API.
        </ListItem>
        <ListItem>
          <StrongText>Family:</StrongText> The Family page provides a detailed overview of a particular family's registered babies. It displays each baby's current status (temperature and humidity), along with a live feed of their activities. Parents can view their babies' historical activity and receive alerts if any concerning activity patterns are detected.
        </ListItem>
      </SubList>
      <ProjectText>
        <StrongText>SignalR Integration:</StrongText>
        The web app employs SignalR, a real-time communication library, to enable live notifications for system administrators. Alerts are generated when a predefined activity threshold is exceeded (e.g., if a baby's temperature falls below a critical value). Admins receive these alerts in real-time.
      </ProjectText>
      <ProjectText>
        <StrongText>Azure Services:</StrongText>
        The web app accesses the Azure storage tables to update and extract information. It uses Azure Function Apps and SignalR. We also use Microsoft Azure authentication to allow different permissions for different people.
      </ProjectText>
    </ProjectContainer>
  );
};

export default Project;
