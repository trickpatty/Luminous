// =============================================================================
// Static Web App Module
// =============================================================================
// Deploys an Azure Static Web App for hosting the Luminous Angular
// web application.
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/web/static-site
// =============================================================================

@description('Name of the Static Web App')
param name string

@description('Azure region for deployment (limited regions supported)')
@allowed(['westus2', 'centralus', 'eastus2', 'westeurope', 'eastasia', 'eastasiastage'])
param location string = 'eastus2'

@description('Tags to apply to the resource')
param tags object = {}

@description('Static Web App SKU')
@allowed(['Free', 'Standard'])
param sku string = 'Free'

@description('API backend URL for linked API')
param apiBackendUrl string = ''

@description('Staging environment policy')
@allowed(['Enabled', 'Disabled'])
param stagingEnvironmentPolicy string = 'Enabled'

@description('Allow config file to override defaults')
param allowConfigFileUpdates bool = true

// =============================================================================
// Resource
// =============================================================================

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
    tier: sku
  }
  properties: {
    stagingEnvironmentPolicy: stagingEnvironmentPolicy
    allowConfigFileUpdates: allowConfigFileUpdates
    provider: 'GitHub'
    buildProperties: {
      appLocation: '/clients/web'
      apiLocation: ''
      outputLocation: 'dist/luminous-web/browser'
      appBuildCommand: 'npm run build'
    }
  }
}

// =============================================================================
// Linked Backend (if API URL provided)
// =============================================================================

resource linkedBackend 'Microsoft.Web/staticSites/linkedBackends@2023-12-01' = if (!empty(apiBackendUrl)) {
  parent: staticWebApp
  name: 'api-backend'
  properties: {
    backendResourceId: ''
    region: location
  }
}

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the Static Web App')
output resourceId string = staticWebApp.id

@description('The name of the Static Web App')
output name string = staticWebApp.name

@description('The default hostname of the Static Web App')
output defaultHostName string = staticWebApp.properties.defaultHostname

@description('The deployment token for CI/CD')
#disable-next-line outputs-should-not-contain-secrets
output deploymentToken string = staticWebApp.listSecrets().properties.apiKey
