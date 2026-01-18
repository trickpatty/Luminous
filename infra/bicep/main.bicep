// =============================================================================
// Luminous - Main Infrastructure Deployment
// =============================================================================
// Deploys Luminous Azure infrastructure using Azure Verified Modules (AVMs)
// directly from the public Bicep registry.
//
// Prerequisites:
//   Resource group must exist before deployment. Create with:
//   az group create --name rg-lum-<env> --location eastus2
//
// Usage:
//   az deployment group create \
//     --resource-group rg-lum-<env> \
//     --template-file main.bicep \
//     --parameters @parameters/dev.bicepparam
//
// TOGAF Principle: TP-4 - Infrastructure as Code
// ADR-007: Bicep with AVMs for IaC
// =============================================================================

// Default scope is resourceGroup - no targetScope needed

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
@description('Enable serverless mode for Cosmos DB (recommended for cost optimization)')
param cosmosDbServerless bool = true

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

// Role Assignments Configuration
@description('Deploy role assignments for managed identities. Set to false if deploying identity lacks User Access Administrator/Owner role.')
param deployRoleAssignments bool = true

// Static Web App Configuration
@description('Static Web App SKU')
@allowed(['Free', 'Standard'])
param staticWebAppSku string = environment == 'prd' ? 'Standard' : 'Free'

// Custom Domain Configuration
@description('Custom domain for the Static Web App (e.g., luminousfamily.com). Leave empty to skip DNS zone creation.')
param customDomain string = ''

@description('Deploy DNS zone for custom domain. Set to true to create the DNS zone in Azure.')
param deployDnsZone bool = false

// =============================================================================
// Variables
// =============================================================================

var namingPrefix = '${projectPrefix}-${environment}'

// 6-character unique suffix based on resource group ID for globally unique names
var uniqueSuffix = take(uniqueString(resourceGroup().id), 6)

// Resource naming following Azure naming conventions
// Resources requiring global uniqueness include the unique suffix
var names = {
  // Globally unique names (include suffix)
  cosmosDb: 'cosmos-${namingPrefix}-${uniqueSuffix}'
  storageAccount: 'st${projectPrefix}${environment}${uniqueSuffix}'
  keyVault: 'kv-${namingPrefix}-${uniqueSuffix}'
  appConfig: 'appcs-${namingPrefix}-${uniqueSuffix}'
  appService: 'app-${namingPrefix}-${uniqueSuffix}'
  functionAppSync: 'func-${namingPrefix}-${uniqueSuffix}-sync'
  functionAppImport: 'func-${namingPrefix}-${uniqueSuffix}-import'
  signalR: 'sigr-${namingPrefix}-${uniqueSuffix}'
  serviceBus: 'sb-${namingPrefix}-${uniqueSuffix}'
  redis: 'redis-${namingPrefix}-${uniqueSuffix}'
  staticWebApp: 'stapp-${namingPrefix}-${uniqueSuffix}'
  communicationServices: 'acs-${namingPrefix}-${uniqueSuffix}'
  emailService: 'email-${namingPrefix}-${uniqueSuffix}'
  // Resource group scoped (no suffix needed)
  appServicePlan: 'asp-${namingPrefix}'
  logAnalytics: 'log-${namingPrefix}'
  appInsights: 'appi-${namingPrefix}'
}

// Constructed CosmosDB endpoint to avoid circular dependency with role assignments
// This allows us to use sqlRoleAssignments in the CosmosDB module
var cosmosDbEndpoint = 'https://${names.cosmosDb}.documents.azure.com:443/'

// Azure Built-in Role IDs
// https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
var builtInRoles = {
  keyVaultSecretsUser: '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secrets User
}

// Custom domain configuration
var hasCustomDomain = !empty(customDomain)
var dnsZoneName = customDomain // DNS zone name is the root domain

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
  { name: 'otptokens', partitionKeyPath: '/email' }
  { name: 'refreshtokens', partitionKeyPath: '/userId' }
  { name: 'calendarconnections', partitionKeyPath: '/familyId' }
  { name: 'oauthsessions', partitionKeyPath: '/familyId' }
]

// =============================================================================
// Monitoring (Deploy first - other resources depend on these)
// =============================================================================

