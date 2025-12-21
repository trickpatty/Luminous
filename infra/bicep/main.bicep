// =============================================================================
// Luminous - Main Infrastructure Deployment
// =============================================================================
// This is the main orchestration file for deploying the Luminous Azure
// infrastructure using Azure Verified Modules (AVMs).
//
// Usage:
//   az deployment sub create \
//     --location <region> \
//     --template-file main.bicep \
//     --parameters @parameters/dev.bicepparam
//
// TOGAF Principle: TP-4 - Infrastructure as Code
// =============================================================================

targetScope = 'subscription'

// =============================================================================
// Parameters
// =============================================================================

@description('Environment name (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Azure region for deployment')
param location string = 'eastus2'

@description('Project name used for resource naming')
param projectName string = 'luminous'

@description('Short project prefix for resource naming (max 3 chars)')
@maxLength(3)
param projectPrefix string = 'lum'

@description('Tags to apply to all resources')
param tags object = {
  Project: 'Luminous'
  Environment: environment
  ManagedBy: 'Bicep'
  Repository: 'trickpatty/Luminous'
}

// Cosmos DB Configuration
@description('Enable serverless mode for Cosmos DB (recommended for dev)')
param cosmosDbServerless bool = environment == 'dev'

@description('Cosmos DB consistency level')
@allowed(['Eventual', 'Session', 'BoundedStaleness', 'Strong', 'ConsistentPrefix'])
param cosmosDbConsistencyLevel string = 'Session'

// App Service Configuration
@description('App Service Plan SKU')
param appServiceSkuName string = environment == 'prod' ? 'P1v3' : 'B1'

// Redis Configuration
@description('Redis Cache SKU')
param redisSku string = environment == 'prod' ? 'Standard' : 'Basic'

@description('Redis Cache Family')
param redisFamily string = 'C'

@description('Redis Cache Capacity')
param redisCapacity int = environment == 'prod' ? 1 : 0

// SignalR Configuration
@description('SignalR Service SKU')
param signalRSku string = environment == 'prod' ? 'Standard_S1' : 'Free_F1'

// Static Web App Configuration
@description('Static Web App SKU')
param staticWebAppSku string = environment == 'prod' ? 'Standard' : 'Free'

// =============================================================================
// Variables
// =============================================================================

var resourceGroupName = 'rg-${projectPrefix}-${environment}'
var namingPrefix = '${projectPrefix}-${environment}'

// Resource naming following Azure naming conventions
var names = {
  resourceGroup: resourceGroupName
  cosmosDb: 'cosmos-${namingPrefix}'
  storageAccount: 'st${projectPrefix}${environment}' // Storage accounts can't have hyphens
  keyVault: 'kv-${namingPrefix}'
  appConfig: 'appcs-${namingPrefix}'
  appServicePlan: 'asp-${namingPrefix}'
  appService: 'app-${namingPrefix}-api'
  functionAppSync: 'func-${namingPrefix}-sync'
  functionAppImport: 'func-${namingPrefix}-import'
  signalR: 'sigr-${namingPrefix}'
  serviceBus: 'sb-${namingPrefix}'
  redis: 'redis-${namingPrefix}'
  staticWebApp: 'stapp-${namingPrefix}'
  logAnalytics: 'log-${namingPrefix}'
  appInsights: 'appi-${namingPrefix}'
}

// Cosmos DB container configurations
var cosmosContainers = [
  { name: 'families', partitionKeyPath: '/id' }
  { name: 'users', partitionKeyPath: '/familyId' }
  { name: 'events', partitionKeyPath: '/familyId' }
  { name: 'chores', partitionKeyPath: '/familyId' }
  { name: 'devices', partitionKeyPath: '/familyId' }
  { name: 'routines', partitionKeyPath: '/familyId' }
  { name: 'lists', partitionKeyPath: '/familyId' }
  { name: 'meals', partitionKeyPath: '/familyId' }
  { name: 'completions', partitionKeyPath: '/familyId' }
  { name: 'invitations', partitionKeyPath: '/familyId' }
  { name: 'credentials', partitionKeyPath: '/userId' } // WebAuthn credentials
]

