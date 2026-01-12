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
param redisCapacity = 0

// SignalR - Free tier for dev
param signalRSku = 'Free_F1'

// Static Web App - Standard tier for dev (required for linked backend)
param staticWebAppSku = 'Standard'

// Role Assignments - defaults to true
// Set to false if the deploying identity lacks User Access Administrator or Owner role
// When false, role assignments must be created separately by a privileged identity
// param deployRoleAssignments = true

// Custom Domain Configuration (Optional for dev)
// Leave commented to use Azure Static Web App default hostname
// param customDomain = 'dev.luminousfamily.com'
// param deployDnsZone = false
