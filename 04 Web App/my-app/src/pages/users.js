import React, { useState, useEffect } from 'react';
import axios from 'axios';
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Card,
  CardContent,
} from '@mui/material';
import styled from 'styled-components';
import SignalRNotifications from './SignalRNotifications';

// Define styled components
const UserContainer = styled(Container)`
  margin-top: 20px;
  text-align: center; /* Center the content */
`;

const UserTitle = styled(Typography)`
  font-size: 2.5rem;
  margin-bottom: 1.5rem;
`;

const BabyCard = styled(Card)`
  margin-bottom: 20px;
  background-color: #f2f2f2; /* Light grey background color */
  border: 2px solid #b3b3ff; /* Border color */
`;

const BabyCardContent = styled(CardContent)`
  display: flex;
  flex-direction: column;
  align-items: center;
`;

const User = () => {
  const [familyStatus, setFamilyStatus] = useState([]);
  const [familyName] = useState('kaplan'); // Replace with family name
  const [newBabyId, setNewBabyId] = useState('');
  const [newBabyName, setNewBabyName] = useState('');

  useEffect(() => {
    fetchFamilyStatus();
  }, []);

  const fetchFamilyStatus = () => {
    axios
      .get(`https://ilovemybaby.azurewebsites.net/api/getfamilystatus?family=${familyName}`)
      .then(response => {
        setFamilyStatus(response.data.status);
      })
      .catch(error => {
        console.error('Error fetching family status:', error);
      });
  };

  const handleAddBaby = () => {
    if (newBabyId && newBabyName) {
      const babyData = {
        family: familyName,
        babyname: newBabyName,
        babyid: newBabyId
      };

      axios
        .post('https://ilovemybaby.azurewebsites.net/api/addbaby', babyData)
        .then(() => {
          fetchFamilyStatus();
          setNewBabyId('');
          setNewBabyName('');
        })
        .catch(error => {
          console.error('Error adding baby:', error);
        });
    }
  };

  return (
    <UserContainer maxWidth="md">
      <UserTitle variant="h4" gutterBottom>
        Family Status for {familyName} Family
      </UserTitle>
      {familyStatus.map(status => {
        const details = status.details ? JSON.parse(status.details) : null;
        const detailsString = details
          ? `Details: location: ${details.location}, temperature: ${details.temprature}, humidity: ${details.humidity}`
          : 'Details: No data available';
        return (
          <BabyCard key={status.babyid}>
            <BabyCardContent>
              <Typography variant="h6">Baby Name: {status.babyname}</Typography>
              <Typography>Last Update: {status.lastupdate}</Typography>
              <Typography>{detailsString}</Typography>
            </BabyCardContent>
          </BabyCard>
        );
      })}
      <Box display="flex" flexDirection="column">
        <TextField
          label="Baby ID"
          value={newBabyId}
          onChange={e => setNewBabyId(e.target.value)}
          variant="outlined"
          margin="dense"
        />

        <TextField
          label="Baby Name"
          value={newBabyName}
          onChange={e => setNewBabyName(e.target.value)}
          variant="outlined"
          margin="dense"
        />
        <Button variant="contained" color="primary" onClick={handleAddBaby}>
          Add new Baby
        </Button>
      </Box>
      <SignalRNotifications />
    </UserContainer>
  );
};

export default User;
