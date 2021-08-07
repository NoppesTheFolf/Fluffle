const colors = require('tailwindcss/colors');

module.exports = {
  purge: ['./src/**/*.{js,jsx,ts,tsx}'],
  darkMode: false,
  theme: {
    colors: {
      transparent: 'transparent',
      black: colors.black,
      white: colors.white,
      rose: colors.rose,
      pink: colors.pink,
      fuchsia: colors.fuchsia,
      purple: colors.purple,
      violet: colors.violet,
      indigo: colors.indigo,
      blue: colors.blue,
      sky: colors.sky,
      cyan: colors.cyan,
      teal: colors.teal,
      emerald: colors.emerald,
      green: colors.green,
      lime: colors.lime,
      yellow: colors.yellow,
      amber: colors.amber,
      orange: colors.orange,
      red: colors.red,
      warmGray: colors.warmGray,
      trueGray: colors.trueGray,
      gray: colors.gray,
      coolGray: colors.coolGray,
      blueGray: colors.blueGray,
      primary: {
        light: '#e67e22',
        DEFAULT: '#d35400'
      },
      info: {
        light: '#3498db',
        DEFAULT: '#2980b9'
      },
      danger: {
        light: '#e74c3c',
        DEFAULT: '#c0392b'
      },
      success: {
        light: '#27af61',
        DEFAULT: '#239e57'
      },
      warning: {
        light: '#e67e22',
        DEFAULT: '#d35400'
      },
      dark: {
        100: '#5e5e64',
        200: '#36393f',
        300: '#202024',
        400: '#16161a',
        500: '#0e0e11'
      },
      light: {
        100: '#dcdcdc',
        200: '#c6c6c6'
      },
      muted: '#969696'
    },
    extend: {},
  },
  variants: {
    extend: {},
  },
  plugins: [],
}
