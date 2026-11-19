// Property Platform - one environment (dev | uat | prod) in one resource group.
// Deploy:  az deployment group create -g rg-propertyplatform-<env> -f infra/main.bicep -p infra/env/<env>.bicepparam
targetScope = 'resourceGroup'

@allowed(['dev', 'uat', 'prod'])
param environmentName string

@description('Primary region. UK South keeps data in the UK.')
param location string = 'uksouth'

@description('Short product prefix used in resource names.')
@maxLength(8)
param prefix string = 'pp'

@description('Container image tag (commit SHA). Empty = deploy a placeholder image (first bootstrap).')
param imageTag string = ''

@description('Entra External ID authority, e.g. https://<tenant>.ciamlogin.com/<tenant-id>/v2.0')
param authAuthority string

@description('API application (client) ID / audience registered in Entra External ID.')
param authAudience string

@description('Postgres administrator login.')
param postgresAdminLogin string = 'ppadmin'

@secure()
@description('Postgres administrator password (pipeline secret variable). Stored in Key Vault only.')
param postgresAdminPassword string

@description('Put Azure Front Door + WAF in front of the apps.')
param deployFrontDoor bool = environmentName != 'dev'

var isProd = environmentName == 'prod'
var suffix = uniqueString(resourceGroup().id)
var tags = {
  product: 'property-platform'
  environment: environmentName
  managedBy: 'bicep'
}

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    name: '${prefix}-${environmentName}'
    location: location
    tags: tags
    retentionDays: isProd ? 90 : 30
  }
}

resource appIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${prefix}-${environmentName}-app'
  location: location
  tags: tags
}

module network 'modules/network.bicep' = {
  name: 'network'
  params: {
    name: 'vnet-${prefix}-${environmentName}'
    location: location
    tags: tags
  }
}

module postgres 'modules/postgres.bicep' = {
  name: 'postgres'
  params: {
    name: 'psql-${prefix}-${environmentName}-${suffix}'
    location: location
    tags: tags
    subnetId: network.outputs.postgresSubnetId
    vnetId: network.outputs.vnetId
    adminLogin: postgresAdminLogin
    adminPassword: postgresAdminPassword
    highAvailability: isProd
    geoRedundantBackup: isProd
    skuName: isProd ? 'Standard_D2ds_v5' : 'Standard_B1ms'
    skuTier: isProd ? 'GeneralPurpose' : 'Burstable'
    backupRetentionDays: isProd ? 35 : 7
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault'
  params: {
    name: 'kv-${prefix}-${environmentName}-${take(suffix, 8)}'
    location: location
    tags: tags
    readerPrincipalId: appIdentity.properties.principalId
    connectionStringSecretName: 'db-connection-string'
    connectionStringValue: 'Host=${postgres.outputs.fqdn};Database=${postgres.outputs.databaseName};Username=${postgresAdminLogin};Password=${postgresAdminPassword};Ssl Mode=Require'
    purgeProtection: isProd
  }
}

module registry 'modules/registry.bicep' = {
  name: 'registry'
  params: {
    name: 'cr${prefix}${environmentName}${suffix}'
    location: location
    tags: tags
    pullPrincipalId: appIdentity.properties.principalId
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    name: 'st${prefix}${environmentName}${take(suffix, 8)}'
    location: location
    tags: tags
    contributorPrincipalId: appIdentity.properties.principalId
    geoRedundant: isProd
  }
}

module apps 'modules/containerapps.bicep' = {
  name: 'containerapps'
  params: {
    prefix: prefix
    environmentName: environmentName
    location: location
    tags: tags
    infrastructureSubnetId: network.outputs.appsSubnetId
    logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsName
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    identityId: appIdentity.id
    identityClientId: appIdentity.properties.clientId
    registryServer: registry.outputs.loginServer
    imageTag: imageTag
    dbConnectionSecretUri: keyVault.outputs.connectionStringSecretUri
    blobEndpoint: storage.outputs.blobEndpoint
    authAuthority: authAuthority
    authAudience: authAudience
    zoneRedundant: isProd
    minReplicas: isProd ? 2 : (environmentName == 'uat' ? 1 : 0)
  }
}

module frontDoor 'modules/frontdoor.bicep' = if (deployFrontDoor) {
  name: 'frontdoor'
  params: {
    name: 'afd-${prefix}-${environmentName}'
    tags: tags
    webOriginHost: apps.outputs.webFqdn
    apiOriginHost: apps.outputs.apiFqdn
    wafMode: isProd ? 'Prevention' : 'Detection'
  }
}

output registryName string = registry.outputs.name
output registryLoginServer string = registry.outputs.loginServer
output apiAppName string = apps.outputs.apiAppName
output webAppName string = apps.outputs.webAppName
output migrationJobName string = apps.outputs.migrationJobName
output apiUrl string = 'https://${apps.outputs.apiFqdn}'
output webUrl string = 'https://${apps.outputs.webFqdn}'
output frontDoorWebHost string = deployFrontDoor ? frontDoor!.outputs.webEndpointHost : ''
