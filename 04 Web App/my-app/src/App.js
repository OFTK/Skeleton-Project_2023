import React from 'react';
import './App.css';
import Navbar from './Navbar';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import About from './pages/about';
import User from './pages/users';
import SignUp from './pages/sign-up';
import Admin from './pages/admin';


function App() {
	return (
		<Router>
			<Navbar />
			<Routes>
				<Route path='/User' element={<User />} />
				<Route path='/Admin' element={<Admin />} />
				<Route path='/About' element={<About />} />
				<Route path='/sign-up' element={<SignUp />} />
			</Routes>
		</Router>
	);
}

export default App;



// import React, { useState, useEffect } from 'react';
// import axios from 'axios';
// import { Box, Container, Typography, TextField, Button } from '@mui/material';

// function App() {
//   const [familyStatus, setFamilyStatus] = useState([]);
//   const [familyName] = useState('family'); // Replace with family name
//   const [newBabyId, setNewBabyId] = useState('');
//   const [newBabyName, setNewBabyName] = useState('');

//   useEffect(() => {
//     fetchFamilyStatus();
//   }, );//[]

//   const fetchFamilyStatus = () => {
//     axios
//       .get(`https://ilovemybaby.azurewebsites.net/api/getfamilystatus?family=${familyName}`)
//       .then(response => {
//         setFamilyStatus(response.data.status);
//       })
//       .catch(error => {
//         console.error('Error fetching family status:', error);
//       });
//   };

//   const handleAddBaby = () => {
//     if (newBabyId && newBabyName) {
//       const babyData = {
//         family: familyName,
//         babyname: newBabyName,
//         babyid: newBabyId
//       };

//       axios
//         .post('https://ilovemybaby.azurewebsites.net/api/addBaby', babyData)
//         .then(() => {
//           fetchFamilyStatus();
//           setNewBabyId('');
//           setNewBabyName('');
//         })
//         .catch(error => {
//           console.error('Error adding baby:', error);
//         });
//     }
//   };

//   return (
//     <Container maxWidth="md" style={{ marginTop: '20px' }}>
//       <Typography variant="h4" gutterBottom>
//         Family Status for {familyName} Family
//       </Typography>
//       {familyStatus.map(status => (
//           <Box key={status.babyid} border={1} p={2} marginBottom={2}>
//             <Typography variant="h6">Baby Name: {status.babyname}</Typography>
//             <Typography>Last Update: {status.lastupdate}</Typography>
//             <Typography>Details: (location: {status.details})</Typography>
//           </Box>
//         ))}
//       <Box display="flex" flexDirection="column">
//         <TextField
//           label="Baby ID"
//           value={newBabyId}
//           onChange={e => setNewBabyId(e.target.value)}
//           variant="outlined"
//           margin="dense"
//         /> 


//         <TextField
//           label="Baby Name"
//           value={newBabyName}
//           onChange={e => setNewBabyName(e.target.value)}
//           variant="outlined"
//           margin="dense"
//         />
//         <Button variant="contained" color="primary" onClick={handleAddBaby}>
//           Add new Baby
//         </Button>
        
//       </Box>
//     </Container>
//   );
// }

// export default App;