module logAnalytics 'br/public:avm/res/operational-insights/workspace:0.14.2' = {
  name: 'deploy-log-analytics'
  params: {
    name: names.logAnalytics
    location: location
    tags: tags
    dataRetention: environment == 'prd' ? 90 : 30
    skuName: 'PerGB2018'
  }
}

module appInsights 'br/public:avm/res/insights/component:0.7.1' = {
  name: 'deploy-app-insights'
  params: {
    name: names.appInsights
    location: location
    tags: tags
    workspaceResourceId: logAnalytics.outputs.resourceId
    applicationType: 'web'
  }
}

// =============================================================================
// Security
// =============================================================================

module keyVault 'br/public:avm/res/key-vault/vault:0.13.3' = {
  name: 'deploy-key-vault'
  params: {
    name: names.keyVault
    location: location
    tags: tags
    sku: 'standard'
    enableRbacAuthorization: true
    enableSoftDelete: environment == 'prd'
    softDeleteRetentionInDays: 90
    enablePurgeProtection: environment == 'prd'
    // Grant Key Vault Secrets User role to App Service and Function Apps
    // This allows them to read secrets using @Microsoft.KeyVault references
    // Note: Role assignments require the deploying identity to have User Access Administrator or Owner role
    // Set deployRoleAssignments=false if the deploying identity lacks these permissions
    roleAssignments: deployRoleAssignments ? [
      {
        principalId: appService.outputs.systemAssignedMIPrincipalId!
        principalType: 'ServicePrincipal'
        roleDefinitionIdOrName: builtInRoles.keyVaultSecretsUser
      }
      {
        principalId: functionAppSync.outputs.systemAssignedMIPrincipalId!
        principalType: 'ServicePrincipal'
        roleDefinitionIdOrName: builtInRoles.keyVaultSecretsUser
      }
      {
        principalId: functionAppImport.outputs.systemAssignedMIPrincipalId!
        principalType: 'ServicePrincipal'
        roleDefinitionIdOrName: builtInRoles.keyVaultSecretsUser
      }
    ] : []
  }
}

module appConfig 'br/public:avm/res/app-configuration/configuration-store:0.9.2' = {
  name: 'deploy-app-config'
  params: {
    name: names.appConfig
    location: location
    tags: tags
    sku: environment == 'prd' ? 'Standard' : 'Free'
  }
}

// =============================================================================
// Data Services
// =============================================================================

module cosmosDb 'br/public:avm/res/document-db/database-account:0.18.0' = {
  name: 'deploy-cosmos-db'
  params: {
    name: names.cosmosDb
    location: location
    tags: tags
    // Disable key-based authentication - enforce AAD/RBAC auth only
    // TOGAF Principle: DP-4 (Data Minimization), Security best practice
    disableLocalAuthentication: true
    capabilitiesToAdd: cosmosDbServerless ? ['EnableServerless'] : []
    defaultConsistencyLevel: cosmosDbConsistencyLevel
    // Network configuration: Allow Azure services and Portal access
    // https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/document-db/database-account
    networkRestrictions: {
      publicNetworkAccess: 'Enabled'
      networkAclBypass: 'AzureServices' // Allows Azure services (App Service, Functions) and Azure Portal
    }
    failoverLocations: [
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
    // Grant managed identities data plane access using AAD authentication
    // Cosmos DB Built-in Data Contributor role: 00000000-0000-0000-0000-000000000002
    // Note: Cosmos DB SQL role assignments are separate from Azure RBAC but included in the same conditional
    sqlRoleAssignments: deployRoleAssignments ? [
      {
        principalId: appService.outputs.systemAssignedMIPrincipalId!
        roleDefinitionId: '00000000-0000-0000-0000-000000000002'
      }
      {
        principalId: functionAppSync.outputs.systemAssignedMIPrincipalId!
        roleDefinitionId: '00000000-0000-0000-0000-000000000002'
      }
      {
        principalId: functionAppImport.outputs.systemAssignedMIPrincipalId!
        roleDefinitionId: '00000000-0000-0000-0000-000000000002'
      }
    ] : []
  }
}

module storageAccount 'br/public:avm/res/storage/storage-account:0.31.0' = {
  name: 'deploy-storage-account'
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
      deleteRetentionPolicyEnabled: true
      deleteRetentionPolicyDays: 7
    }
  }
}

