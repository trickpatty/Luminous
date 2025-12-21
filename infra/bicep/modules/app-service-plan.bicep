// =============================================================================
// App Service Plan Module
// =============================================================================
// Deploys an Azure App Service Plan for hosting the Luminous API and
// Azure Functions.
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/web/serverfarm
// =============================================================================

@description('Name of the App Service Plan')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('App Service Plan SKU name')
@allowed(['F1', 'D1', 'B1', 'B2', 'B3', 'S1', 'S2', 'S3', 'P1v2', 'P2v2', 'P3v2', 'P1v3', 'P2v3', 'P3v3', 'Y1', 'EP1', 'EP2', 'EP3'])
param skuName string = 'B1'

@description('App Service Plan kind')
@allowed(['linux', 'windows', 'functionapp', 'elastic'])
param kind string = 'linux'

@description('Enable zone redundancy')
param zoneRedundant bool = false

@description('Number of workers')
@minValue(1)
param workerCount int = 1

// =============================================================================
// Variables
// =============================================================================

var isLinux = kind == 'linux'
var isConsumption = skuName == 'Y1'
var isElasticPremium = startsWith(skuName, 'EP')
var reserved = isLinux

// =============================================================================
// Resource
// =============================================================================

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: name
  location: location
  tags: tags
  kind: kind
  sku: {
    name: skuName
    capacity: isConsumption ? 0 : workerCount
  }
  properties: {
    reserved: reserved
    zoneRedundant: zoneRedundant && !isConsumption
    targetWorkerCount: isConsumption ? 0 : workerCount
    targetWorkerSizeId: 0
    maximumElasticWorkerCount: isElasticPremium ? 20 : null
  }
}

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the App Service Plan')
output planId string = appServicePlan.id

@description('The name of the App Service Plan')
output planName string = appServicePlan.name
