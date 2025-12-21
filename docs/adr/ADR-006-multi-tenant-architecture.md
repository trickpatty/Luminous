# ADR-006: Multi-Tenant Architecture

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Architecture

## Context

Luminous serves multiple families, each with their own data and users. We need to decide how to architect the system to support multiple tenants (families) while ensuring:
- Complete data isolation between families
- Easy onboarding of new families
- Efficient resource utilization
- Secure access control

## Decision Drivers

- **Data isolation**: Families must never see each other's data
- **Scalability**: Support growth from tens to thousands of families
- **Cost efficiency**: Share infrastructure where appropriate
- **Onboarding**: Quick self-service family creation
- **Security**: Strong tenant boundaries

## Considered Options

### Option 1: Shared Database with Partition Key

Single database with all families, partitioned by family ID.

**Pros:**
- Most cost-efficient
- Simple infrastructure
- Easy to manage
- Fast onboarding (create documents)

**Cons:**
- Requires careful partition key design
- Cross-tenant queries possible if bugs exist
- Single point of failure

### Option 2: Database per Tenant

Separate database for each family.

**Pros:**
- Complete physical isolation
- Easy to backup/restore per tenant

**Cons:**
- Expensive at scale
- Complex provisioning
- Management overhead
- Slow onboarding

### Option 3: Schema per Tenant

Shared database with separate schemas/containers per family.

**Pros:**
- Good isolation
- Moderate cost

**Cons:**
- Complex container management
- Provisioning overhead
- Not natural for Cosmos DB

## Decision

We will use a **shared database with partition key isolation** (Option 1), with familyId as the partition key for all tenant data.

## Rationale

1. **Cost Efficiency**: Single Cosmos DB account with shared throughput is most cost-effective for a SaaS model.

2. **Natural Fit**: Cosmos DB's partition model naturally isolates data by family ID, with each family's data in its own logical partition.

3. **Fast Onboarding**: Creating a new family only requires inserting documents, no infrastructure provisioning.

4. **Azure AD B2C Integration**: JWT tokens contain family_id claim, making authorization straightforward.

5. **Scalability**: Cosmos DB automatically distributes partitions as we grow.

## Tenant Isolation Layers

### Layer 1: Data Partitioning

All tenant data partitioned by familyId in Cosmos DB.

```csharp
// Every query includes partition key
var events = await container
    .GetItemLinqQueryable<Event>()
    .Where(e => e.FamilyId == currentUserFamilyId)
    .ToListAsync();
```

### Layer 2: Authorization

JWT tokens include family_id claim, validated on every request.

```csharp
[Authorize(Policy = "FamilyMember")]
public async Task<IActionResult> GetEvents()
{
    var familyId = User.GetFamilyId(); // From JWT claim
    // All operations scoped to this family
}
```

### Layer 3: Blob Storage

Blob containers organized by family:
```
/families/{familyId}/avatars/
/families/{familyId}/recipes/
```

### Layer 4: SignalR Groups

Real-time updates scoped to family groups:
```csharp
await Clients.Group($"family:{familyId}").SendAsync("EventUpdated", event);
```

## Consequences

### Positive

- Cost-effective for SaaS model
- Fast family onboarding (seconds)
- Simple infrastructure management
- Natural Cosmos DB pattern
- Scales automatically

### Negative

- Bugs could theoretically leak data (mitigated by layers)
- Cross-tenant reporting requires care
- All data in single database

### Neutral

- Must enforce partition key in all queries
- Need comprehensive testing for isolation
- Should implement tenant ID validation middleware

## Security Measures

1. **Partition Key Enforcement**: Repository base class always includes familyId
2. **Claim Validation**: Middleware validates JWT family_id claim
3. **Query Auditing**: Log all data access with tenant context
4. **Penetration Testing**: Regular testing for tenant isolation
5. **Code Review**: Check all queries include partition scope

## Related Decisions

- [ADR-005: CosmosDB as Primary Data Store](./ADR-005-cosmosdb-data-store.md)
- [ADR-010: Azure AD B2C for Identity](./ADR-010-azure-ad-b2c-identity.md)

## References

- [Multi-tenant SaaS patterns](https://learn.microsoft.com/azure/architecture/guide/multitenant/overview)
- [Cosmos DB multi-tenancy](https://learn.microsoft.com/azure/cosmos-db/how-to-multi-master)
