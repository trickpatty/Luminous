// =============================================================================
// App Configuration Module
// =============================================================================
// Deploys an Azure App Configuration store for centralized configuration
// management across all Luminous services.
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/app-configuration/configuration-store
// =============================================================================

@description('Name of the App Configuration store')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('SKU for App Configuration')
@allowed(['Free', 'Standard'])
param sku string = 'Free'

@description('Enable purge protection')
param enablePurgeProtection bool = false

@description('Soft delete retention in days')
@minValue(1)
@maxValue(7)
param softDeleteRetentionInDays int = 7

@description('Disable local authentication (use Azure AD only)')
param disableLocalAuth bool = false

// =============================================================================
// Resource
// =============================================================================

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    disableLocalAuth: disableLocalAuth
    enablePurgeProtection: sku == 'Standard' ? enablePurgeProtection : false
    softDeleteRetentionInDays: sku == 'Standard' ? softDeleteRetentionInDays : 0
    publicNetworkAccess: 'Enabled'
  }
}

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the App Configuration store')
output resourceId string = appConfig.id

@description('The name of the App Configuration store')
output name string = appConfig.name

@description('The endpoint of the App Configuration store')
output endpoint string = appConfig.properties.endpoint
