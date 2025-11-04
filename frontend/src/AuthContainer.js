import React, { useState } from 'react';
import LoginForm from './LoginForm';      // Your existing component
import RegisterForm from './RegisterForm'; // A new component you will create

function AuthContainer({ onLoginSuccess }) {
  // State to toggle between 'login' and 'register' view
  const [currentView, setCurrentView] = useState('login'); 

  const switchToRegister = () => {
    setCurrentView('register');
  };

  const switchToLogin = () => {
    setCurrentView('login');
  };

  return (
    <>
      {currentView === 'login' ? (
        // --- Display Login Form ---
        <LoginForm 
          onLoginSuccess={onLoginSuccess}
          // Pass the function to switch to Register
          onSwitchToRegister={switchToRegister} 
        />
      ) : (
        // --- Display Register Form ---
        <RegisterForm 
          // Pass the function to switch back to Login
          onSwitchToLogin={switchToLogin} 
        />
      )}
    </>
  );
}

export default AuthContainer;