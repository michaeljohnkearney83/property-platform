param name string
param location string
param tags object
@description('Principal (the app managed identity) allowed to read secrets.')
param readerPrincipalId string
@description('Name of the database connection-string secret.')
param connectionStringSecretName string
@secure()
param connectionStringValue string
param purgeProtection bool

resource vault 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: { family: 'A', name: 'standard' }
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: purgeProtection ? true : null
    // TODO(hardening): set publicNetworkAccess to 'Disabled' and add a private endpoint.
    publicNetworkAccess: 'Enabled'
  }
}

resource connectionString 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  parent: vault
  name: connectionStringSecretName
  properties: { value: connectionStringValue }
}

// Key Vault Secrets User
var secretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource readerRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: vault
  name: guid(vault.id, readerPrincipalId, secretsUserRoleId)
  properties: {
    principalId: readerPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', secretsUserRoleId)
  }
}

output name string = vault.name
output connectionStringSecretUri string = '${vault.properties.vaultUri}secrets/${connectionStringSecretName}'
