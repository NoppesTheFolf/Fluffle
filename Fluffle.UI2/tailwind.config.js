const variables = require('./src/variables')

module.exports = {
  purge: ['./src/**/*.{js,jsx,ts,tsx}'],
  darkMode: false,
  theme: {
    colors: variables.colors,
    fill: variables.colors,
    extend: {},
  },
  variants: {
    extend: {
      fill: ['hover']
    },
  },
  plugins: [],
}