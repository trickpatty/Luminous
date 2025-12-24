// =============================================================================
// Luminous - Staging Environment Parameters
// =============================================================================
// Parameters for deploying Luminous infrastructure to the staging
// environment. Uses production-like configuration for testing.
//
// Usage:
//   az deployment sub create \
//     --location eastus2 \
//     --template-file main.bicep \
//     --parameters @parameters/staging.bicepparam
// =============================================================================

using '../main.bicep'

// Environment Configuration
param environment = 'stg'
param location = 'eastus2'
param projectName = 'luminous'
param projectPrefix = 'lum'

// Tags
param tags = {
  Project: 'Luminous'
  Environment: 'Staging'
  ManagedBy: 'Bicep'
  Repository: 'trickpatty/Luminous'
  CostCenter: 'Staging'
}

// Cosmos DB - Provisioned throughput for staging (production-like)
param cosmosDbServerless = false
param cosmosDbConsistencyLevel = 'Session'

// App Service - Standard tier for staging
param appServiceSkuName = 'S1'

// Redis - Standard tier for staging
param redisSku = 'Standard'
param redisCapacity = 0

// SignalR - Standard tier for staging
param signalRSku = 'Standard_S1'

// Static Web App - Standard for staging
param staticWebAppSku = 'Standard'