module redis 'br/public:avm/res/cache/redis:0.16.4' = {
  name: 'deploy-redis-cache'
  params: {
    name: names.redis
    location: location
    tags: tags
    skuName: redisSku
    capacity: redisCapacity
    redisVersion: '6'
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    // Export Redis connection string to Key Vault for secure access
    secretsExportConfiguration: {
      keyVaultResourceId: keyVault.outputs.resourceId
      primaryConnectionStringName: 'redis-connection-string'
    }
  }
}

// =============================================================================
// Messaging Services
// =============================================================================

module serviceBus 'br/public:avm/res/service-bus/namespace:0.16.0' = {
  name: 'deploy-service-bus'
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
}

module signalR 'br/public:avm/res/signal-r-service/signal-r:0.10.1' = {
  name: 'deploy-signalr'
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
}

// =============================================================================
// Communication Services (Email)
// =============================================================================

// Email Communication Service with Azure-managed domain for sending emails
module emailService 'br/public:avm/res/communication/email-service:0.4.2' = {
  name: 'deploy-email-service'
  params: {
    name: names.emailService
    location: 'global' // Email service is global
    tags: tags
    dataLocation: 'United States' // Data residency
    domains: [
      {
        name: 'AzureManagedDomain' // Use Azure-managed domain for easy setup
        domainManagement: 'AzureManaged'
        userEngagementTracking: 'Disabled'
        senderUsernames: [
          {
            name: 'donotreply'
            username: 'donotreply'
            displayName: 'Luminous'
          }
        ]
      }
    ]
  }
}

// Communication Service linked to the Email Service for sending emails
module communicationServices 'br/public:avm/res/communication/communication-service:0.4.2' = {
  name: 'deploy-communication-services'
  params: {
    name: names.communicationServices
    location: 'global' // ACS is a global service
    tags: tags
    dataLocation: 'United States' // Data residency
    linkedDomains: [
      emailService.outputs.domainResourceIds[0] // Link the Azure-managed domain
    ]
  }
}

// =============================================================================
// Key Vault Secrets (deployed after Key Vault and services are ready)
// =============================================================================

// Reference the Key Vault for storing secrets
resource keyVaultRef 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: names.keyVault
}

// Reference the deployed Communication Service to get its connection string
resource communicationServicesRef 'Microsoft.Communication/communicationServices@2023-04-01' existing = {
  name: names.communicationServices
}

// Store ACS connection string in Key Vault
// This allows App Service to securely access the connection string via Key Vault reference
resource acsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultRef
  name: 'acs-connection-string'
  properties: {
    value: communicationServicesRef.listKeys().primaryConnectionString
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
  dependsOn: [keyVault, communicationServices]
}

// Reference the Email Service domain to get the mail-from domain for sender address
resource emailDomainRef 'Microsoft.Communication/emailServices/domains@2023-04-01' existing = {
  name: '${names.emailService}/AzureManagedDomain'
}

// Generate and store JWT secret key in Key Vault
// This is used for signing JWT tokens - must be at least 32 characters for HMACSHA256
resource jwtSecretKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultRef
  name: 'jwt-secret-key'
  properties: {
    // Generate a unique secret key based on resource group ID and a salt
    // This ensures each environment has a different secret key
    value: '${uniqueString(resourceGroup().id, 'jwt-secret')}${uniqueString(subscription().id, 'jwt-luminous')}${uniqueString(resourceGroup().id, 'auth-key')}'
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
  dependsOn: [keyVault]
}

// Calendar OAuth secrets - these must be populated manually after deployment
// Get credentials from Google Cloud Console and Azure AD App Registration
resource calendarGoogleClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultRef
  name: 'calendar-google-client-id'
  properties: {
    // Placeholder - must be updated with actual Google OAuth Client ID from Google Cloud Console
    value: 'REPLACE_WITH_GOOGLE_CLIENT_ID'
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
  dependsOn: [keyVault]
}

resource calendarGoogleClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultRef
  name: 'calendar-google-client-secret'
  properties: {
    // Placeholder - must be updated with actual Google OAuth Client Secret
    value: 'REPLACE_WITH_GOOGLE_CLIENT_SECRET'
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
  dependsOn: [keyVault]
}

