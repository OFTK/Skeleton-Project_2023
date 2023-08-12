// import React from 'react';
// import ReactDOM from 'react-dom';
// import App from './App';
// import CssBaseline from '@mui/material/CssBaseline';
// import { ThemeProvider, createTheme } from '@mui/material/styles';

// // Define a custom theme
// const theme = createTheme();

// ReactDOM.render(
//   <ThemeProvider theme={theme}>
//     <CssBaseline />
//     <App />
//   </ThemeProvider>,
//   document.getElementById('root')
// );

import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import App from "./App";
import reportWebVitals from "./reportWebVitals";

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);

reportWebVitals();