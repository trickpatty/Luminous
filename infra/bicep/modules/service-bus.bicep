// =============================================================================
// Service Bus Module
// =============================================================================
// Deploys an Azure Service Bus namespace with queues for async message
// processing (calendar sync, import processing, notifications).
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/service-bus/namespace
// =============================================================================

@description('Name of the Service Bus namespace')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('Service Bus SKU')
@allowed(['Basic', 'Standard', 'Premium'])
param sku string = 'Basic'

@description('Queue names to create')
param queues array = []

@description('Enable zone redundancy (Premium SKU only)')
param zoneRedundant bool = false

@description('Minimum TLS version')
@allowed(['1.0', '1.1', '1.2'])
param minimumTlsVersion string = '1.2'

// =============================================================================
// Resource - Service Bus Namespace
// =============================================================================

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
    tier: sku
  }
  properties: {
    minimumTlsVersion: minimumTlsVersion
    publicNetworkAccess: 'Enabled'
    zoneRedundant: sku == 'Premium' ? zoneRedundant : false
    disableLocalAuth: false
  }
}

// =============================================================================
// Resource - Service Bus Queues
// =============================================================================

resource serviceBusQueues 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = [for queueName in queues: {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    lockDuration: 'PT1M'
    maxSizeInMegabytes: sku == 'Premium' ? 81920 : 1024
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: true
    enableBatchedOperations: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    enablePartitioning: sku != 'Premium'
    enableExpress: false
  }
}]

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the Service Bus namespace')
output resourceId string = serviceBusNamespace.id

@description('The name of the Service Bus namespace')
output namespaceName string = serviceBusNamespace.name

@description('The fully qualified domain name of the Service Bus namespace')
output fullyQualifiedDomainName string = '${serviceBusNamespace.name}.servicebus.windows.net'

@description('The connection string for the Service Bus namespace')
#disable-next-line outputs-should-not-contain-secrets
output connectionString string = 'Endpoint=sb://${serviceBusNamespace.name}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=${listKeys('${serviceBusNamespace.id}/authorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryKey}'
