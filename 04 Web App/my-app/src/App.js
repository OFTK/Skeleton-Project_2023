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


