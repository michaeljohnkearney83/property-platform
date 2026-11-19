param prefix string
param environmentName string
param location string
param tags object
param infrastructureSubnetId string
param logAnalyticsWorkspaceName string
param appInsightsConnectionString string
param identityId string
param identityClientId string
param registryServer string
param imageTag string
param dbConnectionSecretUri string
param blobEndpoint string
param authAuthority string
param authAudience string
param zoneRedundant bool
param minReplicas int

var bootstrap = empty(imageTag)
var placeholderImage = 'mcr.microsoft.com/k8se/quickstart:latest'
var apiImage = bootstrap ? placeholderImage : '${registryServer}/propertyplatform-api:${imageTag}'
var webImage = bootstrap ? placeholderImage : '${registryServer}/propertyplatform-web:${imageTag}'
var aspnetEnvironment = environmentName == 'prod' ? 'Production' : (environmentName == 'uat' ? 'UAT' : 'Dev')

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: logAnalyticsWorkspaceName
}

resource env 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-${prefix}-${environmentName}'
  location: location
  tags: tags
  properties: {
    zoneRedundant: zoneRedundant
    vnetConfiguration: {
      infrastructureSubnetId: infrastructureSubnetId
      internal: false
    }
    workloadProfiles: [
      { name: 'Consumption', workloadProfileType: 'Consumption' }
    ]
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logs.properties.customerId
        sharedKey: logs.listKeys().primarySharedKey
      }
    }
  }
}

var identity = {
  type: 'UserAssigned'
  userAssignedIdentities: { '${identityId}': {} }
}

var registries = bootstrap ? [] : [{ server: registryServer, identity: identityId }]

var apiSecrets = [
  { name: 'db-connection-string', keyVaultUrl: dbConnectionSecretUri, identity: identityId }
  { name: 'appinsights-connection-string', value: appInsightsConnectionString }
]

var apiEnv = [
  { name: 'ASPNETCORE_ENVIRONMENT', value: aspnetEnvironment }
  { name: 'ConnectionStrings__Platform', secretRef: 'db-connection-string' }
  { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', secretRef: 'appinsights-connection-string' }
  { name: 'AZURE_CLIENT_ID', value: identityClientId }
  { name: 'Storage__BlobEndpoint', value: blobEndpoint }
  { name: 'Auth__Authority', value: authAuthority }
  { name: 'Auth__Audience', value: authAudience }
  { name: 'Cors__AllowedOrigins__0', value: 'https://ca-${prefix}-${environmentName}-web.${env.properties.defaultDomain}' }
]

resource api 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ca-${prefix}-${environmentName}-api'
  location: location
  tags: tags
  identity: identity
  properties: {
    environmentId: env.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Multiple' // enables blue/green traffic splitting
      ingress: {
        external: true
        targetPort: bootstrap ? 80 : 8080
        transport: 'auto'
        allowInsecure: false
      }
      registries: registries
      secrets: apiSecrets
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          resources: { cpu: json('0.5'), memory: '1Gi' }
          env: apiEnv
          probes: bootstrap ? [] : [
            { type: 'Liveness', httpGet: { path: '/health/live', port: 8080 }, periodSeconds: 30 }
            { type: 'Readiness', httpGet: { path: '/health/ready', port: 8080 }, periodSeconds: 15 }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: 10
        rules: [
          { name: 'http', http: { metadata: { concurrentRequests: '50' } } }
        ]
      }
    }
  }
}

resource web 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'ca-${prefix}-${environmentName}-web'
  location: location
  tags: tags
  identity: identity
  properties: {
    environmentId: env.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: bootstrap ? 80 : 3000
        transport: 'auto'
        allowInsecure: false
      }
      registries: registries
    }
    template: {
      containers: [
        {
          name: 'web'
          image: webImage
          resources: { cpu: json('0.5'), memory: '1Gi' }
          env: [
            { name: 'API_BASE_URL', value: 'https://${api.properties.configuration.ingress.fqdn}' }
          ]
          probes: bootstrap ? [] : [
            { type: 'Liveness', httpGet: { path: '/api/health', port: 3000 }, periodSeconds: 30 }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: 10
      }
    }
  }
}

// Runs EF Core migration bundles inside the VNet (the database has no public access).
// Started by the pipeline: az containerapp job start ... --image <registry>/propertyplatform-migrations:<sha>
resource migrations 'Microsoft.App/jobs@2024-03-01' = {
  name: 'caj-${prefix}-${environmentName}-migrate'
  location: location
  tags: tags
  identity: identity
  properties: {
    environmentId: env.id
    workloadProfileName: 'Consumption'
    configuration: {
      triggerType: 'Manual'
      replicaTimeout: 900
      replicaRetryLimit: 0
      manualTriggerConfig: { parallelism: 1, replicaCompletionCount: 1 }
      registries: registries
      secrets: [apiSecrets[0]]
    }
    template: {
      containers: [
        {
          name: 'migrate'
          image: bootstrap ? placeholderImage : '${registryServer}/propertyplatform-migrations:${imageTag}'
          resources: { cpu: json('0.5'), memory: '1Gi' }
          env: [
            { name: 'ConnectionStrings__Platform', secretRef: 'db-connection-string' }
          ]
        }
      ]
    }
  }
}

output apiAppName string = api.name
output webAppName string = web.name
output migrationJobName string = migrations.name
output apiFqdn string = api.properties.configuration.ingress.fqdn
output webFqdn string = web.properties.configuration.ingress.fqdn
