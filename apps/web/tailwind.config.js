/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html}",
    "./wwwroot/**/*.html",
    "./**/*.cshtml"
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          50: "#fff5f8",
          100: "#ffe7f0",
          200: "#ffd1e2",
          300: "#fcb1cb",
          400: "#f48bb2",
          500: "#e66c9d",
          600: "#d15485",
          700: "#b2416e",
          800: "#8f3557",
          900: "#6f2943"
        },
        rosebrand: {
          50: "#fff1f4",
          100: "#ffe0e7",
          200: "#ffc1d2",
          300: "#ff97b2",
          400: "#f46a8f",
          500: "#e43e6f",
          600: "#c9285a",
          700: "#a91e4a",
          800: "#86173b",
          900: "#6b122f"
        }
      },
      fontFamily: {
        sans: ["Manrope", "ui-sans-serif", "system-ui", "sans-serif"],
        display: ["Sora", "Manrope", "ui-sans-serif", "system-ui", "sans-serif"]
      },
      boxShadow: {
        glow: "0 10px 30px rgba(192, 38, 211, 0.22)",
        soft: "0 8px 24px rgba(136, 19, 55, 0.14)"
      }
    }
  },
  plugins: []
};
