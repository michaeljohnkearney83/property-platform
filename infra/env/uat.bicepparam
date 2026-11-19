using '../main.bicep'

param environmentName = 'uat'
param location = 'uksouth'

// Entra External ID - fill in after creating the external tenant and the API app registration.
param authAuthority = readEnvironmentVariable('AUTH_AUTHORITY', 'https://REPLACE.ciamlogin.com/REPLACE-TENANT-ID/v2.0')
param authAudience = readEnvironmentVariable('AUTH_AUDIENCE', 'api://REPLACE-API-CLIENT-ID')

// Supplied by the pipeline (secret variable POSTGRES_ADMIN_PASSWORD). Never commit a value.
param postgresAdminPassword = readEnvironmentVariable('POSTGRES_ADMIN_PASSWORD', '')

// Set by the pipeline to the commit SHA; empty on the very first bootstrap deployment.
param imageTag = readEnvironmentVariable('IMAGE_TAG', '')
