// =============================================================================
// Luminous - Development Environment Parameters
// =============================================================================
// Parameters for deploying Luminous infrastructure to the development
// environment. Uses minimal SKUs to reduce costs.
//
// Usage:
//   az deployment sub create \
//     --location eastus2 \
//     --template-file main.bicep \
//     --parameters @parameters/dev.bicepparam
// =============================================================================

using '../main.bicep'

// Environment Configuration
param environment = 'dev'
param location = 'eastus2'
param projectName = 'luminous'
param projectPrefix = 'lum'

// Tags
param tags = {
  Project: 'Luminous'
  Environment: 'Development'
  ManagedBy: 'Bicep'
  Repository: 'trickpatty/Luminous'
  CostCenter: 'Development'
}

// Cosmos DB - Use serverless for dev to reduce costs
param cosmosDbServerless = true
param cosmosDbConsistencyLevel = 'Session'

// App Service - Basic tier for dev
param appServiceSkuName = 'B1'

// Redis - Basic tier for dev (smallest available)
param redisSku = 'Basic'
param redisFamily = 'C'
param redisCapacity = 0

// SignalR - Free tier for dev
param signalRSku = 'Free_F1'

// Static Web App - Free tier for dev
param staticWebAppSku = 'Free'
