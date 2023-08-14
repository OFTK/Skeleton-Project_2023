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
  CardActions,
} from '@mui/material';
import styled from 'styled-components';
import SignalRNotifications from './SignalRNotifications';

const AdminContainer = styled(Container)`
  margin-top: 20px;
`;

const AdminTitle = styled(Typography)`
  font-size: 2.5rem;
  margin-bottom: 1.5rem;
`;

const FamilyList = styled.ul`
  list-style: none;
  padding: 0;
  display: flex;
  justify-content: center;
`;

const FamilyListItem = styled.li`
  margin: 0 10px;
`;

const FamilyButton = styled(Button)`
  margin-bottom: 10px;
  background-color: #1976d2;
  color: white;
  border: none;
  text-transform: none;

  &:hover {
    background-color: #1565c0;
  }
`;

const BabyCard = styled(Card)`
  margin-bottom: 20px;
`;

const BabyCardContent = styled(CardContent)`
  display: flex;
  flex-direction: column;
  align-items: center;
`;

const BabyCardActions = styled(CardActions)`
  justify-content: center;
`;

function Admin() {
  const [familyNames, setFamilyNames] = useState({ family_names: [] });
  const [selectedFamily, setSelectedFamily] = useState(null);
  const [familyStatus, setFamilyStatus] = useState([]);
  const [newBabyId, setNewBabyId] = useState('');
  const [newBabyName, setNewBabyName] = useState('');

  async function fetchFamilyNames() {
    try {
      const response = await axios.get('https://ilovemybaby.azurewebsites.net/api/getallfamilies');
      setFamilyNames(response.data);
      console.log('Response:', response.data);
    } catch (error) {
      console.error('Error fetching family names:', error);
    }
  }

  async function fetchFamilyDetails(familyName) {
    try {
      const response = await axios.get(`https://ilovemybaby.azurewebsites.net/api/getfamilystatus?family=${familyName}`);
      setFamilyStatus(response.data.status);
    } catch (error) {
      console.error('Error fetching family details:', error);
    }
  }

  const handleAddBaby = () => {
    if (newBabyId && newBabyName && selectedFamily) {
      const babyData = {
        family: selectedFamily,
        babyname: newBabyName,
        babyid: newBabyId,
      };

      axios
        .post('https://ilovemybaby.azurewebsites.net/api/addBaby', babyData)
        .then(() => {
          fetchFamilyDetails(selectedFamily);
          setNewBabyId('');
          setNewBabyName('');
        })
        .catch(error => {
          console.error('Error adding baby:', error);
        });
    }
  };

  const handleDeleteBaby = babyId => {
    if (selectedFamily && babyId) {
      axios
        .post('https://ilovemybaby.azurewebsites.net/api/deletebaby', {
          family: selectedFamily,
          babyid: babyId,
        })
        .then(() => {
          fetchFamilyDetails(selectedFamily);
        })
        .catch(error => {
          console.error('Error deleting baby:', error);
        });
    }
  };

  useEffect(() => {
    fetchFamilyNames();
  }, []);

  useEffect(() => {
    if (selectedFamily) {
      fetchFamilyDetails(selectedFamily);
    }
  }, [selectedFamily]);

  return (
    <AdminContainer maxWidth="md">
      <AdminTitle variant="h4" gutterBottom>
        Administration Page for System Administrators
      </AdminTitle>
      <FamilyList>
        {familyNames &&
          familyNames.family_names.map((familyName, index) => (
            <FamilyListItem key={index}>
              <FamilyButton
                variant="outlined"
                onClick={() => setSelectedFamily(familyName)}
              >
                {familyName}
              </FamilyButton>
            </FamilyListItem>
          ))}
      </FamilyList>

      {selectedFamily && (
        <div>
          <Typography variant="h4" gutterBottom>
            Family Details for {selectedFamily} Family
          </Typography>
          {familyStatus.map(status => {
            const details = status.details ? JSON.parse(status.details) : null;
            const detailsString = details
              ? `Details: location: ${details.location}, temperature: ${details.temprature}, humidity: ${details.humidity}`
              : 'Details: No data available';
            return (
              <BabyCard key={status.babyid} variant="outlined">
                <BabyCardContent>
                  <Typography variant="h6">
                    Baby Name: {status.babyname}
                  </Typography>
                  <Typography>Last Update: {status.lastupdate}</Typography>
                  <Typography>{detailsString}</Typography>
                </BabyCardContent>
                <BabyCardActions>
                  <Button
                    variant="contained"
                    color="secondary"
                    onClick={() => handleDeleteBaby(status.babyid)}
                  >
                    Delete Baby
                  </Button>
                </BabyCardActions>
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
            <Button
              variant="contained"
              color="primary"
              onClick={handleAddBaby}
            >
              Add new Baby
            </Button>
          </Box>
        </div>
      )}
      <SignalRNotifications />
    </AdminContainer>
  );
}

export default Admin;
