// =============================================================================
// Luminous - Production Environment Parameters
// =============================================================================
// Parameters for deploying Luminous infrastructure to the production
// environment. Uses high-availability configurations with appropriate SKUs.
//
// Usage:
//   az deployment sub create \
//     --location eastus2 \
//     --template-file main.bicep \
//     --parameters @parameters/prod.bicepparam
// =============================================================================

using '../main.bicep'

// Environment Configuration
param environment = 'prod'
param location = 'eastus2'
param projectName = 'luminous'
param projectPrefix = 'lum'

// Tags
param tags = {
  Project: 'Luminous'
  Environment: 'Production'
  ManagedBy: 'Bicep'
  Repository: 'trickpatty/Luminous'
  CostCenter: 'Production'
  Criticality: 'High'
}

// Cosmos DB - Provisioned throughput with auto-scale for production
param cosmosDbServerless = false
param cosmosDbConsistencyLevel = 'Session'

// App Service - Premium tier for production (better performance, scaling)
param appServiceSkuName = 'P1v3'

// Redis - Standard tier for production with higher capacity
param redisSku = 'Standard'
param redisFamily = 'C'
param redisCapacity = 1

// SignalR - Standard tier for production
param signalRSku = 'Standard_S1'

// Static Web App - Standard for production
param staticWebAppSku = 'Standard'
