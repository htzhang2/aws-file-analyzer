import React, { useState } from 'react';
import axios from 'axios'; // Import Axios

// Assume your backend API URL for register is:
const REGISTER_URL = 'https://localhost:5000/api/Security/register'; 

const RegisterForm = ({ onSwitchToLogin }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

  // 6. State for Register Status and Loading
    const [statusMessage, setStatusMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const handleRegister = async (event) => {
        event.preventDefault();
        setStatusMessage('');
        setIsError(false);

        if (password !== confirmPassword)
        {
          setIsError(true);
          setStatusMessage('Error: Passwords do not match.');
          return; // Stop execution if passwords don't match
        }

        setIsLoading(true);
        try {
            // 1. Make the POST request to the backend API
            const response = await axios.post(REGISTER_URL, {
                username: username, // Send username
                password: password  // Send password
            });

            if (response.status === 201 || response.status === 200) {
              setIsError(false);
              setStatusMessage('Registration successful! Redirecting to login...');
              
              // Wait a moment then switch to the Login form
              setTimeout(() => {
                onSwitchToLogin();
              }, 1500); 

            } else {
              // Handle unexpected success status codes here
              setIsError(true);
              setStatusMessage('Registration failed due to server error.');
            }
        } catch (err) {
            console.error('Registration error:', err);
            setIsError(true);
            // Use the error message from the backend if available
            const message = err.response?.data?.message || 'Registration failed. Please try again.';
            setStatusMessage(message);
        } finally {
            // 6. Stop loading regardless of success or failure
            setIsLoading(false);
        }
    };

    return (
        <form 
        onSubmit={handleRegister} 
        className="bg-white p-8 rounded-lg shadow-xl w-full max-w-sm"
      >
        <h2 className="text-3xl font-bold mb-6 text-center text-gray-800">Register</h2>
        
        {/* Error Message Display */}
        {isError && (
          <p className="p-3 mb-4 text-sm text-red-800 bg-red-100 rounded-lg">
            {statusMessage}
          </p>
        )}

        {/* Username Input Group */}
        <div className="mb-4">
          <label htmlFor="username" className="block text-gray-700 text-sm font-semibold mb-2">
            Username
          </label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Enter your username"
          />
        </div>
        
        {/* Password Input Group */}
        <div className="mb-6">
          <label htmlFor="password" className="block text-gray-700 text-sm font-semibold mb-2">
            Password
          </label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 mb-3 leading-tight focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="********"
          />
        </div>
        
        {/* Confirm Password Input Group */}
        <div className="mb-6">
          <label htmlFor="confirmPassword" className="block text-gray-700 text-sm font-semibold mb-2">
            Confirm Password
          </label>
          <input
            type="password"
            id="confirmPassword"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 mb-3 leading-tight focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="********"
          />
        </div>

        {/* Submit Button */}
        <button 
          type="submit" 
          disabled={isLoading}
          className={`
            w-full py-2 px-4 rounded-lg text-white font-semibold transition duration-300 
            ${isLoading 
              ? 'bg-gray-400 cursor-not-allowed' 
              : 'bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-4 focus:ring-blue-300'
            }
          `}
        >
          {isLoading ? 'Registering...' : 'Register'}
        </button>

        {/* Status Message */}
        {statusMessage && (
            <p className={`text-center font-medium ${isError ? 'text-red-500' : 'text-green-600'}`}>
                {statusMessage}
            </p>
        )}

        {/* Link to Login form */}
        <p className="mt-4 text-sm text-center text-gray-600">
            Already have an account?
            <a 
              href="#" 
              onClick={(e) => { 
                e.preventDefault(); 
                onSwitchToLogin(); 
              }} 
              className="ml-1 font-medium text-indigo-600 hover:text-indigo-500"
            >
              Log In
            </a>
        </p>
      </form>
  );
};

export default RegisterForm;