// =============================================================================
// Resource Group
// =============================================================================

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: names.resourceGroup
  location: location
  tags: tags
}

// =============================================================================
// Monitoring (Deploy first - other resources depend on these)
// =============================================================================

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'deploy-log-analytics'
  scope: rg
  params: {
    name: names.logAnalytics
    location: location
    tags: tags
    retentionInDays: environment == 'prod' ? 90 : 30
  }
}

module appInsights 'modules/app-insights.bicep' = {
  name: 'deploy-app-insights'
  scope: rg
  params: {
    name: names.appInsights
    location: location
    tags: tags
    logAnalyticsWorkspaceId: logAnalytics.outputs.workspaceId
  }
}

// =============================================================================
// Security
// =============================================================================

module keyVault 'modules/key-vault.bicep' = {
  name: 'deploy-key-vault'
  scope: rg
  params: {
    name: names.keyVault
    location: location
    tags: tags
    enableSoftDelete: environment == 'prod'
    enablePurgeProtection: environment == 'prod'
  }
}

module appConfig 'modules/app-configuration.bicep' = {
  name: 'deploy-app-config'
  scope: rg
  params: {
    name: names.appConfig
    location: location
    tags: tags
    sku: environment == 'prod' ? 'Standard' : 'Free'
  }
}

// =============================================================================
// Data Services
// =============================================================================

module cosmosDb 'modules/cosmos-db.bicep' = {
  name: 'deploy-cosmos-db'
  scope: rg
  params: {
    name: names.cosmosDb
    location: location
    tags: tags
    enableServerless: cosmosDbServerless
    consistencyLevel: cosmosDbConsistencyLevel
    databaseName: projectName
    containers: cosmosContainers
  }
}

module storageAccount 'modules/storage-account.bicep' = {
  name: 'deploy-storage-account'
  scope: rg
  params: {
    name: names.storageAccount
    location: location
    tags: tags
    sku: environment == 'prod' ? 'Standard_GRS' : 'Standard_LRS'
    containerNames: [
      'avatars'
      'recipes'
      'imports'
      'exports'
    ]
  }
}

module redis 'modules/redis-cache.bicep' = {
  name: 'deploy-redis-cache'
  scope: rg
  params: {
    name: names.redis
    location: location
    tags: tags
    sku: redisSku
    family: redisFamily
    capacity: redisCapacity
  }
}

// =============================================================================
// Messaging Services
// =============================================================================

module serviceBus 'modules/service-bus.bicep' = {
  name: 'deploy-service-bus'
  scope: rg
  params: {
    name: names.serviceBus
    location: location
    tags: tags
    sku: environment == 'prod' ? 'Standard' : 'Basic'
    queues: [
      'calendar-sync'
      'import-processing'
      'notifications'
    ]
  }
}

module signalR 'modules/signalr.bicep' = {
  name: 'deploy-signalr'
  scope: rg
  params: {
    name: names.signalR
    location: location
    tags: tags
    sku: signalRSku
    serviceMode: 'Default'
  }
}

// =============================================================================
// Compute Services
// =============================================================================

module appServicePlan 'modules/app-service-plan.bicep' = {
  name: 'deploy-app-service-plan'
  scope: rg
  params: {
    name: names.appServicePlan
    location: location
    tags: tags
    skuName: appServiceSkuName
    kind: 'linux'
  }
}

module appService 'modules/app-service.bicep' = {
  name: 'deploy-app-service'
  scope: rg
  params: {
    name: names.appService
    location: location
    tags: tags
    appServicePlanId: appServicePlan.outputs.planId
    appInsightsConnectionString: appInsights.outputs.connectionString
    appInsightsInstrumentationKey: appInsights.outputs.instrumentationKey
    runtimeStack: 'DOTNETCORE'
    runtimeVersion: '10.0'
    appSettings: [
      { name: 'ASPNETCORE_ENVIRONMENT', value: environment == 'prod' ? 'Production' : 'Development' }
      { name: 'CosmosDb__Endpoint', value: cosmosDb.outputs.endpoint }
      { name: 'CosmosDb__DatabaseName', value: projectName }
      { name: 'Redis__ConnectionString', value: redis.outputs.connectionString }
      { name: 'SignalR__ConnectionString', value: signalR.outputs.connectionString }
      { name: 'ServiceBus__ConnectionString', value: serviceBus.outputs.connectionString }
      { name: 'Storage__ConnectionString', value: storageAccount.outputs.connectionString }
      { name: 'AppConfig__Endpoint', value: appConfig.outputs.endpoint }
    ]
    keyVaultName: keyVault.outputs.name
  }
  dependsOn: [
    cosmosDb
    redis
    signalR
    serviceBus
    storageAccount
    appConfig
  ]
}

