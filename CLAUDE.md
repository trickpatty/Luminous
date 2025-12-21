# CLAUDE.md - Repository Guidelines for Luminous

This document defines the expectations, standards, and development practices for the Luminous project. All contributors (human and AI) should follow these guidelines.

## Project Overview

**Luminous** is a digital family command center designed for large, portrait-mounted touchscreens. It provides a calm, glanceable view of household schedules, tasks, and reminders.

For comprehensive project documentation, see:
- [Project Overview](./docs/PROJECT-OVERVIEW.md) - Complete product specification
- [Architecture](./docs/ARCHITECTURE.md) - Technical architecture documentation
- [Roadmap](./docs/ROADMAP.md) - Development phases and milestones
- [Architecture Decision Records](./docs/adr/) - Key design decisions

---

## TOGAF Enterprise Architecture Principles

Luminous follows TOGAF (The Open Group Architecture Framework) principles for enterprise architecture. All architectural decisions must align with these principles.

### Architecture Development Method (ADM)

We follow TOGAF ADM phases in our development process:

| Phase | Description | Luminous Application |
|-------|-------------|---------------------|
| **Preliminary** | Framework and principles | Establish guidelines (this document) |
| **A: Vision** | Architecture vision | Project overview and goals |
| **B: Business** | Business architecture | Feature capabilities and processes |
| **C: Information** | Data architecture | Domain models and data flows |
| **C: Application** | Application architecture | Component design and boundaries |
| **D: Technology** | Technology architecture | Technology stack decisions |
| **E: Opportunities** | Solutions and migration | Roadmap phases |
| **F: Migration** | Planning | Phase transitions |
| **G: Governance** | Implementation governance | Code reviews, ADRs |
| **H: Change** | Architecture change | Version updates, migrations |

### Core Architecture Principles

#### Business Principles

| ID | Principle | Description |
|----|-----------|-------------|
| BP-1 | **Family-Centric Design** | All features must serve real family coordination needs |
| BP-2 | **Zero Distraction** | The product differentiates by what it omits |
| BP-3 | **Inclusive by Default** | Design for all ages and abilities |
| BP-4 | **Privacy as a Feature** | Users own and control their data |

#### Data Principles

| ID | Principle | Description |
|----|-----------|-------------|
| DP-1 | **Cloud-First with Local Cache** | Primary data in Azure; local caching for offline support |
| DP-2 | **User Data Ownership** | Users can export their data; multi-tenant isolation guaranteed |
| DP-3 | **Single Source of Truth** | CosmosDB as source; partition isolation per family |
| DP-4 | **Data Minimization** | Collect only what's necessary |

#### Application Principles

| ID | Principle | Description |
|----|-----------|-------------|
| AP-1 | **Modular Composability** | Features evolve independently |
| AP-2 | **Cross-Platform Consistency** | Unified experience across platforms |
| AP-3 | **Graceful Degradation** | Partial failures shouldn't block usage |
| AP-4 | **Performance on Constraints** | Optimize for modest hardware |

#### Technology Principles

| ID | Principle | Description |
|----|-----------|-------------|
| TP-1 | **Azure-Native Stack** | Leverage Azure PaaS services; .NET/Angular/native mobile |
| TP-2 | **Commodity Hardware** | ARM and x86 support for displays; any device for mobile |
| TP-3 | **Standards-Based Integration** | OAuth 2.0/OIDC, REST APIs, OpenAPI contracts |
| TP-4 | **Infrastructure as Code** | Bicep with AVMs for reproducible deployments |

### Architecture Governance

1. **All significant decisions require an ADR**
   - New technology choices
   - Architectural pattern changes
   - Integration approaches
   - Data model changes

2. **ADRs must follow the template** in `/docs/adr/ADR-000-template.md`

3. **Review process**
   - ADRs proposed via pull request
   - Minimum one reviewer approval
   - Discussion period before acceptance

