import React from "react";
import { Nav, NavLink, NavMenu }
	from "./NavbarElements";

const Navbar = () => {
	return (
		<>
			<Nav>
				<NavMenu>
					<NavLink to="/admin" activestyle>
						Admin
					</NavLink>
					<NavLink to="/user" activestyle>
						Users App
					</NavLink>
					<NavLink to="/about" activestyle>
						About Us
					</NavLink>
					<NavLink to="/sign-up" activestyle>
						Sign Up 
					</NavLink>
					<NavLink to="/project" activestyle>
						Project README 
					</NavLink>
				</NavMenu>
			</Nav>
		</>
	);
};

export default Navbar;
