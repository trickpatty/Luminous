// =============================================================================
// Redis Cache Module
// =============================================================================
// Deploys an Azure Cache for Redis for session management, real-time sync
// state, and caching.
//
// Azure Verified Module Reference:
// https://github.com/Azure/bicep-registry-modules/tree/main/avm/res/cache/redis
// =============================================================================

@description('Name of the Redis Cache')
param name string

@description('Azure region for deployment')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('Redis Cache SKU name')
@allowed(['Basic', 'Standard', 'Premium'])
param sku string = 'Basic'

@description('Redis Cache family')
@allowed(['C', 'P'])
param family string = 'C'

@description('Redis Cache capacity (size)')
@minValue(0)
@maxValue(6)
param capacity int = 0

@description('Enable non-SSL port (not recommended for production)')
param enableNonSslPort bool = false

@description('Minimum TLS version')
@allowed(['1.0', '1.1', '1.2'])
param minimumTlsVersion string = '1.2'

@description('Redis version')
@allowed(['4', '6'])
param redisVersion string = '6'

// =============================================================================
// Resource
// =============================================================================

resource redisCache 'Microsoft.Cache/redis@2023-08-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      name: sku
      family: family
      capacity: capacity
    }
    enableNonSslPort: enableNonSslPort
    minimumTlsVersion: minimumTlsVersion
    redisVersion: redisVersion
    publicNetworkAccess: 'Enabled'
    redisConfiguration: {
      'maxmemory-policy': 'volatile-lru'
    }
  }
}

// =============================================================================
// Outputs
// =============================================================================

@description('The resource ID of the Redis Cache')
output resourceId string = redisCache.id

@description('The name of the Redis Cache')
output name string = redisCache.name

@description('The hostname of the Redis Cache')
output hostName string = redisCache.properties.hostName

@description('The SSL port of the Redis Cache')
output sslPort int = redisCache.properties.sslPort

@description('The connection string for the Redis Cache')
#disable-next-line outputs-should-not-contain-secrets
output connectionString string = '${redisCache.properties.hostName}:${redisCache.properties.sslPort},password=${redisCache.listKeys().primaryKey},ssl=True,abortConnect=False'
