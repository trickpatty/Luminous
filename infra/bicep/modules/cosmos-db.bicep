// =============================================================================
// Cosmos DB Module
// =============================================================================
// Deploys an Azure Cosmos DB account with SQL API for the Luminous family
// hub data store. Supports multi-tenant data isolation via partition keys.
//
// TOGAF Principles:
// - DP-1: Cloud-Native Storage
// - DP-2: Tenant Data Isolation (partition key per family)
// - DP-3: Single Source of Truth
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/document-db/database-account
// =============================================================================

@description('Name of the Cosmos DB account')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('Enable serverless capacity mode')
param enableServerless bool = false

@description('Default consistency level')
@allowed(['Eventual', 'Session', 'BoundedStaleness', 'Strong', 'ConsistentPrefix'])
param consistencyLevel string = 'Session'

@description('Name of the database')
param databaseName string

@description('Container configurations')
param containers array = []

@description('Enable automatic failover')
param enableAutomaticFailover bool = false

@description('Enable multi-region writes')
param enableMultipleWriteLocations bool = false

@description('Max staleness prefix for BoundedStaleness consistency')
param maxStalenessPrefix int = 100000

@description('Max staleness interval in seconds for BoundedStaleness consistency')
param maxIntervalInSeconds int = 300

// =============================================================================
// Resource - Cosmos DB Account
// =============================================================================

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: name
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: enableAutomaticFailover
    enableMultipleWriteLocations: enableMultipleWriteLocations
    consistencyPolicy: {
      defaultConsistencyLevel: consistencyLevel
      maxStalenessPrefix: consistencyLevel == 'BoundedStaleness' ? maxStalenessPrefix : null
      maxIntervalInSeconds: consistencyLevel == 'BoundedStaleness' ? maxIntervalInSeconds : null
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: enableServerless ? [
      {
        name: 'EnableServerless'
      }
    ] : []
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Local'
      }
    }
    publicNetworkAccess: 'Enabled'
    networkAclBypass: 'AzureServices'
    disableLocalAuth: false
  }
}

// =============================================================================
// Resource - SQL Database
// =============================================================================

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
    options: enableServerless ? {} : {
      autoscaleSettings: {
        maxThroughput: 4000
      }
    }
  }
}

// =============================================================================
// Resource - SQL Containers
// =============================================================================

resource sqlContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = [for container in containers: {
  parent: database
  name: container.name
  properties: {
    resource: {
      id: container.name
      partitionKey: {
        paths: [container.partitionKeyPath]
        kind: 'Hash'
        version: 2
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
}]

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the Cosmos DB account')
output resourceId string = cosmosAccount.id

@description('The name of the Cosmos DB account')
output accountName string = cosmosAccount.name

@description('The endpoint of the Cosmos DB account')
output endpoint string = cosmosAccount.properties.documentEndpoint

@description('The name of the database')
output databaseName string = database.name

@description('The primary connection string (sensitive - use Key Vault reference in production)')
#disable-next-line outputs-should-not-contain-secrets
output primaryConnectionString string = cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