module functionAppSync 'modules/function-app.bicep' = {
  name: 'deploy-function-app-sync'
  scope: rg
  params: {
    name: names.functionAppSync
    location: location
    tags: tags
    appServicePlanId: appServicePlan.outputs.planId
    storageAccountName: storageAccount.outputs.name
    appInsightsConnectionString: appInsights.outputs.connectionString
    appInsightsInstrumentationKey: appInsights.outputs.instrumentationKey
    runtimeStack: 'dotnet-isolated'
    runtimeVersion: '10.0'
    appSettings: [
      { name: 'CosmosDb__Endpoint', value: cosmosDb.outputs.endpoint }
      { name: 'CosmosDb__DatabaseName', value: projectName }
      { name: 'ServiceBus__ConnectionString', value: serviceBus.outputs.connectionString }
    ]
    keyVaultName: keyVault.outputs.name
  }
  dependsOn: [
    cosmosDb
    serviceBus
    storageAccount
  ]
}

module functionAppImport 'modules/function-app.bicep' = {
  name: 'deploy-function-app-import'
  scope: rg
  params: {
    name: names.functionAppImport
    location: location
    tags: tags
    appServicePlanId: appServicePlan.outputs.planId
    storageAccountName: storageAccount.outputs.name
    appInsightsConnectionString: appInsights.outputs.connectionString
    appInsightsInstrumentationKey: appInsights.outputs.instrumentationKey
    runtimeStack: 'dotnet-isolated'
    runtimeVersion: '10.0'
    appSettings: [
      { name: 'CosmosDb__Endpoint', value: cosmosDb.outputs.endpoint }
      { name: 'CosmosDb__DatabaseName', value: projectName }
      { name: 'ServiceBus__ConnectionString', value: serviceBus.outputs.connectionString }
      { name: 'Storage__ConnectionString', value: storageAccount.outputs.connectionString }
    ]
    keyVaultName: keyVault.outputs.name
  }
  dependsOn: [
    cosmosDb
    serviceBus
    storageAccount
  ]
}

// =============================================================================
// Web Hosting
// =============================================================================

module staticWebApp 'modules/static-web-app.bicep' = {
  name: 'deploy-static-web-app'
  scope: rg
  params: {
    name: names.staticWebApp
    location: location // Note: Static Web Apps have limited region support
    tags: tags
    sku: staticWebAppSku
    apiBackendUrl: 'https://${names.appService}.azurewebsites.net'
  }
}

// =============================================================================
// Outputs
// =============================================================================

output resourceGroupName string = rg.name
output resourceGroupId string = rg.id

// Data Services
output cosmosDbEndpoint string = cosmosDb.outputs.endpoint
output cosmosDbAccountName string = cosmosDb.outputs.accountName
output storageAccountName string = storageAccount.outputs.name
output redisHostName string = redis.outputs.hostName

// Compute Services
output appServiceUrl string = appService.outputs.defaultHostName
output functionAppSyncUrl string = functionAppSync.outputs.defaultHostName
output functionAppImportUrl string = functionAppImport.outputs.defaultHostName

// Web Hosting
output staticWebAppUrl string = staticWebApp.outputs.defaultHostName

// Security
output keyVaultName string = keyVault.outputs.name
output keyVaultUri string = keyVault.outputs.uri
output appConfigEndpoint string = appConfig.outputs.endpoint

// Messaging
output signalRHostName string = signalR.outputs.hostName
output serviceBusNamespace string = serviceBus.outputs.namespaceName

// Monitoring
output appInsightsConnectionString string = appInsights.outputs.connectionString
output logAnalyticsWorkspaceId string = logAnalytics.outputs.workspaceId
