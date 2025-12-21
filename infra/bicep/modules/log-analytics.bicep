// =============================================================================
// Log Analytics Workspace Module
// =============================================================================
// Deploys an Azure Log Analytics workspace for centralized logging and
// monitoring of all Luminous resources.
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/operational-insights/workspace
// =============================================================================

@description('Name of the Log Analytics workspace')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('Data retention in days')
@minValue(30)
@maxValue(730)
param retentionInDays int = 30

@description('SKU for the workspace')
@allowed(['Free', 'PerGB2018', 'PerNode', 'Premium', 'Standalone', 'Standard'])
param sku string = 'PerGB2018'

@description('Daily quota for data ingestion in GB (-1 for unlimited)')
param dailyQuotaGb int = -1

// =============================================================================
// Resource
// =============================================================================

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      name: sku
    }
    retentionInDays: retentionInDays
    workspaceCapping: dailyQuotaGb > 0 ? {
      dailyQuotaGb: dailyQuotaGb
    } : null
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the Log Analytics workspace')
output workspaceId string = logAnalyticsWorkspace.id

@description('The name of the Log Analytics workspace')
output workspaceName string = logAnalyticsWorkspace.name

@description('The customer ID (workspace ID) for the Log Analytics workspace')
output customerId string = logAnalyticsWorkspace.properties.customerId
