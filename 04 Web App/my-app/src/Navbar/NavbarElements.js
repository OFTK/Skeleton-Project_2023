import { NavLink as Link } from "react-router-dom";
import styled from "styled-components";

export const Nav = styled.nav`
  background: linear-gradient(135deg, #ffb3ff, #ff66b3);
  height: 85px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem calc((100vw - 1000px) / 2);
  z-index: 12;
  box-shadow: 0px 2px 6px rgba(0, 0, 0, 0.1);
`;

export const NavLink = styled(Link)`
  color: #808080;
  display: flex;
  align-items: center;
  text-decoration: none;
  padding: 0 1rem;
  height: 100%;
  cursor: pointer;
  font-weight: bold;
  transition: color 0.3s ease-in-out;

  &:hover {
    color: #4d4dff;
  }

  &.active {
    color: #4d4dff;
  }
`;

export const NavMenu = styled.div`
  display: flex;
  align-items: center;
  margin-right: -1rem;
  /* Second Nav */
  /* margin-right: 24px; */
  /* Third Nav */
  /* width: 100vw;
  white-space: nowrap; */
  @media screen and (max-width: 768px) {
    display: none;
  }
`;
