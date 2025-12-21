# ADR-003: Azure as Cloud Platform

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Architecture, Technology

## Context

Luminous needs a cloud platform to host:
- Multi-tenant API backend
- Serverless functions for background processing
- Database for family data
- File storage for media
- Identity services for authentication
- Real-time messaging for sync

We need to choose between major cloud providers or a multi-cloud/self-hosted approach.

## Decision Drivers

- **Enterprise readiness**: Production-grade services with SLAs
- **.NET integration**: Seamless experience with our backend platform
- **Identity services**: Consumer identity management (B2C)
- **Global availability**: Multi-region support for future expansion
- **Cost management**: Predictable pricing for SaaS model
- **Developer experience**: Good tooling and documentation

## Considered Options

### Option 1: Microsoft Azure

Microsoft's cloud platform with .NET-first approach.

**Pros:**
- Best .NET SDK and tooling integration
- Azure AD B2C for consumer identity
- Cosmos DB for global-scale NoSQL
- Azure Static Web Apps for Angular
- Excellent Azure Functions experience
- Bicep for native IaC

**Cons:**
- Some services more expensive than alternatives
- Occasional service complexity
- Steeper learning curve for some services

### Option 2: AWS

Amazon's comprehensive cloud platform.

**Pros:**
- Largest market share
- Mature services
- Competitive pricing

**Cons:**
- .NET is second-class citizen
- No built-in B2C identity (need Cognito)
- Terraform required for IaC (not native)

### Option 3: Google Cloud

Google's cloud platform.

**Pros:**
- Strong in data and AI
- Good Kubernetes support

**Cons:**
- Smallest enterprise adoption
- .NET support limited
- No B2C identity service

### Option 4: Self-Hosted

Run on own infrastructure or VPS.

**Pros:**
- Full control
- No vendor lock-in

**Cons:**
- Significant operational burden
- No managed identity service
- Limited scalability
- Requires DevOps expertise

## Decision

We will use **Microsoft Azure** as the cloud platform for Luminous.

## Rationale

1. **.NET First**: Azure is the natural home for .NET applications with first-class SDKs, tooling, and documentation.

2. **Azure AD B2C**: Provides consumer identity management out of the box, including social logins (Google, Apple), MFA, and custom policies.

3. **Cosmos DB**: Global-scale NoSQL database with excellent .NET SDK, ideal for multi-tenant family data.

4. **Serverless**: Azure Functions with .NET isolated model provides the best experience for our background jobs.

5. **Bicep with AVMs**: Azure's native IaC with verified modules reduces infrastructure code complexity.

6. **SignalR Service**: Managed WebSocket infrastructure for real-time sync between devices.

## Consequences

### Positive

- Seamless .NET development experience
- Managed identity without building auth from scratch
- Global scale with Cosmos DB
- Native IaC with Bicep
- Single vendor for all services

### Negative

- Vendor lock-in to Microsoft
- Some services more expensive than alternatives
- Learning curve for Azure-specific services

### Neutral

- Need Azure subscription management
- Should implement cost monitoring and alerts
- Consider Azure credits for development

## Azure Services Used

| Service | Purpose |
|---------|---------|
| **App Service** | API hosting |
| **Azure Functions** | Serverless compute |
| **Cosmos DB** | Document database |
| **Blob Storage** | File storage |
| **Azure AD B2C** | Consumer identity |
| **SignalR Service** | Real-time messaging |
| **Key Vault** | Secrets management |
| **App Configuration** | Feature flags, settings |
| **Redis Cache** | Session and cache |
| **Service Bus** | Message queuing |
| **Static Web Apps** | Angular hosting |
| **Application Insights** | Monitoring |

## Related Decisions

- [ADR-001: .NET 10 as Backend Platform](./ADR-001-dotnet-backend.md)
- [ADR-005: CosmosDB as Primary Data Store](./ADR-005-cosmosdb-data-store.md)
- [ADR-007: Bicep with AVMs for IaC](./ADR-007-bicep-avm-iac.md)
- [ADR-010: Azure AD B2C for Identity](./ADR-010-azure-ad-b2c-identity.md)

## References

- [Azure Documentation](https://learn.microsoft.com/azure)
- [Azure for .NET Developers](https://learn.microsoft.com/dotnet/azure/)
