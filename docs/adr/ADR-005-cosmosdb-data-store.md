# ADR-005: CosmosDB as Primary Data Store

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Architecture, Technology

## Context

Luminous needs a database that can:
- Store family data (events, chores, lists, profiles)
- Support multi-tenant data isolation
- Scale to thousands of families
- Provide low-latency reads for real-time displays
- Handle flexible document schemas

## Decision Drivers

- **Multi-tenancy**: Clean isolation between family data
- **Scalability**: Handle growth from MVP to thousands of families
- **Performance**: Low-latency reads for always-on displays
- **Flexibility**: Schema evolution without migrations
- **Azure integration**: Native integration with our cloud platform
- **Global distribution**: Option for multi-region in future

## Considered Options

### Option 1: Azure Cosmos DB (SQL API)

Microsoft's globally distributed, multi-model database.

**Pros:**
- Native Azure integration with managed identity
- Horizontal scaling with partitioning
- Low-latency global distribution
- Flexible JSON document model
- Automatic indexing
- Built-in change feed for sync

**Cons:**
- Cost can grow with scale
- Requires partition key design
- RU model takes learning

### Option 2: Azure SQL Database

Traditional relational database on Azure.

**Pros:**
- Familiar SQL model
- Strong consistency
- Rich query capabilities

**Cons:**
- Schema migrations needed
- Less flexible for document data
- Harder to partition for multi-tenant
- Scaling more complex

### Option 3: PostgreSQL (Azure)

Open-source relational database.

**Pros:**
- Open standard
- JSON support
- Lower cost

**Cons:**
- Manual scaling configuration
- Schema management overhead
- Less Azure-native integration

### Option 4: MongoDB Atlas

Managed MongoDB service.

**Pros:**
- Popular document database
- Good developer experience

**Cons:**
- Third-party service (not Azure-native)
- Separate billing relationship
- Less Azure integration

## Decision

We will use **Azure Cosmos DB with SQL API** as the primary data store for Luminous.

## Rationale

1. **Multi-Tenant Partitioning**: Cosmos DB's partition key (familyId) provides clean data isolation and horizontal scaling for multi-tenant architecture.

2. **Azure Native**: Seamless integration with managed identity, Azure Functions, and other Azure services.

3. **Document Flexibility**: JSON documents adapt well to our domain (events, chores, profiles) without rigid schema migrations.

4. **Change Feed**: Built-in change feed enables real-time sync to connected displays.

5. **Performance**: Guaranteed single-digit millisecond latency at any scale.

6. **Global Distribution**: Option to expand to multiple regions as we grow.

## Consequences

### Positive

- Clean multi-tenant isolation via partitioning
- Flexible schema for domain evolution
- Low-latency reads for displays
- Change feed for real-time sync
- Automatic indexing

### Negative

- RU-based pricing requires monitoring
- Partition key design is critical upfront
- Cross-partition queries are expensive
- Learning curve for RU model

### Neutral

- Need to design documents and partition strategy carefully
- Serverless mode used across all environments for cost optimization

## Data Model

### Partition Strategy

| Container | Partition Key | Reasoning |
|-----------|---------------|-----------|
| families | /id | One document per family |
| users | /familyId | All users in same family together |
| events | /familyId | Query events by family |
| chores | /familyId | Query chores by family |
| devices | /familyId | Devices belong to family |
| lists | /familyId | Lists belong to family |

### Capacity Mode

**Serverless** mode is used for all environments (dev, stg, prd) for cost optimization:
- Pay-per-request pricing (no minimum throughput cost)
- Automatic scaling based on demand
- Ideal for variable/unpredictable workloads
- Maximum 5,000 RU/s per operation (sufficient for family-scale apps)

| Operation | Estimated RUs per Request |
|-----------|---------------------------|
| Read today's events | ~5 RUs |
| Write event | ~10 RUs |
| Read chores | ~5 RUs |
| Complete chore | ~10 RUs |

## Related Decisions

- [ADR-003: Azure as Cloud Platform](./ADR-003-azure-cloud-platform.md)
- [ADR-006: Multi-Tenant Architecture](./ADR-006-multi-tenant-architecture.md)

## References

- [Azure Cosmos DB Documentation](https://learn.microsoft.com/azure/cosmos-db/)
- [Cosmos DB Partitioning](https://learn.microsoft.com/azure/cosmos-db/partitioning-overview)