4. **Principle compliance checks** in code reviews:
   - Does this change align with architecture principles?
   - Does it follow the bounded context boundaries?
   - Is it appropriately documented?

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
# .NET Backend (Clean Architecture)
src/
├── Luminous.Domain/           # Domain entities, value objects, interfaces
├── Luminous.Application/      # Use cases, DTOs, application services
├── Luminous.Infrastructure/   # Data access, external services, Azure integrations
├── Luminous.Api/              # ASP.NET Core Web API, controllers
└── Luminous.Functions/        # Azure Functions for background processing

# Angular Web/Display Applications
clients/
├── web/                       # Main web application
│   ├── src/app/
│   │   ├── core/              # Singleton services, guards, interceptors
│   │   ├── shared/            # Shared components, pipes, directives
│   │   ├── features/          # Feature modules (lazy-loaded)
│   │   └── models/            # TypeScript interfaces and types
├── display/                   # Electron display application
└── shared/                    # Shared Angular libraries

# Native Mobile Applications
mobile/
├── ios/                       # Swift/SwiftUI iOS app
└── android/                   # Kotlin/Compose Android app

# Infrastructure as Code
infra/
├── modules/                   # Bicep modules wrapping AVMs
├── environments/              # Parameter files per environment
└── main.bicep                 # Main orchestration
```

#### Component Guidelines

**Angular (Web/Display)**
- One component per file with co-located template and styles
- Use standalone components (Angular 19+)
- Inputs/Outputs should be typed and documented
- Use services for state management, avoid deep component hierarchies
- Components should be presentational or smart, not both

**Native Mobile**
- iOS: Follow SwiftUI patterns with ObservableObject for state
- Android: Use Compose with ViewModel and StateFlow
- Share API models via OpenAPI-generated clients

**.NET Backend**
- Follow Clean Architecture layer separation
- Use MediatR for CQRS pattern
- Repository pattern for data access
- Domain entities should be persistence-ignorant

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

**Features**
- `calendar` - Calendar-related changes
- `tasks` - Task/chore management
- `routines` - Routine management
- `rewards` - Rewards and gamification
- `meals` - Meal planning and recipes
- `lists` - List management
- `profiles` - Profile and household management
- `notes` - Notes and reminders
- `import` - Magic import feature
- `sync` - Real-time synchronization (SignalR)
- `linking` - Device linking flow

**Applications**
- `api` - .NET API backend
- `functions` - Azure Functions
- `web` - Angular web application
- `display` - Angular/Electron display application
- `ios` - iOS native application
- `android` - Android native application

**Infrastructure**
- `ui` - General UI components
- `auth` - Authentication (Passkeys/WebAuthn/OAuth)
- `cosmos` - CosmosDB/data layer changes
- `storage` - Blob storage changes
- `infra` - Bicep/IaC infrastructure
- `config` - Configuration and settings

**Meta**
- `docs` - Documentation
- `adr` - Architecture Decision Records
- `ci` - CI/CD pipeline changes

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

### Local Development Setup

```bash
# Start local Azure emulators (CosmosDB, Storage, Redis)
docker-compose up -d

# .NET API
cd src/Luminous.Api
dotnet restore
dotnet run

# Angular Web App
cd clients/web
npm install
npm start

# Angular Display App (Electron)
cd clients/display
npm install
npm run electron:dev
```

### .NET Backend Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run API with hot reload
dotnet watch run --project src/Luminous.Api

# Run Azure Functions locally
func start --csharp
```

### Angular Frontend Commands

```bash
# Install dependencies
npm install

# Development server
npm start

# Run tests
npm test

# Build for production
npm run build

# Lint code
npm run lint

# Type check
npm run typecheck
```

### Infrastructure Commands

```bash
# Validate Bicep
az bicep build --file infra/main.bicep

# Deploy to Azure (dev environment)
./infra/scripts/deploy.sh dev

# Deploy to Azure (prod environment)
./infra/scripts/deploy.sh prod
```

### Mobile Development

```bash
# iOS (requires macOS with Xcode)
cd mobile/ios
xcodebuild -scheme Luminous -sdk iphonesimulator

# Android
cd mobile/android
./gradlew assembleDebug
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
