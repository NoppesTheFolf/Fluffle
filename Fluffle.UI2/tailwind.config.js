const variables = require('./src/variables')

module.exports = {
  purge: ['./src/**/*.{js,jsx,ts,tsx}'],
  darkMode: false,
  theme: {
    colors: variables.colors,
    fill: variables.colors,
    extend: {
      typography: {
        DEFAULT: {
          css: {
            color: variables.colors.light['100'],
            lineHeight: 1.6,
            a: {
              color: variables.colors.info.DEFAULT,
              textDecoration: null,
              '&:hover': {
                color: variables.colors.info.light
              },
            },
            h1: {
              color: variables.colors.light['100'],
            },
            h2: {
              color: variables.colors.light['100'],
              marginTop: '2rem',
              marginBottom: '0.5rem'
            },
            p: {
              marginTop: '0.5rem',
              marginBottom: '1rem'
            }
          },
        },
      }
    },
  },
  variants: {
    extend: {
      fill: ['hover']
    },
  },
  plugins: [
    require('@tailwindcss/typography')
  ],
}
