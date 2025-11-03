import React, { useState, useEffect } from "react";
import FileUploadAnalyzer from "./FileUploadAnalyze";
import AuthContainer from "./AuthContainer";

export default function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isCheckingToken, setIsCheckingToken] = useState(true); // New state for initial load

  // --- Initial Check (Login Persistence) ---
  useEffect(() => {
    // Check if a token exists in local storage when the app first loads
    const token = localStorage.getItem('authToken');
    if (token) {
      // Optionally: You should also validate this token against your backend
      // For simplicity, we assume if a token exists, the user is logged in
      setIsLoggedIn(true);
    }
    setIsCheckingToken(false); // Done checking
  }, []);

  // Handler passed to the LoginForm
  const handleSuccessfulLogin = () => {
    setIsLoggedIn(true);
  };

  // Handler for Log Out
  const handleLogout = () => {
    localStorage.removeItem('authToken'); // Clear the stored token
    setIsLoggedIn(false);
  };

  // Show a loading screen while checking for a token
  if (isCheckingToken) {
      return <div>Loading Application...</div>;
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-100 p-6">
      {/* Conditional Rendering: Show UI only if logged in */}
      {isLoggedIn ? (
        <FileUploadAnalyzer handleLogout={handleLogout}/>
      ) : (
        <AuthContainer onLoginSuccess={handleSuccessfulLogin} />
      )}
    </div>
  );
}
