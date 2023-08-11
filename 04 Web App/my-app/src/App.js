import React, { useState, useEffect } from 'react';
import axios from 'axios';

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
    <div>
      <h1> Family Status for {familyName} family </h1>
      <ul>
        {familyStatus.map(status => (
          <li key={status.babyid}>
            <p>Baby Name: {status.babyname}</p>
            <p>Last Update: {status.lastupdate}</p>
            <p>Location: ({status.latitude}, {status.longtitude})</p>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default App;
