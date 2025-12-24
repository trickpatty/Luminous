// =============================================================================
// Luminous - Main Infrastructure Deployment
// =============================================================================
// Deploys Luminous Azure infrastructure using Azure Verified Modules (AVMs)
// directly from the public Bicep registry.
//
// Usage:
//   az deployment sub create \
//     --location <region> \
//     --template-file main.bicep \
//     --parameters @parameters/dev.bicepparam
//
// TOGAF Principle: TP-4 - Infrastructure as Code
// ADR-007: Bicep with AVMs for IaC
// =============================================================================

targetScope = 'subscription'

// =============================================================================
// Parameters
// =============================================================================

@description('Environment name (dev, stg, prd)')
@allowed(['dev', 'stg', 'prd'])
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
param appServiceSkuName string = environment == 'prd' ? 'P1v3' : 'B1'

// Redis Configuration
@description('Redis Cache SKU')
@allowed(['Basic', 'Standard', 'Premium'])
param redisSku string = environment == 'prd' ? 'Standard' : 'Basic'

@description('Redis Cache Capacity')
param redisCapacity int = environment == 'prd' ? 1 : 0

// SignalR Configuration
@description('SignalR Service SKU')
param signalRSku string = environment == 'prd' ? 'Standard_S1' : 'Free_F1'

// Static Web App Configuration
@description('Static Web App SKU')
@allowed(['Free', 'Standard'])
param staticWebAppSku string = environment == 'prd' ? 'Standard' : 'Free'

// =============================================================================
// Variables
// =============================================================================

var resourceGroupName = 'rg-${projectPrefix}-${environment}'
var namingPrefix = '${projectPrefix}-${environment}'

// Resource naming following Azure naming conventions
var names = {
  cosmosDb: 'cosmos-${namingPrefix}'
  storageAccount: 'st${projectPrefix}${environment}'
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

// Cosmos DB container configurations for multi-tenant data isolation
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
  { name: 'credentials', partitionKeyPath: '/userId' }
]

// =============================================================================
// Resource Group (using AVM)
// =============================================================================

module rg 'br/public:avm/res/resources/resource-group:0.4.0' = {
  name: 'deploy-resource-group'
  params: {
    name: resourceGroupName
    location: location
    tags: tags
  }
}

// =============================================================================
// Monitoring (Deploy first - other resources depend on these)
// =============================================================================

module logAnalytics 'br/public:avm/res/operational-insights/workspace:0.9.0' = {
  name: 'deploy-log-analytics'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.logAnalytics
    location: location
    tags: tags
    dataRetention: environment == 'prd' ? 90 : 30
    skuName: 'PerGB2018'
  }
  dependsOn: [rg]
}

module appInsights 'br/public:avm/res/insights/component:0.4.1' = {
  name: 'deploy-app-insights'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.appInsights
    location: location
    tags: tags
    workspaceResourceId: logAnalytics.outputs.resourceId
    applicationType: 'web'
  }
  dependsOn: [rg]
}

// =============================================================================
// Security
// =============================================================================

module keyVault 'br/public:avm/res/key-vault/vault:0.9.0' = {
  name: 'deploy-key-vault'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.keyVault
    location: location
    tags: tags
    sku: 'standard'
    enableRbacAuthorization: true
    enableSoftDelete: environment == 'prd'
    softDeleteRetentionInDays: 90
    enablePurgeProtection: environment == 'prd'
  }
  dependsOn: [rg]
}

module appConfig 'br/public:avm/res/app-configuration/configuration-store:0.5.1' = {
  name: 'deploy-app-config'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.appConfig
    location: location
    tags: tags
    sku: environment == 'prd' ? 'Standard' : 'Free'
  }
  dependsOn: [rg]
}

// =============================================================================
// Data Services
// =============================================================================

module cosmosDb 'br/public:avm/res/document-db/database-account:0.8.1' = {
  name: 'deploy-cosmos-db'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.cosmosDb
    location: location
    tags: tags
    capabilitiesToAdd: cosmosDbServerless ? ['EnableServerless'] : []
    defaultConsistencyLevel: cosmosDbConsistencyLevel
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    sqlDatabases: [
      {
        name: projectName
        containers: [for container in cosmosContainers: {
          name: container.name
          paths: [container.partitionKeyPath]
        }]
      }
    ]
  }
  dependsOn: [rg]
}

module storageAccount 'br/public:avm/res/storage/storage-account:0.14.3' = {
  name: 'deploy-storage-account'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.storageAccount
    location: location
    tags: tags
    skuName: environment == 'prd' ? 'Standard_GRS' : 'Standard_LRS'
    kind: 'StorageV2'
    allowBlobPublicAccess: false
    blobServices: {
      containers: [
        { name: 'avatars', publicAccess: 'None' }
        { name: 'recipes', publicAccess: 'None' }
        { name: 'imports', publicAccess: 'None' }
        { name: 'exports', publicAccess: 'None' }
      ]
      deleteRetentionPolicy: {
        enabled: true
        days: 7
      }
    }
  }
  dependsOn: [rg]
}

module redis 'br/public:avm/res/cache/redis:0.8.0' = {
  name: 'deploy-redis-cache'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.redis
    location: location
    tags: tags
    skuName: redisSku
    capacity: redisCapacity
    redisVersion: '6'
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
  dependsOn: [rg]
}

