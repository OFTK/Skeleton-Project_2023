import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Box, Container, Typography } from '@mui/material';

function App() {
  const [familyStatus, setFamilyStatus] = useState([]);
  const familyName = 'family'; // Replace with family name

  useEffect(() => {
    // Fetch family status data from Azure Function App getfamilystatus
    axios.get(`https://ilovemybaby.azurewebsites.net/api/getfamilystatus?family=${familyName}`)
      .then(response => {
        setFamilyStatus(response.data.status);
      })
      .catch(error => {
        console.error('Error fetching family status:', error);
      });
  }, [familyName]);

  console.log('Family Status:', familyStatus);

  return (
    <Container maxWidth="md" style={{ marginTop: '20px' }}>
      <Typography variant="h4" gutterBottom>
        Family Status for {familyName} family
      </Typography>
      <Box display="flex" flexDirection="column">
        {familyStatus.map(status => (
          <Box key={status.babyid} border={1} p={2} marginBottom={2}>
            <Typography variant="h6">Baby Name: {status.babyname}</Typography>
            <Typography>Last Update: {status.lastupdate}</Typography>
            <Typography>
              Location: ({status.latitude}, {status.longitude})
            </Typography>
          </Box>
        ))}
      </Box>
    </Container>
  );
}

export default App;