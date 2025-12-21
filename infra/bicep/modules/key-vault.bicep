// =============================================================================
// Key Vault Module
// =============================================================================
// Deploys an Azure Key Vault for secure storage of secrets, keys, and
// certificates used by Luminous services.
//
// TOGAF Principle: NFR-SEC-004 - Encrypted data at rest
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/key-vault/vault
// =============================================================================

@description('Name of the Key Vault')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('Enable soft delete for the Key Vault')
param enableSoftDelete bool = true

@description('Soft delete retention in days')
@minValue(7)
@maxValue(90)
param softDeleteRetentionInDays int = 90

@description('Enable purge protection (cannot be disabled once enabled)')
param enablePurgeProtection bool = false

@description('Enable RBAC authorization (recommended over access policies)')
param enableRbacAuthorization bool = true

@description('SKU for the Key Vault')
@allowed(['standard', 'premium'])
param sku string = 'standard'

@description('Enable public network access')
param publicNetworkAccess string = 'Enabled'

// =============================================================================
// Resource
// =============================================================================

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: sku
    }
    tenantId: subscription().tenantId
    enabledForDeployment: true
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: softDeleteRetentionInDays
    enablePurgeProtection: enablePurgeProtection ? true : null
    enableRbacAuthorization: enableRbacAuthorization
    publicNetworkAccess: publicNetworkAccess
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the Key Vault')
output resourceId string = keyVault.id

@description('The name of the Key Vault')
output name string = keyVault.name

@description('The URI of the Key Vault')
output uri string = keyVault.properties.vaultUri
