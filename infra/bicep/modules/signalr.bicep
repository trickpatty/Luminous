// =============================================================================
// SignalR Service Module
// =============================================================================
// Deploys an Azure SignalR Service for real-time communication between
// clients and the Luminous backend API.
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/signal-r-service/signal-r
// =============================================================================

@description('Name of the SignalR Service')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('SignalR Service SKU')
@allowed(['Free_F1', 'Standard_S1', 'Premium_P1'])
param sku string = 'Free_F1'

@description('SignalR Service mode')
@allowed(['Default', 'Serverless', 'Classic'])
param serviceMode string = 'Default'

@description('Number of SignalR units')
@minValue(1)
@maxValue(100)
param capacity int = 1

@description('Enable connectivity logs')
param enableConnectivityLogs bool = true

@description('Enable messaging logs')
param enableMessagingLogs bool = false

// =============================================================================
// Variables
// =============================================================================

var skuName = sku
var skuTier = startsWith(sku, 'Free') ? 'Free' : (startsWith(sku, 'Standard') ? 'Standard' : 'Premium')

// =============================================================================
// Resource
// =============================================================================

resource signalR 'Microsoft.SignalRService/signalR@2023-08-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuTier
    capacity: skuTier == 'Free' ? 1 : capacity
  }
  kind: 'SignalR'
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: serviceMode
      }
      {
        flag: 'EnableConnectivityLogs'
        value: enableConnectivityLogs ? 'True' : 'False'
      }
      {
        flag: 'EnableMessagingLogs'
        value: enableMessagingLogs ? 'True' : 'False'
      }
    ]
    cors: {
      allowedOrigins: ['*']
    }
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    disableAadAuth: false
  }
}

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the SignalR Service')
output resourceId string = signalR.id

@description('The name of the SignalR Service')
output name string = signalR.name

@description('The hostname of the SignalR Service')
output hostName string = signalR.properties.hostName

@description('The connection string for the SignalR Service')
#disable-next-line outputs-should-not-contain-secrets
output connectionString string = signalR.listKeys().primaryConnectionString
