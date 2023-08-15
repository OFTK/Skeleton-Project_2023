import React from "react";
import styled from "styled-components";

const AboutContainer = styled.div`
  display: flex;
  flex-direction: column;
  align-items: flex-start; /* Align text to the left */
  padding: 2rem;
  background-color: #f7f7f7; /* Light grey background color */
  min-height: 100vh; /* Ensure the container takes up the full viewport height */
`;

const AboutTitle = styled.h1`
  font-size: 2.5rem;
  margin-bottom: 1.5rem;
  color: #333; /* Dark text color */
`;

const AboutText = styled.p`
  font-size: 1.2rem;
  line-height: 1.6;
  color: #666; /* Medium text color */
  margin-bottom: 1rem;
`;


const About = () => {
  return (
    <AboutContainer>
      <AboutTitle>Discover Our "Find My Baby" System</AboutTitle>
      <AboutText>
        Young babies need constant care. Our device will alert all caregivers if
        the baby is not close to any of them or if the environmental conditions
        for the baby are dangerous.
      </AboutText>
      <AboutText>
        Our project aims to help parents and caregivers track their baby's
        location and environmental conditions using mobile and web apps with
        BLE connection.
        </AboutText>
        <AboutText>
        It is a command and control system for parents to:

      </AboutText>
      <AboutText>
        <li>Track their baby's location and environmental conditions</li>
        <li>Alert parents if their baby is left unattended or in dangerous conditions</li>
        <li>Notify parents of the last time the caretaker was near the baby</li>
      </AboutText>
      <AboutText>
        The system consists of an IoT device, multiple mobile devices with the project’s
        app installed, and a web application.
      </AboutText>
      <AboutText>
        With "Find My Baby," you can ensure your baby's safety and well-being!
      </AboutText>
    </AboutContainer>
  );
};

export default About;
