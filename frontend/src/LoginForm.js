import React, { useState } from 'react';
import axios from 'axios'; // Import Axios

// Assume your backend API URL for login is:
const LOGIN_URL = 'https://localhost:5000/api/Security/login'; 

const LoginForm = ({ onLoginSuccess, onSwitchToRegister }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [isLoading, setIsLoading] = useState(false); // For button state
    const [error, setError] = useState(''); // For error messages

    const handleSubmit = async (event) => {
        event.preventDefault();
        setError(''); // Clear previous errors
        setIsLoading(true); // Start loading

        try {
            // 1. Make the POST request to the backend API
            const response = await axios.post(LOGIN_URL, {
                username: username, // Send username
                password: password  // Send password
            });

            // 2. Assuming a successful response (status 200/201)
            // The token is usually returned in the response data
            const authToken = response.data.accessToken;

            // 3. Store the token (for persistence and future API calls)
            localStorage.setItem('authToken', authToken);

            // 4. Notify the parent component of success
            onLoginSuccess();
        } catch (err) {
            // 5. Handle errors (e.g., wrong credentials, network issue)
            console.error('Login failed:', err);
            // Display a user-friendly error message
            if (err.response) {
                // The request was made and the server responded with a status code
                // that falls out of the range of 2xx (e.g., 401 Unauthorized)
                setError(err.response.data.message || 'Invalid username or password.');
            } else {
                // Something else happened while setting up the request (e.g., network error)
                setError('Login failed. Please check your network connection.');
            }
        } finally {
            // 6. Stop loading regardless of success or failure
            setIsLoading(false);
        }
    };

    return (
        <form 
        onSubmit={handleSubmit} 
        className="bg-white p-8 rounded-lg shadow-xl w-full max-w-sm"
      >
        <h2 className="text-3xl font-bold mb-6 text-center text-gray-800">Log In</h2>
        
        {/* Error Message Display */}
        {error && (
          <p className="p-3 mb-4 text-sm text-red-800 bg-red-100 rounded-lg">
            {error}
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
          {isLoading ? 'Processing...' : 'Log In'}
        </button>

        {/* Link to register form */}
        <p className="mt-4 text-sm text-center text-gray-600">
            Don't have an account?
            <a 
              href="#" 
              onClick={(e) => { 
                e.preventDefault(); 
                onSwitchToRegister(); 
              }} 
              className="ml-1 font-medium text-indigo-600 hover:text-indigo-500"
            >
              Register
            </a>
        </p>
      </form>
  );
};

export default LoginForm;

