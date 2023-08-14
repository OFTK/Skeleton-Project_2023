import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Box, Container, Typography, TextField, Button } from '@mui/material';
import SignalRNotifications from './SignalRNotifications'; 

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
                babyid: newBabyId
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

    const handleDeleteBaby = (babyId) => {
        if (selectedFamily && babyId) {
            axios
                .post('https://ilovemybaby.azurewebsites.net/api/deletebaby', {
                    family: selectedFamily,
                    babyid: babyId
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
        <Container maxWidth="md" style={{ marginTop: '20px' }}>
            <Typography variant="h4" gutterBottom>
                Administration Page for System Administrators
            </Typography>
            <ul style={{ listStyle: 'none', padding: 0, display: 'flex', justifyContent: 'center' }}>
    {familyNames && familyNames.family_names.map((familyName, index) => (
        <li key={index}>
            <Button
                variant="outlined"
                onClick={() => setSelectedFamily(familyName)}
                style={{
                    marginBottom: '10px',
                    marginRight: '10px',
                    backgroundColor: '#1976d2',
                    color: 'white',
                    border: 'none',
                    textTransform: 'none',
                }}
            >
                {familyName}
            </Button>
        </li>
    ))}
</ul>


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
                            <Box key={status.babyid} border={1} p={2} marginBottom={2}>
                                <Typography variant="h6">Baby Name: {status.babyname}</Typography>
                                <Typography>Last Update: {status.lastupdate}</Typography>
                                <Typography>{detailsString}</Typography>
                                <Button variant="contained" color="secondary" onClick={() => handleDeleteBaby(status.babyid)}>
                                    Delete Baby
                                </Button>
                            </Box>
                        );
                    })}
                    <Box display="flex" flexDirection="column">
                        <TextField
                            label="Baby ID"
                            value={newBabyId}
                            onChange={e => setNewBabyId(e.target.value)}
                            variant="outlined"
                            margin="dense" />

                        <TextField
                            label="Baby Name"
                            value={newBabyName}
                            onChange={e => setNewBabyName(e.target.value)}
                            variant="outlined"
                            margin="dense" />
                        <Button variant="contained" color="primary" onClick={handleAddBaby}>
                            Add new Baby
                        </Button>
                    </Box>
                </div>
            )} 
            <SignalRNotifications />
        </Container>
    );
}

export default Admin;
