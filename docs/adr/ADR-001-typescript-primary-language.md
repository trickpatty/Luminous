# ADR-001: TypeScript as Primary Language

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Technology

## Context

Luminous is a cross-platform application suite consisting of:
- A display application (desktop/embedded)
- Mobile applications (iOS and Android)
- A web application
- An optional sync server
- Shared domain logic and UI components

We need to select a primary programming language that can serve all these platforms effectively while maintaining code quality, developer experience, and long-term maintainability.

## Decision Drivers

- **Cross-platform capability**: Must support web, mobile, desktop, and server environments
- **Type safety**: Strong typing to catch errors early and improve maintainability
- **Developer ecosystem**: Rich tooling, libraries, and community support
- **Team familiarity**: Common knowledge base for frontend developers
- **Code sharing**: Ability to share domain logic across all platforms
- **Hiring and contributions**: Available talent pool for open-source contributions

## Considered Options

### Option 1: TypeScript

TypeScript is a typed superset of JavaScript that compiles to plain JavaScript.

**Pros:**
- Excellent type system with gradual typing
- Native support for React, React Native, and Node.js
- Large ecosystem and community
- First-class tooling (VS Code, ESLint, etc.)
- Can share code between all platforms
- Popular among frontend developers

**Cons:**
- Runtime type checking requires additional libraries
- Build step required
- Some edge cases where types don't match runtime behavior

### Option 2: JavaScript (ES2024+)

Modern JavaScript with the latest ECMAScript features.

**Pros:**
- No build step for type checking
- Universal support
- Simpler toolchain

**Cons:**
- No static type checking
- Higher maintenance burden at scale
- Runtime errors harder to catch
- Less IDE support for refactoring

### Option 3: Dart/Flutter

Cross-platform framework with its own language.

**Pros:**
- Excellent cross-platform mobile and desktop
- Strong typing
- Hot reload

**Cons:**
- Separate web ecosystem
- Smaller community than React
- Less code sharing with web technologies
- Different paradigm than most web developers know

### Option 4: Kotlin Multiplatform

Kotlin with multiplatform support.

**Pros:**
- Strong typing
- Native performance on mobile
- Good for Android developers

**Cons:**
- Immature web support
- Smaller React ecosystem integration
- Steeper learning curve for web developers
- Limited React Native integration

## Decision

We will use **TypeScript** as the primary language for all Luminous applications and packages.

## Rationale

TypeScript best satisfies our decision drivers:

1. **Cross-platform**: TypeScript runs natively in all our target environments through existing runtimes (browsers, Node.js, React Native).

2. **Type safety**: TypeScript's type system catches a large class of bugs at compile time, improving code quality and reducing runtime errors. This is critical for a family coordination app where reliability matters.

3. **Code sharing**: With TypeScript, we can share domain models, validation logic, and business rules across display, mobile, web, and server applications through a monorepo structure.

4. **Developer ecosystem**: The TypeScript/React ecosystem is the largest and most mature for our use case, with excellent libraries for state management, testing, and UI.

5. **Contribution-friendly**: TypeScript is widely known, making it easier for open-source contributors to participate.

## Consequences

### Positive

- Consistent language across all platforms reduces context switching
- Type definitions serve as living documentation
- Refactoring is safer with compiler-checked types
- Excellent IDE support improves developer productivity
- Large pool of potential contributors

### Negative

- Build step adds complexity to development workflow
- Type definitions for some libraries may be incomplete or incorrect
- Learning curve for developers new to TypeScript
- Slightly larger bundle size due to some TypeScript patterns

### Neutral

- Must maintain TypeScript configuration across all packages
- Team must agree on TypeScript coding conventions (strict mode, type inference, etc.)

## Implementation Notes

- Use TypeScript strict mode in all packages
- Configure path aliases for clean imports
- Use project references for monorepo type checking
- Prefer interfaces over types for object shapes
- Use `unknown` over `any` where type is genuinely unknown

## Related Decisions

- [ADR-002: React as UI Framework](./ADR-002-react-ui-framework.md)

## References

- [TypeScript Official Documentation](https://www.typescriptlang.org/)
- [TypeScript Design Goals](https://github.com/Microsoft/TypeScript/wiki/TypeScript-Design-Goals)
