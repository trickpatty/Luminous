// =============================================================================
// Storage Account Module
// =============================================================================
// Deploys an Azure Storage Account for blob storage (avatars, recipe photos,
// import files) and function app storage.
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/storage/storage-account
// =============================================================================

@description('Name of the Storage Account (3-24 chars, lowercase alphanumeric)')
@minLength(3)
@maxLength(24)
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('Storage account SKU')
@allowed(['Standard_LRS', 'Standard_GRS', 'Standard_RAGRS', 'Standard_ZRS', 'Premium_LRS', 'Premium_ZRS'])
param sku string = 'Standard_LRS'

@description('Storage account kind')
@allowed(['StorageV2', 'BlobStorage', 'BlockBlobStorage', 'FileStorage'])
param kind string = 'StorageV2'

@description('Blob container names to create')
param containerNames array = []

@description('Enable hierarchical namespace (Data Lake Gen2)')
param enableHierarchicalNamespace bool = false

@description('Minimum TLS version')
@allowed(['TLS1_0', 'TLS1_1', 'TLS1_2'])
param minimumTlsVersion string = 'TLS1_2'

@description('Allow blob public access')
param allowBlobPublicAccess bool = false

// =============================================================================
// Resource - Storage Account
// =============================================================================

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: kind
  properties: {
    minimumTlsVersion: minimumTlsVersion
    allowBlobPublicAccess: allowBlobPublicAccess
    allowSharedKeyAccess: true
    supportsHttpsTrafficOnly: true
    isHnsEnabled: enableHierarchicalNamespace
    accessTier: 'Hot'
    encryption: {
      services: {
        blob: {
          enabled: true
          keyType: 'Account'
        }
        file: {
          enabled: true
          keyType: 'Account'
        }
        queue: {
          enabled: true
          keyType: 'Account'
        }
        table: {
          enabled: true
          keyType: 'Account'
        }
      }
      keySource: 'Microsoft.Storage'
    }
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// =============================================================================
// Resource - Blob Service
// =============================================================================

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

// =============================================================================
// Resource - Blob Containers
// =============================================================================

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [for containerName in containerNames: {
  parent: blobService
  name: containerName
  properties: {
    publicAccess: 'None'
  }
}]

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the Storage Account')
output resourceId string = storageAccount.id

@description('The name of the Storage Account')
output name string = storageAccount.name

@description('The primary blob endpoint')
output primaryBlobEndpoint string = storageAccount.properties.primaryEndpoints.blob

@description('The connection string for the Storage Account')
#disable-next-line outputs-should-not-contain-secrets
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
