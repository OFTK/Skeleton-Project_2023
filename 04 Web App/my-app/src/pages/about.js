import React from "react";
import styled from "styled-components";

const AboutContainer = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2rem;
  text-align: center;
`;

const AboutTitle = styled.h1`
  font-size: 2.5rem;
  margin-bottom: 1.5rem;
`;

const AboutText = styled.p`
  font-size: 1.2rem;
  line-height: 1.6;
`;

const About = () => {
  return (
    <AboutContainer>
      <AboutTitle>Discover Our "Find My Baby" System</AboutTitle>
      <AboutText>
        Young babies need constant care. Our device will alert all caregivers if
        the baby is not close to any of them, or if the environmental conditions
        for the baby are dangerous.
      </AboutText>
      <AboutText>
        Our project aims to help parents and caregivers track their babys
        location and environmental conditions using mobile and web apps with
        BLE connection.  It is a command and control system for parents to:
		track their babys location and environment contditions
		alert parents if their baby is left unattended, or in dangerous conditions
		notify parents of the last time the caretaker was near the baby

		</AboutText>
		<AboutText>
		The system consists of an IOT device, multiple mobile devices which have the project’s
		app installed and a web application.
      </AboutText>
	  <AboutText>
		With "Find My Baby," you can ensure your baby's safety
        and well-being!
		</AboutText>
	  
    </AboutContainer>
  );
};

export default About;