// =============================================================================
// Messaging Services
// =============================================================================

module serviceBus 'br/public:avm/res/service-bus/namespace:0.10.0' = {
  name: 'deploy-service-bus'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.serviceBus
    location: location
    tags: tags
    skuObject: {
      name: environment == 'prd' ? 'Standard' : 'Basic'
    }
    queues: [
      { name: 'calendar-sync' }
      { name: 'import-processing' }
      { name: 'notifications' }
    ]
  }
  dependsOn: [rg]
}

module signalR 'br/public:avm/res/signal-r-service/signal-r:0.5.0' = {
  name: 'deploy-signalr'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.signalR
    location: location
    tags: tags
    sku: signalRSku
    kind: 'SignalR'
    features: [
      { flag: 'ServiceMode', value: 'Default' }
      { flag: 'EnableConnectivityLogs', value: 'True' }
    ]
  }
  dependsOn: [rg]
}

// =============================================================================
// Compute Services
// =============================================================================

module appServicePlan 'br/public:avm/res/web/serverfarm:0.3.0' = {
  name: 'deploy-app-service-plan'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.appServicePlan
    location: location
    tags: tags
    skuName: appServiceSkuName
    skuCapacity: 1
    kind: 'Linux'
    reserved: true
  }
  dependsOn: [rg]
}

module appService 'br/public:avm/res/web/site:0.11.1' = {
  name: 'deploy-app-service'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.appService
    location: location
    tags: tags
    kind: 'app,linux'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    managedIdentities: {
      systemAssigned: true
    }
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: appServiceSkuName != 'F1' && appServiceSkuName != 'D1'
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
    }
    httpsOnly: true
    clientAffinityEnabled: false
    appSettingsKeyValuePairs: {
      ASPNETCORE_ENVIRONMENT: environment == 'prd' ? 'Production' : 'Development'
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
      CosmosDb__Endpoint: cosmosDb.outputs.endpoint
      CosmosDb__DatabaseName: projectName
      SignalR__Endpoint: 'https://${signalR.outputs.name}.service.signalr.net'
      AppConfig__Endpoint: appConfig.outputs.endpoint
    }
  }
}

module functionAppSync 'br/public:avm/res/web/site:0.11.1' = {
  name: 'deploy-function-app-sync'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.functionAppSync
    location: location
    tags: tags
    kind: 'functionapp,linux'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    managedIdentities: {
      systemAssigned: true
    }
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|9.0'
      use32BitWorkerProcess: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
    appSettingsKeyValuePairs: {
      FUNCTIONS_EXTENSION_VERSION: '~4'
      FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
      AzureWebJobsStorage: storageAccount.outputs.primaryBlobEndpoint
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
      CosmosDb__Endpoint: cosmosDb.outputs.endpoint
      CosmosDb__DatabaseName: projectName
    }
  }
}

module functionAppImport 'br/public:avm/res/web/site:0.11.1' = {
  name: 'deploy-function-app-import'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.functionAppImport
    location: location
    tags: tags
    kind: 'functionapp,linux'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    managedIdentities: {
      systemAssigned: true
    }
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|9.0'
      use32BitWorkerProcess: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
    appSettingsKeyValuePairs: {
      FUNCTIONS_EXTENSION_VERSION: '~4'
      FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
      AzureWebJobsStorage: storageAccount.outputs.primaryBlobEndpoint
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
      CosmosDb__Endpoint: cosmosDb.outputs.endpoint
      CosmosDb__DatabaseName: projectName
    }
  }
}

// =============================================================================
// Web Hosting
// =============================================================================

module staticWebApp 'br/public:avm/res/web/static-site:0.6.0' = {
  name: 'deploy-static-web-app'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: names.staticWebApp
    location: location
    tags: tags
    sku: staticWebAppSku
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
  }
  dependsOn: [rg]
}

// =============================================================================
// Outputs
// =============================================================================

output resourceGroupName string = resourceGroupName
output resourceGroupId string = rg.outputs.resourceId

// Data Services
output cosmosDbEndpoint string = cosmosDb.outputs.endpoint
output cosmosDbAccountName string = cosmosDb.outputs.name
output storageAccountName string = storageAccount.outputs.name
output redisHostName string = redis.outputs.hostName

// Compute Services
output appServiceUrl string = appService.outputs.defaultHostname
output functionAppSyncUrl string = functionAppSync.outputs.defaultHostname
output functionAppImportUrl string = functionAppImport.outputs.defaultHostname

// Web Hosting
output staticWebAppUrl string = staticWebApp.outputs.defaultHostname

// Security
output keyVaultName string = keyVault.outputs.name
output keyVaultUri string = keyVault.outputs.uri
output appConfigEndpoint string = appConfig.outputs.endpoint

// Messaging
output signalREndpoint string = 'https://${signalR.outputs.name}.service.signalr.net'
output serviceBusNamespace string = serviceBus.outputs.name

// Monitoring
output appInsightsConnectionString string = appInsights.outputs.connectionString
output logAnalyticsWorkspaceId string = logAnalytics.outputs.resourceId
