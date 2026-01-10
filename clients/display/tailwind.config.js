/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        canvas: 'var(--canvas)',
        surface: {
          primary: 'var(--surface-primary)',
          secondary: 'var(--surface-secondary)',
          interactive: 'var(--surface-interactive)',
          pressed: 'var(--surface-pressed)',
          elevated: 'var(--surface-elevated)',
        },
        accent: {
          50: 'var(--accent-50)',
          100: 'var(--accent-100)',
          200: 'var(--accent-200)',
          300: 'var(--accent-300)',
          400: 'var(--accent-400)',
          500: 'var(--accent-500)',
          600: 'var(--accent-600)',
          700: 'var(--accent-700)',
          800: 'var(--accent-800)',
          900: 'var(--accent-900)',
        },
        member: {
          sky: 'var(--member-sky)',
          emerald: 'var(--member-emerald)',
          amber: 'var(--member-amber)',
          orange: 'var(--member-orange)',
          rose: 'var(--member-rose)',
          violet: 'var(--member-violet)',
          pink: 'var(--member-pink)',
          teal: 'var(--member-teal)',
        },
        success: {
          DEFAULT: 'var(--success)',
          light: 'var(--success-light)',
          dark: 'var(--success-dark)',
        },
        warning: {
          DEFAULT: 'var(--warning)',
          light: 'var(--warning-light)',
          dark: 'var(--warning-dark)',
        },
        danger: {
          DEFAULT: 'var(--danger)',
          light: 'var(--danger-light)',
          dark: 'var(--danger-dark)',
        },
        info: {
          DEFAULT: 'var(--info)',
          light: 'var(--info-light)',
          dark: 'var(--info-dark)',
        },
      },
      spacing: {
        'touch-min': 'var(--touch-min)',
        'touch-md': 'var(--touch-md)',
        'touch-lg': 'var(--touch-lg)',
        'touch-xl': 'var(--touch-xl)',
        'safe-area': '48px',
      },
      fontSize: {
        'display-xl': ['4.5rem', { lineHeight: '1.1', fontWeight: '600' }],
        'display-lg': ['3.5rem', { lineHeight: '1.15', fontWeight: '600' }],
        'display-md': ['2.5rem', { lineHeight: '1.2', fontWeight: '500' }],
        'display-sm': ['2rem', { lineHeight: '1.25', fontWeight: '500' }],
        'glanceable': ['2rem', { lineHeight: '1.3', fontWeight: '500' }],
      },
      borderRadius: {
        DEFAULT: 'var(--radius-md)',
        sm: 'var(--radius-sm)',
        md: 'var(--radius-md)',
        lg: 'var(--radius-lg)',
        xl: 'var(--radius-xl)',
        '2xl': 'var(--radius-2xl)',
      },
      boxShadow: {
        xs: 'var(--shadow-xs)',
        sm: 'var(--shadow-sm)',
        md: 'var(--shadow-md)',
        lg: 'var(--shadow-lg)',
        xl: 'var(--shadow-xl)',
      },
      fontFamily: {
        sans: 'var(--font-family)',
      },
      transitionDuration: {
        instant: 'var(--duration-instant)',
        quick: 'var(--duration-quick)',
        standard: 'var(--duration-standard)',
        moderate: 'var(--duration-moderate)',
        slow: 'var(--duration-slow)',
        deliberate: 'var(--duration-deliberate)',
      },
      transitionTimingFunction: {
        'ease-in': 'var(--ease-in)',
        'ease-out': 'var(--ease-out)',
        'ease-in-out': 'var(--ease-in-out)',
        spring: 'var(--ease-spring)',
      },
      animation: {
        'spin-slow': 'spin 3s linear infinite',
        'pulse-soft': 'pulse 2s ease-in-out infinite',
        'slide-up': 'slideUp 0.3s ease-out',
        'fade-in': 'fadeIn 0.2s ease-out',
        'scale-in': 'scaleIn 0.2s ease-out',
      },
      keyframes: {
        slideUp: {
          '0%': { opacity: '0', transform: 'translateY(16px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        scaleIn: {
          '0%': { opacity: '0', transform: 'scale(0.95)' },
          '100%': { opacity: '1', transform: 'scale(1)' },
        },
      },
    },
  },
  // Plugins are now imported in styles.scss using @plugin directive (Tailwind v4)
  plugins: [],
};
