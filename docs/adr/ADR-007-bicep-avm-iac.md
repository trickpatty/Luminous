# ADR-007: Bicep with AVMs for Infrastructure as Code

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Architecture, Technology

## Context

Luminous requires infrastructure as code (IaC) to provision and manage Azure resources consistently across environments. We need to decide on the IaC approach that provides:
- Reproducible deployments
- Version-controlled infrastructure
- Environment parity (dev/staging/production)
- Secure credential handling
- Easy maintenance

## Decision Drivers

- **Azure-native**: Best integration with Azure resources
- **Maintainability**: Easy to understand and modify
- **Community support**: Reusable modules and patterns
- **CI/CD integration**: Works with GitHub Actions
- **Security**: Proper secrets management
- **Type safety**: Catch errors before deployment

## Considered Options

### Option 1: Bicep with Azure Verified Modules (AVMs)

Azure's native IaC language with Microsoft-maintained modules.

**Pros:**
- Native Azure integration
- Type-safe with IntelliSense
- Simpler syntax than ARM templates
- AVMs are enterprise-tested and security-reviewed
- Built-in parameter validation
- Direct Azure CLI integration

**Cons:**
- Azure-only (not multi-cloud)
- Newer than Terraform (smaller community)
- AVMs can have version updates

### Option 2: Terraform

HashiCorp's multi-cloud IaC tool.

**Pros:**
- Multi-cloud support
- Large community
- Mature tooling
- State management

**Cons:**
- Third-party tool for Azure
- HCL learning curve
- State file management complexity
- Azure provider can lag behind features

### Option 3: Azure ARM Templates

Azure's original JSON-based IaC.

**Pros:**
- Fully supported
- No transpilation

**Cons:**
- Verbose JSON syntax
- Hard to read and maintain
- No native modularity
- Error-prone

### Option 4: Pulumi

Programming language-based IaC.

**Pros:**
- Use TypeScript/C#
- Full programming language features

**Cons:**
- Additional service to manage
- State management complexity
- Steeper learning curve

## Decision

We will use **Bicep with Azure Verified Modules (AVMs)** for all infrastructure as code.

## Rationale

1. **Azure Native**: Bicep is the native IaC for Azure, with first-class support and immediate access to new features.

2. **Azure Verified Modules**: AVMs are Microsoft-maintained, security-reviewed modules that follow best practices and reduce boilerplate.

3. **Type Safety**: Bicep provides compile-time type checking, catching errors before deployment.

4. **Simplicity**: Cleaner syntax than ARM templates, easier to read and maintain than Terraform HCL.

5. **CI/CD Integration**: Direct integration with Azure CLI and GitHub Actions without additional tools.

6. **No State Management**: Unlike Terraform, Bicep doesn't require separate state file management.

## Consequences

### Positive

- Clean, readable infrastructure code
- Enterprise-tested modules via AVMs
- Type-safe with IntelliSense support
- No external state management
- Direct Azure integration
- Security-reviewed modules

### Negative

- Azure-only (not portable)
- Smaller community than Terraform
- AVM versions need monitoring
- Limited third-party integrations

### Neutral

- Need to learn Bicep syntax
- Should pin AVM versions in production
- Requires Azure CLI for deployments

## Implementation

### Project Structure

```
infra/
├── main.bicep              # Main orchestration
├── main.bicepparam         # Parameter file
├── modules/
│   ├── app-service.bicep   # App Service wrapper
│   ├── cosmos-db.bicep     # Cosmos DB wrapper
│   ├── storage.bicep       # Storage account wrapper
│   └── ...
├── environments/
│   ├── dev.bicepparam      # Development parameters
│   ├── stg.bicepparam      # Stg parameters
│   └── prd.bicepparam      # Production parameters
└── scripts/
    ├── deploy.sh           # Deployment script
    └── validate.sh         # Validation script
```

### AVM Usage Example

```bicep
// Using Azure Verified Module for App Service
module appService 'br/public:avm/res/web/site:0.3.0' = {
  name: 'appServiceDeployment'
  params: {
    name: 'luminous-api-${environment}'
    location: location
    kind: 'app'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    managedIdentities: {
      systemAssigned: true
    }
    appSettingsKeyValuePairs: {
      AZURE_COSMOS_ENDPOINT: cosmosDb.outputs.endpoint
    }
  }
}

// Using AVM for Cosmos DB
module cosmosDb 'br/public:avm/res/document-db/database-account:0.6.0' = {
  name: 'cosmosDbDeployment'
  params: {
    name: 'luminous-cosmos-${environment}'
    location: location
    sqlDatabases: [
      {
        name: 'luminous'
        containers: [
          {
            name: 'families'
            partitionKeyPath: '/id'
          }
          {
            name: 'users'
            partitionKeyPath: '/familyId'
          }
        ]
      }
    ]
  }
}
```

### Deployment Script

```bash
#!/bin/bash
# deploy.sh

ENVIRONMENT=${1:-dev}
LOCATION=${2:-eastus2}
RESOURCE_GROUP="luminous-${ENVIRONMENT}-rg"

# Create resource group if needed
az group create --name $RESOURCE_GROUP --location $LOCATION

# Deploy infrastructure
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters "environments/${ENVIRONMENT}.bicepparam" \
  --name "luminous-deploy-$(date +%Y%m%d%H%M%S)"
```

### GitHub Actions Integration

```yaml
# .github/workflows/deploy-infra.yml
name: Deploy Infrastructure

on:
  push:
    branches: [main]
    paths: ['infra/**']

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Deploy Bicep
        uses: azure/arm-deploy@v2
        with:
          resourceGroupName: luminous-prd-rg
          template: ./infra/main.bicep
          parameters: ./infra/environments/prd.bicepparam
```

## Related Decisions

- [ADR-003: Azure as Cloud Platform](./ADR-003-azure-cloud-platform.md)
- [ADR-005: CosmosDB as Primary Data Store](./ADR-005-cosmosdb-data-store.md)
- [ADR-010: Azure AD B2C for Identity](./ADR-010-azure-ad-b2c-identity.md)

## References

- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure Verified Modules](https://azure.github.io/Azure-Verified-Modules/)
- [AVM Bicep Module Index](https://azure.github.io/Azure-Verified-Modules/indexes/bicep/)
