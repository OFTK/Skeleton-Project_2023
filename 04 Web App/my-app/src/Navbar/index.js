import React from "react";
import { Nav, NavLink, NavMenu }
	from "./NavbarElements";

const Navbar = () => {
	return (
		<>
			<Nav>
				<NavMenu>
					<NavLink to="/admin" activeStyle>
						Admin
					</NavLink>
					<NavLink to="/user" activeStyle>
						Users App
					</NavLink>
					<NavLink to="/about" activeStyle>
						About Us
					</NavLink>
					<NavLink to="/sign-up" activeStyle>
						Sign Up 
					</NavLink>
				</NavMenu>
			</Nav>
		</>
	);
};

export default Navbar;