resource calendarMicrosoftClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultRef
  name: 'calendar-microsoft-client-id'
  properties: {
    // Placeholder - must be updated with actual Microsoft/Azure AD App Client ID
    value: 'REPLACE_WITH_MICROSOFT_CLIENT_ID'
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
  dependsOn: [keyVault]
}

resource calendarMicrosoftClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVaultRef
  name: 'calendar-microsoft-client-secret'
  properties: {
    // Placeholder - must be updated with actual Microsoft/Azure AD App Client Secret
    value: 'REPLACE_WITH_MICROSOFT_CLIENT_SECRET'
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
  dependsOn: [keyVault]
}

// =============================================================================
// Web Hosting (Static Web App deployed first for CORS configuration)
// =============================================================================

module staticWebApp 'br/public:avm/res/web/static-site:0.9.3' = {
  name: 'deploy-static-web-app'
  params: {
    name: names.staticWebApp
    location: location
    tags: tags
    sku: staticWebAppSku
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
  }
}

// =============================================================================
// DNS Zone & Custom Domain (Optional - for production deployments)
// =============================================================================
// Deploy DNS Zone if customDomain is specified and deployDnsZone is true.
// For OSS users: Set customDomain parameter to your own domain.
// After deployment, configure your domain registrar's nameservers to point to Azure DNS.

module dnsZone 'br/public:avm/res/network/dns-zone:0.5.4' = if (hasCustomDomain && deployDnsZone) {
  name: 'deploy-dns-zone'
  params: {
    name: dnsZoneName
    location: 'global' // DNS zones are global resources
    tags: tags
  }
}

// Custom domain binding for Static Web App
// NOTE: Apex domains (e.g., luminousfamily.com) MUST use 'dns-txt-token' validation.
// Subdomains can use CNAME validation, but dns-txt-token works for both cases.
// Azure Static Web Apps automatically provisions SSL certificates for custom domains.
resource staticWebAppCustomDomain 'Microsoft.Web/staticSites/customDomains@2023-12-01' = if (hasCustomDomain) {
  name: '${names.staticWebApp}/${customDomain}'
  properties: {
    validationMethod: 'dns-txt-token'
  }
  dependsOn: [staticWebApp]
}

// Create CNAME record in DNS zone pointing to Static Web App
// This is only created if we're deploying the DNS zone
resource wwwCname 'Microsoft.Network/dnsZones/CNAME@2023-07-01-preview' = if (hasCustomDomain && deployDnsZone) {
  name: '${dnsZoneName}/www'
  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: staticWebApp.outputs.defaultHostname
    }
  }
  dependsOn: [dnsZone]
}

// Create A record with Azure alias for apex domain (root domain without www)
// Static Web Apps support apex domains via Azure DNS ALIAS records
resource apexAlias 'Microsoft.Network/dnsZones/A@2023-07-01-preview' = if (hasCustomDomain && deployDnsZone) {
  name: '${dnsZoneName}/@'
  properties: {
    TTL: 3600
    targetResource: {
      id: staticWebApp.outputs.resourceId
    }
  }
  dependsOn: [dnsZone]
}

// =============================================================================
// Compute Services
// =============================================================================

module appServicePlan 'br/public:avm/res/web/serverfarm:0.5.0' = {
  name: 'deploy-app-service-plan'
  params: {
    name: names.appServicePlan
    location: location
    tags: tags
    skuName: appServiceSkuName
    skuCapacity: 1
    kind: 'linux'
    reserved: true
  }
}

module appService 'br/public:avm/res/web/site:0.19.4' = {
  name: 'deploy-app-service'
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
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: appServiceSkuName != 'F1' && appServiceSkuName != 'D1'
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
    }
    httpsOnly: true
    clientAffinityEnabled: false
    // Send diagnostic logs to Log Analytics (linked to Application Insights)
    diagnosticSettings: [
      {
        workspaceResourceId: logAnalytics.outputs.resourceId
        logCategoriesAndGroups: [
          { categoryGroup: 'allLogs' }
        ]
        metricCategories: [
          { category: 'AllMetrics' }
        ]
      }
    ]
    // Note: App settings are deployed separately to avoid circular dependency with Key Vault secrets
  }
}

