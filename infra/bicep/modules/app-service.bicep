// =============================================================================
// App Service Module
// =============================================================================
// Deploys an Azure App Service for hosting the Luminous .NET API.
//
// TOGAF Principle: TP-1 - Azure-Native Stack
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/web/site
// =============================================================================

@description('Name of the App Service')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('Resource ID of the App Service Plan')
param appServicePlanId string

@description('Application Insights connection string')
param appInsightsConnectionString string

@description('Application Insights instrumentation key')
param appInsightsInstrumentationKey string

@description('Runtime stack')
@allowed(['DOTNETCORE', 'DOTNET', 'NODE', 'PYTHON', 'JAVA'])
param runtimeStack string = 'DOTNETCORE'

@description('Runtime version')
param runtimeVersion string = '10.0'

@description('Additional app settings')
param appSettings array = []

@description('Key Vault name for secret references')
param keyVaultName string = ''

@description('Enable managed identity')
param enableManagedIdentity bool = true

@description('Always On setting')
param alwaysOn bool = true

@description('Minimum TLS version')
@allowed(['1.0', '1.1', '1.2'])
param minTlsVersion string = '1.2'

// =============================================================================
// Variables
// =============================================================================

var linuxFxVersion = '${runtimeStack}|${runtimeVersion}'

var baseAppSettings = [
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: appInsightsConnectionString
  }
  {
    name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
    value: '~3'
  }
  {
    name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
    value: appInsightsInstrumentationKey
  }
  {
    name: 'XDT_MicrosoftApplicationInsights_Mode'
    value: 'recommended'
  }
]

var allAppSettings = concat(baseAppSettings, appSettings)

// =============================================================================
// Resource
// =============================================================================

resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  kind: 'app,linux'
  identity: enableManagedIdentity ? {
    type: 'SystemAssigned'
  } : null
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      alwaysOn: alwaysOn
      http20Enabled: true
      minTlsVersion: minTlsVersion
      ftpsState: 'Disabled'
      appSettings: allAppSettings
      cors: {
        allowedOrigins: [
          'https://*.azurestaticapps.net'
          'http://localhost:4200'
          'http://localhost:3000'
        ]
        supportCredentials: true
      }
      healthCheckPath: '/health'
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
        objectId: appService.identity.principalId
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

@description('The resource ID of the App Service')
output resourceId string = appService.id

@description('The name of the App Service')
output name string = appService.name

@description('The default hostname of the App Service')
output defaultHostName string = appService.properties.defaultHostName

@description('The principal ID of the managed identity')
output principalId string = enableManagedIdentity ? appService.identity.principalId : ''
