const variables = require('./src/variables')

module.exports = {
  content: ['./src/**/*.{js,jsx,ts,tsx,md,mdx}'],
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
            strong: {
              color: variables.colors.light['100']
            },
            h1: {
              color: variables.colors.light['100']
            },
            h2: {
              color: variables.colors.light['100'],
              marginTop: '2rem',
              marginBottom: '0.5rem'
            },
            h3: {
              marginTop: '1.5rem',
              color: variables.colors.light['100']
            },
            h4: {
              marginTop: '1rem',
              color: variables.colors.light['100']
            },
            p: {
              marginTop: '0.5rem',
              marginBottom: '1rem'
            },
            hr: {
              borderColor: variables.colors.dark['100'],
              marginTop: '2rem',
              marginBottom: '2rem'
            },
            pre: {
              backgroundColor: variables.colors.dark['300'],
              paddingTop: 0,
              paddingRight: 0,
              paddingBottom: 0,
              paddingLeft: 0
            },
            code: {
              color: variables.colors.light['100']
            },
            'code::before': {
              content: '""',
            },
            'code::after': {
              content: '""',
            },
            table: {
              fontSize: '1em'
            },
            thead: {
              borderBottomColor: variables.colors.dark['200']
            },
            'thead th': {
              color: variables.colors.light['100'],
            },
            'tbody tr': {
              borderBottomColor: variables.colors.dark['200']
            }
          },
        },
      }
    },
  },
  variants: {
    extend: {
      textColor: ['visited'],
      fill: ['hover', 'visited']
    },
  },
  plugins: [
    require('@tailwindcss/typography')
  ],
}
