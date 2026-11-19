param name string
param location string
param tags object

resource vnet 'Microsoft.Network/virtualNetworks@2024-05-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    addressSpace: { addressPrefixes: ['10.20.0.0/16'] }
    subnets: [
      {
        // Container Apps environment (workload profiles) - /23 or larger recommended
        name: 'snet-apps'
        properties: {
          addressPrefix: '10.20.0.0/23'
          delegations: [
            { name: 'aca', properties: { serviceName: 'Microsoft.App/environments' } }
          ]
        }
      }
      {
        // PostgreSQL Flexible Server - VNet-integrated, no public access
        name: 'snet-postgres'
        properties: {
          addressPrefix: '10.20.2.0/24'
          delegations: [
            { name: 'pg', properties: { serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers' } }
          ]
        }
      }
      {
        // Private endpoints (Key Vault, Storage) - add as hardening step
        name: 'snet-private-endpoints'
        properties: {
          addressPrefix: '10.20.3.0/24'
        }
      }
    ]
  }
}

output vnetId string = vnet.id
output appsSubnetId string = vnet.properties.subnets[0].id
output postgresSubnetId string = vnet.properties.subnets[1].id
output privateEndpointSubnetId string = vnet.properties.subnets[2].id
