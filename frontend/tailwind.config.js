/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./public/index.html",
    "./src/**/*.{js,ts,jsx,tsx}", // <-- THIS LINE IS CRUCIAL
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}

