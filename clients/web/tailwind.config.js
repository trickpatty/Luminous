/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        // Canvas colors (time-adaptive background)
        canvas: {
          DEFAULT: 'var(--canvas)',
          dawn: 'var(--canvas-dawn)',
          morning: 'var(--canvas-morning)',
          afternoon: 'var(--canvas-afternoon)',
          evening: 'var(--canvas-evening)',
          night: 'var(--canvas-night)',
        },

        // Surface colors
        surface: {
          primary: 'var(--surface-primary)',
          secondary: 'var(--surface-secondary)',
          elevated: 'var(--surface-elevated)',
          interactive: 'var(--surface-interactive)',
          pressed: 'var(--surface-pressed)',
        },

        // Brand accent colors
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

        // Text colors
        text: {
          primary: 'var(--text-primary)',
          secondary: 'var(--text-secondary)',
          tertiary: 'var(--text-tertiary)',
          inverse: 'var(--text-inverse)',
          'on-color': 'var(--text-on-color)',
        },

        // Border colors
        border: {
          DEFAULT: 'var(--border-color)',
          light: 'var(--border-color-light)',
          strong: 'var(--border-color-strong)',
        },

        // Semantic colors
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

        // Family member colors
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

        // Legacy primary colors (maps to accent for backwards compatibility)
        primary: {
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
          950: 'var(--accent-900)',
        },
      },

      fontFamily: {
        sans: ['var(--font-family)'],
        mono: ['var(--font-family-mono)'],
      },

      fontSize: {
        // Display sizes (wall display)
        'display-xl': ['4.5rem', { lineHeight: '1.1', fontWeight: '600' }],
        'display-lg': ['3.5rem', { lineHeight: '1.15', fontWeight: '600' }],
        'display-md': ['2.5rem', { lineHeight: '1.2', fontWeight: '500' }],
        'display-sm': ['2rem', { lineHeight: '1.25', fontWeight: '500' }],

        // Content sizes
        'title-lg': ['1.5rem', { lineHeight: '1.3', fontWeight: '600' }],
        'title-md': ['1.25rem', { lineHeight: '1.35', fontWeight: '600' }],
        'title-sm': ['1.125rem', { lineHeight: '1.4', fontWeight: '500' }],
        'body-lg': ['1.125rem', { lineHeight: '1.5', fontWeight: '400' }],
        'body-md': ['1rem', { lineHeight: '1.5', fontWeight: '400' }],
        'body-sm': ['0.875rem', { lineHeight: '1.5', fontWeight: '400' }],
        caption: ['0.75rem', { lineHeight: '1.4', fontWeight: '500' }],
        overline: ['0.6875rem', { lineHeight: '1.3', fontWeight: '600', letterSpacing: '0.05em', textTransform: 'uppercase' }],
      },

      spacing: {
        // Design system spacing
        'space-1': 'var(--space-1)',
        'space-2': 'var(--space-2)',
        'space-3': 'var(--space-3)',
        'space-4': 'var(--space-4)',
        'space-5': 'var(--space-5)',
        'space-6': 'var(--space-6)',
        'space-8': 'var(--space-8)',
        'space-10': 'var(--space-10)',
        'space-12': 'var(--space-12)',
        'space-16': 'var(--space-16)',
        'space-20': 'var(--space-20)',

        // Touch targets
        'touch-min': 'var(--touch-min)',
        'touch-md': 'var(--touch-md)',
        'touch-lg': 'var(--touch-lg)',
        'touch-xl': 'var(--touch-xl)',

        // Legacy support
        'touch': 'var(--touch-min)',
      },

      minWidth: {
        'touch': 'var(--touch-min)',
        'touch-min': 'var(--touch-min)',
      },

      minHeight: {
        'touch': 'var(--touch-min)',
        'touch-min': 'var(--touch-min)',
      },

      borderRadius: {
        sm: 'var(--radius-sm)',
        DEFAULT: 'var(--radius-md)',
        md: 'var(--radius-md)',
        lg: 'var(--radius-lg)',
        xl: 'var(--radius-xl)',
        '2xl': 'var(--radius-2xl)',
        full: 'var(--radius-full)',
      },

      boxShadow: {
        xs: 'var(--shadow-xs)',
        sm: 'var(--shadow-sm)',
        DEFAULT: 'var(--shadow-sm)',
        md: 'var(--shadow-md)',
        lg: 'var(--shadow-lg)',
        xl: 'var(--shadow-xl)',
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

      zIndex: {
        dropdown: 'var(--z-dropdown)',
        sticky: 'var(--z-sticky)',
        fixed: 'var(--z-fixed)',
        'modal-backdrop': 'var(--z-modal-backdrop)',
        modal: 'var(--z-modal)',
        popover: 'var(--z-popover)',
        tooltip: 'var(--z-tooltip)',
        toast: 'var(--z-toast)',
      },

      animation: {
        'fade-in': 'fadeIn var(--duration-standard) var(--ease-out)',
        'fade-out': 'fadeOut var(--duration-quick) var(--ease-in)',
        'slide-up': 'slideUp var(--duration-moderate) var(--ease-out)',
        'slide-down': 'slideDown var(--duration-standard) var(--ease-in)',
        'scale-in': 'scaleIn var(--duration-standard) var(--ease-out)',
        'scale-out': 'scaleOut var(--duration-quick) var(--ease-in)',
        'task-complete': 'taskComplete var(--duration-slow) var(--ease-spring)',
        'toast-enter': 'toastEnter var(--duration-moderate) var(--ease-spring)',
        'toast-exit': 'toastExit var(--duration-standard) var(--ease-in)',
        shimmer: 'shimmer 1.5s infinite',
      },

      keyframes: {
        fadeIn: {
          from: { opacity: '0' },
          to: { opacity: '1' },
        },
        fadeOut: {
          from: { opacity: '1' },
          to: { opacity: '0' },
        },
        slideUp: {
          from: { opacity: '0', transform: 'translateY(16px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
        slideDown: {
          from: { opacity: '1', transform: 'translateY(0)' },
          to: { opacity: '0', transform: 'translateY(16px)' },
        },
        scaleIn: {
          from: { opacity: '0', transform: 'scale(0.95)' },
          to: { opacity: '1', transform: 'scale(1)' },
        },
        scaleOut: {
          from: { opacity: '1', transform: 'scale(1)' },
          to: { opacity: '0', transform: 'scale(0.95)' },
        },
        taskComplete: {
          '0%': { transform: 'scale(1)' },
          '50%': { transform: 'scale(1.1)' },
          '100%': { transform: 'scale(1)' },
        },
        toastEnter: {
          from: { opacity: '0', transform: 'translateX(100%)' },
          to: { opacity: '1', transform: 'translateX(0)' },
        },
        toastExit: {
          from: { opacity: '1', transform: 'translateX(0)' },
          to: { opacity: '0', transform: 'translateX(100%)' },
        },
        shimmer: {
          '0%': { backgroundPosition: '-200% 0' },
          '100%': { backgroundPosition: '200% 0' },
        },
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
  ],
}
