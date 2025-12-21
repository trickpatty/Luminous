# ADR-002: React as UI Framework

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Technology, Architecture

## Context

Luminous requires a UI framework that can power:
- A desktop/embedded display application (Electron or Tauri)
- iOS and Android mobile applications
- A responsive web application
- A caregiver web portal

We need a framework that enables maximum code sharing while delivering native-feeling experiences on each platform.

## Decision Drivers

- **Cross-platform code sharing**: Maximize reuse of UI components and logic
- **Performance**: Smooth 60fps rendering on constrained hardware (Raspberry Pi)
- **Component model**: Support for building a design system
- **Mobile support**: Native-quality mobile applications
- **Ecosystem**: Available libraries for calendar, drag-drop, accessibility
- **Stability**: Long-term support and active maintenance

## Considered Options

### Option 1: React + React Native

React for web/desktop, React Native for mobile.

**Pros:**
- Largest ecosystem and community
- Excellent TypeScript support
- React Native provides near-native mobile performance
- Huge selection of UI libraries
- Stable and well-documented
- Share business logic and some UI code

**Cons:**
- Not 100% code sharing between web and native
- React Native requires platform-specific code for some features
- Large bundle size for web

### Option 2: Flutter

Cross-platform UI framework using Dart.

**Pros:**
- True single codebase for all platforms
- Excellent performance
- Built-in widget library
- Good for custom designs

**Cons:**
- Dart instead of TypeScript (see ADR-001)
- Smaller ecosystem for web
- Different paradigm than web development
- Web performance not as optimized

### Option 3: Vue + Capacitor

Vue.js for web, wrapped in Capacitor for mobile.

**Pros:**
- Single web-based codebase
- Simpler mental model than React
- Good TypeScript support

**Cons:**
- Mobile apps are essentially webviews (less native feel)
- Performance limitations on low-end devices
- Smaller ecosystem than React
- Less suitable for always-on display

### Option 4: Svelte + SvelteKit

Svelte for web with SvelteKit.

**Pros:**
- Excellent performance (compile-time optimization)
- Smaller bundle size
- Simple syntax

**Cons:**
- No mature mobile solution
- Smaller ecosystem
- Less proven at scale
- TypeScript support is good but not as mature

## Decision

We will use **React** for web and desktop applications, and **React Native** for mobile applications.

## Rationale

React + React Native provides the best balance of our decision drivers:

1. **Code sharing**: While not 100%, we can share:
   - All domain logic and state management
   - Business validation and rules
   - Design tokens and theming
   - API and data layer code
   - Some UI components with react-native-web

2. **Performance**: React's virtual DOM and React Native's native bridges provide sufficient performance for our use cases, including the Raspberry Pi display.

3. **Ecosystem**: React has the largest selection of libraries for our needs:
   - Calendar components
   - Drag-and-drop for task management
   - Accessibility helpers
   - Animation libraries

4. **Stability**: React is maintained by Meta with a strong commitment to backwards compatibility and clear upgrade paths.

5. **Developer availability**: React is the most popular UI framework, maximizing our contributor pool.

## Consequences

### Positive

- Access to the largest UI ecosystem
- Well-understood patterns and best practices
- Excellent tooling and debugging
- Large talent pool for contributions
- Strong TypeScript integration

### Negative

- Platform-specific code required for some mobile features
- Need to maintain separate but similar codebases for web/native
- React Native has some quirks and requires native knowledge for advanced features
- Learning curve for React Native-specific patterns

### Neutral

- Must decide on state management solution (Zustand, Redux, etc.)
- Design system needs to account for web vs native differences
- Testing requires different approaches for web and native

## Implementation Notes

- Use React 19 features where beneficial
- Consider `react-native-web` for maximum component sharing
- Build design system primitives that abstract platform differences
- Use Zustand for state management (lightweight, TypeScript-first)
- Implement component variants for display (large touch) vs mobile

## Related Decisions

- [ADR-001: TypeScript as Primary Language](./ADR-001-typescript-primary-language.md)
- [ADR-003: Local-First Data Architecture](./ADR-003-local-first-architecture.md)

## References

- [React Documentation](https://react.dev/)
- [React Native Documentation](https://reactnative.dev/)
- [React Native Web](https://necolas.github.io/react-native-web/)
