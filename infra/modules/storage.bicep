@minLength(5)
param name string
param location string
param tags object
param contributorPrincipalId string
param geoRedundant bool

resource account 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: take(name, 24)
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: { name: geoRedundant ? 'Standard_GZRS' : 'Standard_LRS' }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false // managed identity + user-delegation SAS only
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2024-01-01' = {
  parent: account
  name: 'default'
  properties: {
    isVersioningEnabled: true
    deleteRetentionPolicy: { enabled: true, days: 30 }
    containerDeleteRetentionPolicy: { enabled: true, days: 30 }
  }
}

// Tenancy agreements, certificates, inspection photos, invoices
resource documents 'Microsoft.Storage/storageAccounts/blobServices/containers@2024-01-01' = {
  parent: blobService
  name: 'documents'
  properties: { publicAccess: 'None' }
}

// Storage Blob Data Contributor
var blobContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource contributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: account
  name: guid(account.id, contributorPrincipalId, blobContributorRoleId)
  properties: {
    principalId: contributorPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', blobContributorRoleId)
  }
}

output blobEndpoint string = account.properties.primaryEndpoints.blob