// Deploy App Service settings after all secrets are created in Key Vault
// This avoids circular dependency: App Service needs secrets, but Key Vault needs App Service's managed identity
module appServiceSettings 'br/public:avm/res/web/site/config:0.1.1' = {
  name: 'deploy-app-service-settings'
  params: {
    appName: names.appService
    name: 'appsettings'
    properties: {
      ASPNETCORE_ENVIRONMENT: environment == 'prd' ? 'Production' : 'Development'
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
      CosmosDb__AccountEndpoint: cosmosDbEndpoint
      CosmosDb__DatabaseName: projectName
      CosmosDb__UseManagedIdentity: 'true'
      SignalR__Endpoint: 'https://${signalR.outputs.name}.service.signalr.net'
      AppConfig__Endpoint: appConfig.outputs.endpoint
      // CORS: Allow Static Web App origin for direct API calls
      Cors__AllowedOrigins__0: 'https://${staticWebApp.outputs.defaultHostname}'
      Cors__AllowedOrigins__1: 'http://localhost:4200'
      Cors__AllowedOrigins__2: 'https://localhost:4200'
      // Custom domain CORS origins (only added if custom domain is configured)
      Cors__AllowedOrigins__3: hasCustomDomain ? 'https://${customDomain}' : ''
      Cors__AllowedOrigins__4: hasCustomDomain ? 'https://www.${customDomain}' : ''
      // Redis cache for distributed session/cache (WebAuthn sessions, etc.)
      // Connection string is securely stored in Key Vault by the Redis module
      Redis__ConnectionString: '@Microsoft.KeyVault(VaultName=${names.keyVault};SecretName=redis-connection-string)'
      Redis__InstanceName: 'luminous-${environment}:'
      // Email settings - Azure deployments use ACS, local dev uses console logging
      Email__UseDevelopmentMode: 'false'
      Email__ConnectionString: '@Microsoft.KeyVault(VaultName=${names.keyVault};SecretName=acs-connection-string)'
      // Sender address format: DoNotReply@<azure-managed-domain>.azurecomm.net
      Email__SenderAddress: 'DoNotReply@${emailDomainRef.properties.mailFromSenderDomain}'
      Email__SenderName: 'Luminous'
      // Use custom domain for email links if configured
      Email__BaseUrl: hasCustomDomain ? 'https://${customDomain}' : 'https://${staticWebApp.outputs.defaultHostname}'
      Email__HelpUrl: hasCustomDomain ? 'https://${customDomain}/help' : 'https://${staticWebApp.outputs.defaultHostname}/help'
      // JWT settings for authentication
      Jwt__SecretKey: '@Microsoft.KeyVault(VaultName=${names.keyVault};SecretName=jwt-secret-key)'
      Jwt__Issuer: 'https://luminous.auth'
      Jwt__Audience: 'luminous-api'
      Jwt__ExpirationMinutes: '60'
      Jwt__RefreshExpirationDays: '7'
      // FIDO2/WebAuthn settings for passkey authentication
      // Use custom domain if configured, otherwise use Static Web App default hostname
      Fido2__ServerDomain: hasCustomDomain ? customDomain : staticWebApp.outputs.defaultHostname
      Fido2__ServerName: 'Luminous Family Hub'
      Fido2__Origins__0: hasCustomDomain ? 'https://${customDomain}' : 'https://${staticWebApp.outputs.defaultHostname}'
      Fido2__Origins__1: hasCustomDomain ? 'https://www.${customDomain}' : ''
      // Also allow the SWA default hostname for passkeys registered before custom domain setup
      Fido2__Origins__2: hasCustomDomain ? 'https://${staticWebApp.outputs.defaultHostname}' : ''
      // Calendar OAuth settings - credentials stored securely in Key Vault
      // These must be populated after deployment with values from Google Cloud Console and Azure AD
      Calendar__Google__ClientId: '@Microsoft.KeyVault(VaultName=${names.keyVault};SecretName=calendar-google-client-id)'
      Calendar__Google__ClientSecret: '@Microsoft.KeyVault(VaultName=${names.keyVault};SecretName=calendar-google-client-secret)'
      Calendar__Microsoft__ClientId: '@Microsoft.KeyVault(VaultName=${names.keyVault};SecretName=calendar-microsoft-client-id)'
      Calendar__Microsoft__ClientSecret: '@Microsoft.KeyVault(VaultName=${names.keyVault};SecretName=calendar-microsoft-client-secret)'
      Calendar__Microsoft__TenantId: 'common'
      // Use custom domain for OAuth redirect if configured
      Calendar__DefaultRedirectUri: hasCustomDomain ? 'https://${customDomain}/auth/calendar/callback' : 'https://${staticWebApp.outputs.defaultHostname}/auth/calendar/callback'
    }
  }
  dependsOn: [
    appService
    keyVault
    redis // Ensures Redis secret is exported to Key Vault
    acsConnectionStringSecret
    jwtSecretKeySecret
    emailService // Ensures email domain exists for sender address
    calendarGoogleClientIdSecret
    calendarGoogleClientSecretSecret
    calendarMicrosoftClientIdSecret
    calendarMicrosoftClientSecretSecret
  ]
}

