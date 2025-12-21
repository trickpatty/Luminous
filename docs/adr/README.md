# Architecture Decision Records

This directory contains Architecture Decision Records (ADRs) for the Luminous project.

## What is an ADR?

An Architecture Decision Record captures an important architectural decision made along with its context and consequences. ADRs are used to document why certain decisions were made and to communicate these decisions to stakeholders.

## ADR Template

New ADRs should follow the template in [ADR-000-template.md](./ADR-000-template.md).

## ADR Index

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [ADR-001](./ADR-001-dotnet-backend.md) | .NET 10 as Backend Platform | Accepted | 2025-12-21 |
| [ADR-002](./ADR-002-angular-web-framework.md) | Angular as Web Framework | Accepted | 2025-12-21 |
| [ADR-003](./ADR-003-azure-cloud-platform.md) | Azure as Cloud Platform | Accepted | 2025-12-21 |
| [ADR-004](./ADR-004-native-mobile-apps.md) | Native iOS and Android Apps | Accepted | 2025-12-21 |
| [ADR-005](./ADR-005-cosmosdb-data-store.md) | CosmosDB as Primary Data Store | Accepted | 2025-12-21 |
| [ADR-006](./ADR-006-multi-tenant-architecture.md) | Multi-Tenant Architecture | Accepted | 2025-12-21 |
| [ADR-007](./ADR-007-bicep-avm-iac.md) | Bicep with AVMs for IaC | Accepted | 2025-12-21 |
| [ADR-008](./ADR-008-magic-import-approval.md) | Magic Import Requires Approval | Accepted | 2025-12-21 |
| [ADR-009](./ADR-009-zero-distraction-principle.md) | Zero-Distraction Design Principle | Accepted | 2025-12-21 |
| [ADR-010](./ADR-010-azure-ad-b2c-identity.md) | Azure AD B2C for Identity | Accepted | 2025-12-21 |

## Superseded ADRs (Historical Reference)

The following ADRs from the initial architecture have been superseded:

| Old ADR | Title | Superseded By |
|---------|-------|---------------|
| ADR-001-typescript | TypeScript as Primary Language | ADR-001, ADR-002 (C#/TypeScript) |
| ADR-002-react | React as UI Framework | ADR-002 (Angular) |
| ADR-003-local-first | Local-First Data Architecture | ADR-003 (Azure Cloud) |
| ADR-007-self-hosting | Self-Hosting as Primary Model | ADR-003 (Azure Cloud) |

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
