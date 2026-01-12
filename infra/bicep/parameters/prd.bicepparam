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
//     --parameters @parameters/prd.bicepparam
// =============================================================================

using '../main.bicep'

// Environment Configuration
param environment = 'prd'
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

// App Service - Basic tier for production
param appServiceSkuName = 'B2'

// Redis - Standard tier for production with higher capacity
param redisSku = 'Standard'
param redisCapacity = 1

// SignalR - Standard tier for production
param signalRSku = 'Standard_S1'

// Static Web App - Standard for production
param staticWebAppSku = 'Standard'

// Custom Domain Configuration
// OSS Note: Replace 'luminousfamily.com' with your own domain
// Set deployDnsZone=true to create Azure DNS zone (requires domain nameserver delegation)
// See docs/PRODUCTION-DEPLOYMENT.md for complete setup instructions
param customDomain = 'luminousfamily.com'
param deployDnsZone = true