module functionAppSync 'br/public:avm/res/web/site:0.19.4' = {
  name: 'deploy-function-app-sync'
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
      linuxFxVersion: 'DOTNET-ISOLATED|10.0'
      use32BitWorkerProcess: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
    // Send diagnostic logs to Log Analytics (linked to Application Insights)
    diagnosticSettings: [
      {
        workspaceResourceId: logAnalytics.outputs.resourceId
        logCategoriesAndGroups: [
          { categoryGroup: 'allLogs' }
        ]
        metricCategories: [
          { category: 'AllMetrics' }
        ]
      }
    ]
    configs: [
      {
        name: 'appsettings'
        properties: {
          FUNCTIONS_EXTENSION_VERSION: '~4'
          FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
          AzureWebJobsStorage: storageAccount.outputs.primaryBlobEndpoint
          APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
          CosmosDb__AccountEndpoint: cosmosDbEndpoint
          CosmosDb__DatabaseName: projectName
          CosmosDb__UseManagedIdentity: 'true'
        }
      }
    ]
  }
}

module functionAppImport 'br/public:avm/res/web/site:0.19.4' = {
  name: 'deploy-function-app-import'
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
      linuxFxVersion: 'DOTNET-ISOLATED|10.0'
      use32BitWorkerProcess: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
    // Send diagnostic logs to Log Analytics (linked to Application Insights)
    diagnosticSettings: [
      {
        workspaceResourceId: logAnalytics.outputs.resourceId
        logCategoriesAndGroups: [
          { categoryGroup: 'allLogs' }
        ]
        metricCategories: [
          { category: 'AllMetrics' }
        ]
      }
    ]
    configs: [
      {
        name: 'appsettings'
        properties: {
          FUNCTIONS_EXTENSION_VERSION: '~4'
          FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
          AzureWebJobsStorage: storageAccount.outputs.primaryBlobEndpoint
          APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
          CosmosDb__AccountEndpoint: cosmosDbEndpoint
          CosmosDb__DatabaseName: projectName
          CosmosDb__UseManagedIdentity: 'true'
        }
      }
    ]
  }
}

// Link the App Service API as a backend for the Static Web App
// This enables the SWA to proxy /api/* requests to the App Service
resource staticWebAppBackend 'Microsoft.Web/staticSites/linkedBackends@2023-12-01' = {
  name: '${names.staticWebApp}/backend'
  properties: {
    backendResourceId: appService.outputs.resourceId
    region: location
  }
  // Note: Implicit dependency on staticWebApp via the name property
}

// =============================================================================
// Outputs
// =============================================================================

output resourceGroupName string = resourceGroup().name

// Data Services
output cosmosDbEndpoint string = cosmosDbEndpoint
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

// Communication Services
output communicationServicesName string = communicationServices.outputs.name
output emailServiceName string = emailService.outputs.name
output emailDomainResourceId string = emailService.outputs.domainResourceIds[0]

// Custom Domain & DNS (only output if configured)
output customDomainConfigured bool = hasCustomDomain
output customDomainName string = hasCustomDomain ? customDomain : ''
output dnsZoneNameServers array = dnsZone.?outputs.nameServers ?? []
output webAppUrl string = hasCustomDomain ? 'https://${customDomain}' : 'https://${staticWebApp.outputs.defaultHostname}'
