# ADR-002: Angular as Web Framework

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Technology

## Context

Luminous requires a web framework for:
- The main web application (accessed by family members)
- The wall-mounted display application (Angular + Electron)
- The caregiver portal

We need a framework that supports enterprise-grade applications with strong TypeScript integration.

## Decision Drivers

- **TypeScript-first**: Strong typing throughout the application
- **Enterprise features**: Built-in routing, forms, HTTP client, dependency injection
- **Performance**: Fast rendering for always-on display
- **Desktop support**: Can be packaged with Electron for display app
- **Maintainability**: Clear patterns for large applications
- **Long-term stability**: Backed by major organization with clear roadmap

## Considered Options

### Option 1: Angular

Google's enterprise web framework with TypeScript.

**Pros:**
- TypeScript-first with decorators and DI
- Complete framework (routing, forms, HTTP, DI built-in)
- Excellent enterprise adoption
- Clear upgrade path and long-term support
- Works well with Electron
- Strong testing infrastructure

**Cons:**
- Larger bundle size than some alternatives
- Steeper learning curve
- More opinionated structure

### Option 2: React

Meta's component library with TypeScript support.

**Pros:**
- Largest ecosystem
- Flexible architecture
- React Native for mobile code sharing

**Cons:**
- Not a complete framework (need many libraries)
- We're using native mobile, so no RN code sharing
- Less opinionated (can lead to inconsistency)

### Option 3: Vue

Progressive framework with TypeScript support.

**Pros:**
- Gentle learning curve
- Good TypeScript support
- Smaller bundle size

**Cons:**
- Smaller enterprise adoption
- Less mature tooling
- Fewer resources for complex apps

## Decision

We will use **Angular 19+** as the web framework for all web-based Luminous applications.

## Rationale

1. **TypeScript Integration**: Angular is built with TypeScript and uses it idiomatically with decorators, interfaces, and dependency injection.

2. **Complete Framework**: Angular includes everything needed for enterprise applications (routing, forms, HTTP, state management) without needing to assemble many libraries.

3. **Electron Compatibility**: Angular works excellently with Electron for the display application, with good patterns for main/renderer process communication.

4. **Enterprise Patterns**: Angular's dependency injection and module system align well with our Clean Architecture backend, creating a consistent codebase.

5. **Google Support**: Long-term support and clear upgrade paths from Google.

## Consequences

### Positive

- Consistent TypeScript throughout frontend
- Built-in features reduce dependency decisions
- Clear patterns for large applications
- Works well with Electron for display app
- Strong form handling for family data entry

### Negative

- Larger bundle size (mitigated by lazy loading)
- Steeper learning curve for new contributors
- No direct mobile code sharing (using native instead)

### Neutral

- Need to stay current with Angular versions
- Must follow Angular style guide for consistency

## Implementation Notes

- Use Angular 19+ with standalone components
- Configure strict TypeScript settings
- Use Angular Material or Tailwind CSS for styling
- Implement lazy loading for feature modules
- Use MSAL Angular for Azure AD B2C authentication

## Related Decisions

- [ADR-001: .NET 10 as Backend Platform](./ADR-001-dotnet-backend.md)
- [ADR-004: Native iOS and Android Apps](./ADR-004-native-mobile-apps.md)

## References

- [Angular Documentation](https://angular.io)
- [Angular + Electron](https://www.electronjs.org/)
