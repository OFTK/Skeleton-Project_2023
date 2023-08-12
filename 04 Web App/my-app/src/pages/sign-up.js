import React from 'react';
import styled from 'styled-components';

const SignUpContainer = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2rem;
  text-align: center;
`;

const SignUpTitle = styled.h1`
  font-size: 2.5rem;
  margin-bottom: 1.5rem;
`;

const SignUpForm = styled.form`
  display: flex;
  flex-direction: column;
  align-items: center;
`;

const FormLabel = styled.label`
  font-size: 1.2rem;
  margin-top: 1rem;
`;

const FormInput = styled.input`
  width: 100%;
  padding: 0.5rem;
  margin-top: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
`;

const FormButton = styled.button`
  margin-top: 1rem;
  padding: 0.5rem 1rem;
  background-color: #4d4dff;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1.2rem;
  cursor: pointer;
  transition: background-color 0.3s ease-in-out;

  &:hover {
    background-color: #3b3bff;
  }
`;

const SignUp = () => {
  return (
    <SignUpContainer>
      <SignUpTitle>Sign Up to Our Service!</SignUpTitle>
      <SignUpForm>
        <FormLabel>Name:</FormLabel>
        <FormInput type="text" placeholder="Enter your name" />
        <FormLabel>Email:</FormLabel>
        <FormInput type="email" placeholder="Enter your email" />
        <FormLabel>Password:</FormLabel>
        <FormInput type="password" placeholder="Enter your password" />
        <FormButton>Sign Up</FormButton>
      </SignUpForm>
    </SignUpContainer>
  );
};

export default SignUp;
