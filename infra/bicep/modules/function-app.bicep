// =============================================================================
// Function App Module
// =============================================================================
// Deploys an Azure Function App for serverless processing (calendar sync,
// import processing, background jobs).
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/web/site
// =============================================================================

@description('Name of the Function App')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('Resource ID of the App Service Plan')
param appServicePlanId string

@description('Name of the Storage Account for function storage')
param storageAccountName string

@description('Application Insights connection string')
param appInsightsConnectionString string

@description('Application Insights instrumentation key')
param appInsightsInstrumentationKey string

@description('Runtime stack')
@allowed(['dotnet-isolated', 'dotnet', 'node', 'python', 'java', 'powershell'])
param runtimeStack string = 'dotnet-isolated'

@description('Runtime version')
param runtimeVersion string = '10.0'

@description('Additional app settings')
param appSettings array = []

@description('Key Vault name for secret references')
param keyVaultName string = ''

@description('Enable managed identity')
param enableManagedIdentity bool = true

// =============================================================================
// Variables
// =============================================================================

var workerRuntime = runtimeStack == 'dotnet-isolated' ? 'dotnet-isolated' : runtimeStack

var baseAppSettings = [
  {
    name: 'AzureWebJobsStorage'
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
  }
  {
    name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
  }
  {
    name: 'WEBSITE_CONTENTSHARE'
    value: toLower(name)
  }
  {
    name: 'FUNCTIONS_EXTENSION_VERSION'
    value: '~4'
  }
  {
    name: 'FUNCTIONS_WORKER_RUNTIME'
    value: workerRuntime
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: appInsightsConnectionString
  }
  {
    name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
    value: appInsightsInstrumentationKey
  }
]

var allAppSettings = concat(baseAppSettings, appSettings)

// =============================================================================
// Existing Resources
// =============================================================================

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

// =============================================================================
// Resource
// =============================================================================

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  kind: 'functionapp,linux'
  identity: enableManagedIdentity ? {
    type: 'SystemAssigned'
  } : null
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|${runtimeVersion}'
      use32BitWorkerProcess: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: allAppSettings
    }
    clientAffinityEnabled: false
  }
}

// =============================================================================
// Key Vault Access Policy (if Key Vault name provided)
// =============================================================================

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = if (!empty(keyVaultName) && enableManagedIdentity) {
  name: '${keyVaultName}/add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: functionApp.identity.principalId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
  }
}

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the Function App')
output resourceId string = functionApp.id

@description('The name of the Function App')
output name string = functionApp.name

@description('The default hostname of the Function App')
output defaultHostName string = functionApp.properties.defaultHostName

@description('The principal ID of the managed identity')
output principalId string = enableManagedIdentity ? functionApp.identity.principalId : ''
