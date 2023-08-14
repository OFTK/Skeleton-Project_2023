import React from "react";
import styled from "styled-components";

const ProjectContainer = styled.div`
  display: flex;
  flex-direction: column;
  align-items: flex-start; /* Align text to the left */
  padding: 2rem;
  background-color: #f7f7f7; /* Light grey background color */
  min-height: 100vh; /* Ensure the container takes up the full viewport height */
`;

const ProjectTitle = styled.h1`
  font-size: 2.5rem;
  margin-bottom: 1.5rem;
  color: #333; /* Dark text color */
`;

const ProjectText = styled.p`
  font-size: 1.2rem;
  line-height: 1.8;
  color: #666; /* Medium text color */
  margin-bottom: 1rem;
`;

const ProjectList = styled.ul`
  list-style: none;
  padding-left: 0;
  margin-bottom: 2rem;
`;

const ProjectListItem = styled.li`
  margin-bottom: 1rem;
`;

const SubList = styled.ul`
  list-style-type: disc;
  margin-left: 1rem;
`;

const SubListItem = styled.li`
  margin-bottom: 0.5rem;
`;

const StrongText = styled.strong`
  color: #333;
`;

const Project = () => {
  return (
    <ProjectContainer>
      <ProjectTitle>Project Documentation and Details</ProjectTitle>
      <ProjectText>
        <ProjectList>
          <ProjectListItem>
            <StrongText>Mobile App:</StrongText> Description of the mobile app.
          </ProjectListItem>
          <ProjectListItem>
            <StrongText>IoT Device:</StrongText> Description of the IoT device.
          </ProjectListItem>
          <ProjectListItem>
            <StrongText>Serverless and DB:</StrongText> Description of serverless and DB.
          </ProjectListItem>
          <ProjectListItem>
            <StrongText>Web App:</StrongText> The web app has been created using the React framework with the Axios library. This web app is multi-page.
            <SubList>
              <SubListItem>
                <StrongText>Admin:</StrongText> The Admin page serves as a control center for system managers. Administrators gain access to oversee all registered families, monitor family activities, receive real-time SignalR alerts, and manage babies by deleting and adding them with valid UUIDs.
              </SubListItem>
              <SubListItem>
                <StrongText>User:</StrongText> The User page is designed for individual families within the system. Users can view their own family's information, add new babies with valid UUIDs, and receive pop-up updates through SignalR about their family's babies.
              </SubListItem>
              <SubListItem>
                <StrongText>About:</StrongText> A page that presents the project and our vision.
              </SubListItem>
              <SubListItem>
                <StrongText>Sign Up:</StrongText> A page where visitors can leave contact information and sign up for our service.
              </SubListItem>
            </SubList>
          </ProjectListItem>
        </ProjectList>
        <StrongText>Azure Services:</StrongText> The web app accesses Azure storage tables to update and extract information. It utilizes Azure Function Apps and SignalR. We also implement Microsoft Azure authentication to grant different permissions to different users.
      </ProjectText>
    </ProjectContainer>
  );
};

export default Project;
