# ADR-001: .NET 10 as Backend Platform

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Technology

## Context

Luminous requires a robust backend platform that can:
- Host REST APIs for all client applications
- Run serverless functions for background processing
- Integrate seamlessly with Azure services
- Support high-performance, multi-tenant operations

We need to select a backend platform that aligns with our Azure-first hosting strategy.

## Decision Drivers

- **Azure integration**: First-class support for Azure services
- **Performance**: High throughput for API operations
- **Type safety**: Strong typing to catch errors at compile time
- **Ecosystem**: Rich libraries for web APIs, data access, and authentication
- **Team expertise**: Available talent and community support
- **Long-term support**: Stable platform with clear upgrade paths

## Considered Options

### Option 1: .NET 10 (C#)

Microsoft's flagship application platform with C# language.

**Pros:**
- Best-in-class Azure integration
- Excellent performance (among fastest web frameworks)
- Mature ecosystem (Entity Framework, ASP.NET Core)
- Strong typing with modern C# features
- Native support for Azure Functions
- Long-term support from Microsoft

**Cons:**
- Separate language from frontend (TypeScript)
- Larger runtime compared to some alternatives
- Learning curve for non-.NET developers

### Option 2: Node.js (TypeScript)

JavaScript runtime with TypeScript for type safety.

**Pros:**
- Same language as Angular frontend
- Large npm ecosystem
- Good Azure Functions support
- Familiar to web developers

**Cons:**
- Single-threaded model limits performance
- TypeScript types are compile-time only
- Less mature ORM ecosystem
- Azure integration not as deep

### Option 3: Go

Google's systems programming language.

**Pros:**
- Excellent performance
- Simple, fast compilation
- Good for microservices

**Cons:**
- Limited Azure-native integration
- Smaller ecosystem for web APIs
- Less common for enterprise applications
- No native Azure Functions support

## Decision

We will use **.NET 10** with C# as the backend platform for all Luminous server-side components.

## Rationale

1. **Azure Integration**: .NET has the deepest integration with Azure services. Azure SDKs for .NET are first-class, with features like Managed Identity working seamlessly.

2. **Performance**: ASP.NET Core consistently ranks among the highest-performing web frameworks. This is critical for a multi-tenant platform serving many families.

3. **Azure Functions**: .NET isolated worker model provides the best experience for serverless functions with dependency injection and testability.

4. **Enterprise Ready**: .NET has mature patterns for building enterprise applications including CQRS, domain-driven design, and clean architecture.

5. **Cosmos DB Integration**: The Azure Cosmos DB SDK for .NET is the most feature-complete, supporting all API operations.

## Consequences

### Positive

- Seamless Azure service integration
- High API performance
- Mature tooling and debugging experience
- Strong community and Microsoft support
- Clear patterns for enterprise architecture

### Negative

- Different language from Angular frontend (C# vs TypeScript)
- Requires .NET knowledge for backend contributions
- Larger container images compared to Go

### Neutral

- Need to define API contracts for frontend/backend communication
- Must maintain separate build pipelines for .NET and Angular

## Implementation Notes

- Use .NET 10 (latest LTS when available)
- Follow Clean Architecture with separate Domain, Application, Infrastructure, and API layers
- Use MediatR for CQRS pattern
- Use FluentValidation for request validation
- Configure dependency injection with built-in DI container

## Related Decisions

- [ADR-002: Angular as Web Framework](./ADR-002-angular-web-framework.md)
- [ADR-003: Azure as Cloud Platform](./ADR-003-azure-cloud-platform.md)
- [ADR-005: CosmosDB as Primary Data Store](./ADR-005-cosmosdb-data-store.md)

## References

- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core)
- [.NET Performance](https://www.techempower.com/benchmarks/)
