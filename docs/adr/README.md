# Architecture Decision Records

This directory contains Architecture Decision Records (ADRs) for the Luminous project.

## What is an ADR?

An Architecture Decision Record captures an important architectural decision made along with its context and consequences. ADRs are used to document why certain decisions were made and to communicate these decisions to stakeholders.

## ADR Template

New ADRs should follow the template in [ADR-000-template.md](./ADR-000-template.md).

## ADR Index

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [ADR-001](./ADR-001-typescript-primary-language.md) | TypeScript as Primary Language | Accepted | 2025-12-21 |
| [ADR-002](./ADR-002-react-ui-framework.md) | React as UI Framework | Accepted | 2025-12-21 |
| [ADR-003](./ADR-003-local-first-architecture.md) | Local-First Data Architecture | Accepted | 2025-12-21 |
| [ADR-004](./ADR-004-two-way-sync-google-first.md) | Two-Way Sync Initially Google-Only | Accepted | 2025-12-21 |
| [ADR-005](./ADR-005-magic-import-approval.md) | Magic Import Requires Approval | Accepted | 2025-12-21 |
| [ADR-006](./ADR-006-zero-distraction-principle.md) | Zero-Distraction Design Principle | Accepted | 2025-12-21 |
| [ADR-007](./ADR-007-self-hosting-first.md) | Self-Hosting as Primary Model | Accepted | 2025-12-21 |

## ADR Lifecycle

- **Draft**: Initial proposal under discussion
- **Proposed**: Ready for review and approval
- **Accepted**: Decision has been approved and adopted
- **Deprecated**: Decision is no longer relevant but kept for historical context
- **Superseded**: Replaced by a newer ADR (reference the new ADR)

## Creating a New ADR

1. Copy the template: `cp ADR-000-template.md ADR-XXX-short-title.md`
2. Fill in all sections
3. Submit a pull request for review
4. Update this README index after approval
