# CLAUDE.md - Repository Guidelines for Luminous

This document defines the expectations, standards, and development practices for the Luminous project. All contributors (human and AI) should follow these guidelines.

## Project Overview

**Luminous** is a digital family command center designed for large, portrait-mounted touchscreens. It provides a calm, glanceable view of household schedules, tasks, and reminders.

---

## Core Development Principles

### 1. Programming Best Practices

#### DRY (Don't Repeat Yourself)
- Extract repeated logic into reusable functions, components, or utilities
- Use shared constants and configuration files
- Create abstractions only when patterns emerge (Rule of Three)
- Prefer composition over inheritance

#### SOLID Principles
- **S**ingle Responsibility: Each module/class/function should have one reason to change
- **O**pen/Closed: Open for extension, closed for modification
- **L**iskov Substitution: Subtypes must be substitutable for their base types
- **I**nterface Segregation: Many specific interfaces over one general-purpose interface
- **D**ependency Inversion: Depend on abstractions, not concretions

#### Domain-Driven Design (DDD)
- Model the domain explicitly with ubiquitous language
- Separate domain logic from infrastructure concerns
- Use bounded contexts to define clear boundaries
- Implement aggregates, entities, and value objects appropriately
- Keep domain models pure and framework-agnostic

#### Test-Driven Development (TDD)
- Write tests before implementation (Red-Green-Refactor)
- Each feature must have corresponding unit tests
- Integration tests for component interactions
- End-to-end tests for critical user journeys
- Maintain high test coverage without sacrificing test quality

---

## Architecture Standards

### Modular & Componentized Design

All code must be:

1. **Modular**: Self-contained units with clear boundaries
2. **Componentized**: UI elements as reusable, composable components
3. **Decoupled**: Minimal dependencies between modules
4. **Testable**: Easy to test in isolation

#### Directory Structure Expectations

```
src/
├── components/       # Reusable UI components
├── features/         # Feature-based modules (vertical slices)
├── domain/           # Domain models and business logic
├── services/         # External service integrations
├── hooks/            # Shared React hooks (if applicable)
├── utils/            # Pure utility functions
├── types/            # TypeScript type definitions
└── config/           # Configuration and constants
```

#### Component Guidelines

- One component per file
- Co-locate component tests, styles, and stories
- Props should be typed and documented
- Avoid prop drilling; use context or state management appropriately
- Components should be presentational or container, not both

---

## Documentation-Driven Design (DDD)

### All New Features MUST Follow This Process

1. **Write Documentation First**
   - Create/update relevant documentation before writing code
   - Define the feature's purpose, API, and expected behavior
   - Include usage examples and edge cases

2. **Design Review**
   - Documentation serves as the design specification
   - Review and iterate on the design before implementation
   - Get stakeholder alignment through documentation

3. **Implementation**
   - Code to match the documented specification
   - Update documentation if implementation reveals issues

4. **Documentation Artifacts Required**
   - README updates for user-facing features
   - API documentation for new endpoints/interfaces
   - Architecture Decision Records (ADRs) for significant decisions
   - Inline code documentation for complex logic

---

## Git Workflow & Commit Standards

### Conventional Commits with Gitmoji

All commits MUST follow the format:

```
<gitmoji> <type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Gitmoji Reference

| Emoji | Code | Type | Description |
|-------|------|------|-------------|
| ✨ | `:sparkles:` | feat | New feature |
| 🐛 | `:bug:` | fix | Bug fix |
| 📝 | `:memo:` | docs | Documentation |
| 💄 | `:lipstick:` | style | UI/style updates |
| ♻️ | `:recycle:` | refactor | Code refactoring |
| ✅ | `:white_check_mark:` | test | Adding/updating tests |
| 🔧 | `:wrench:` | chore | Configuration/tooling |
| ⚡ | `:zap:` | perf | Performance improvement |
| 🔥 | `:fire:` | remove | Removing code/files |
| 🚀 | `:rocket:` | deploy | Deployment related |
| 🔒 | `:lock:` | security | Security fix |
| ⬆️ | `:arrow_up:` | deps | Upgrade dependencies |
| ⬇️ | `:arrow_down:` | deps | Downgrade dependencies |
| 🏗️ | `:building_construction:` | arch | Architectural changes |
| 🎨 | `:art:` | format | Code formatting |
| 🚧 | `:construction:` | wip | Work in progress |
| 💚 | `:green_heart:` | ci | CI/CD fixes |
| 🔊 | `:loud_sound:` | logs | Add/update logs |
| 🔇 | `:mute:` | logs | Remove logs |
| ♿ | `:wheelchair:` | a11y | Accessibility |
| 🌐 | `:globe_with_meridians:` | i18n | Internationalization |
| 📱 | `:iphone:` | responsive | Responsive design |

### Scopes

Use scopes to indicate the affected area:

- `calendar` - Calendar-related changes
- `tasks` - Task/chore management
- `notes` - Notes and reminders
- `ui` - General UI components
- `display` - Display/kiosk mode
- `config` - Configuration
- `api` - API/backend changes
- `auth` - Authentication
- `db` - Database changes
- `infra` - Infrastructure

### Commit Examples

```bash
# Feature with scope
✨ feat(calendar): add week view navigation

# Bug fix
🐛 fix(tasks): resolve task completion state not persisting

# Documentation
📝 docs(readme): update installation instructions

# Refactoring with scope
♻️ refactor(ui): extract button component from form

# Tests
✅ test(calendar): add unit tests for date parsing

# Breaking change (use footer)
💥 feat(api)!: redesign event response structure

BREAKING CHANGE: Event objects now use ISO 8601 timestamps
```

---

## Code Quality Standards

### Code Review Checklist

- [ ] Follows DRY, SOLID, and DDD principles
- [ ] Includes appropriate tests
- [ ] Documentation updated
- [ ] No console.log or debug statements
- [ ] No hardcoded values (use constants/config)
- [ ] Accessible (WCAG 2.1 AA minimum)
- [ ] Performs well on target hardware
- [ ] Error handling is appropriate
- [ ] No security vulnerabilities introduced

### Linting & Formatting

- All code must pass linting without errors
- Use the project's configured formatter
- Pre-commit hooks enforce standards

### Performance Considerations

Given the always-on display nature of Luminous:

- Minimize re-renders and DOM updates
- Avoid memory leaks (clean up subscriptions, timers)
- Optimize for 24/7 operation
- Test on target hardware specifications
- Keep bundle size minimal

---

## Accessibility Requirements

Luminous must be usable by all household members:

- Support keyboard navigation
- Maintain proper focus management
- Use semantic HTML
- Provide sufficient color contrast
- Support screen readers where applicable
- Touch targets minimum 44x44 pixels
- Text readable from across the room

---

## Security Guidelines

- Never commit secrets, API keys, or credentials
- Validate all user input
- Sanitize displayed content
- Use HTTPS for all external communications
- Follow OWASP guidelines
- Regular dependency audits

---

## Build & Development Commands

```bash
# Install dependencies
npm install

# Run development server
npm run dev

# Run tests
npm test

# Run tests in watch mode
npm run test:watch

# Build for production
npm run build

# Lint code
npm run lint

# Format code
npm run format

# Type check
npm run typecheck
```

---

## Getting Help

- Check existing documentation first
- Search closed issues for similar problems
- Open a discussion for questions
- File an issue for bugs with reproduction steps

---

## License

This project is licensed under AGPL-3.0. All contributions must be compatible with this license.
