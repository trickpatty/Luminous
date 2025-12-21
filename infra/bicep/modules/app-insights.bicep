// =============================================================================
// Application Insights Module
// =============================================================================
// Deploys an Azure Application Insights resource for application performance
// monitoring and diagnostics.
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/insights/component
// =============================================================================

@description('Name of the Application Insights resource')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('Resource ID of the Log Analytics workspace')
param logAnalyticsWorkspaceId string

@description('Application type')
@allowed(['web', 'other'])
param applicationType string = 'web'

@description('Enable sampling to reduce telemetry volume')
param enableSampling bool = true

@description('Sampling percentage (1-100)')
@minValue(1)
@maxValue(100)
param samplingPercentage int = 100

// =============================================================================
// Resource
// =============================================================================

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  tags: tags
  kind: applicationType
  properties: {
    Application_Type: applicationType
    WorkspaceResourceId: logAnalyticsWorkspaceId
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    SamplingPercentage: enableSampling ? samplingPercentage : 100
    RetentionInDays: 90
    DisableIpMasking: false
    DisableLocalAuth: false
  }
}

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of Application Insights')
output resourceId string = appInsights.id

@description('The name of Application Insights')
output name string = appInsights.name

@description('The instrumentation key for Application Insights')
output instrumentationKey string = appInsights.properties.InstrumentationKey

@description('The connection string for Application Insights')
output connectionString string = appInsights.properties.ConnectionString
