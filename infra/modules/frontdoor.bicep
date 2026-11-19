param name string
param tags object
param webOriginHost string
param apiOriginHost string
@allowed(['Detection', 'Prevention'])
param wafMode string

resource profile 'Microsoft.Cdn/profiles@2024-02-01' = {
  name: name
  location: 'global'
  tags: tags
  sku: { name: 'Premium_AzureFrontDoor' }
}

resource waf 'Microsoft.Network/FrontDoorWebApplicationFirewallPolicies@2024-02-01' = {
  name: replace('waf${name}', '-', '')
  location: 'global'
  tags: tags
  sku: { name: 'Premium_AzureFrontDoor' }
  properties: {
    policySettings: {
      enabledState: 'Enabled'
      mode: wafMode
    }
    managedRules: {
      managedRuleSets: [
        { ruleSetType: 'Microsoft_DefaultRuleSet', ruleSetVersion: '2.1', ruleSetAction: 'Block' }
        { ruleSetType: 'Microsoft_BotManagerRuleSet', ruleSetVersion: '1.1' }
      ]
    }
  }
}

var sites = [
  { key: 'web', host: webOriginHost, probePath: '/api/health' }
  { key: 'api', host: apiOriginHost, probePath: '/health/live' }
]

resource endpoints 'Microsoft.Cdn/profiles/afdEndpoints@2024-02-01' = [for s in sites: {
  parent: profile
  name: '${name}-${s.key}'
  location: 'global'
  properties: { enabledState: 'Enabled' }
}]

resource originGroups 'Microsoft.Cdn/profiles/originGroups@2024-02-01' = [for s in sites: {
  parent: profile
  name: 'og-${s.key}'
  properties: {
    loadBalancingSettings: { sampleSize: 4, successfulSamplesRequired: 3 }
    healthProbeSettings: {
      probePath: s.probePath
      probeProtocol: 'Https'
      probeRequestType: 'GET'
      probeIntervalInSeconds: 60
    }
  }
}]

resource origins 'Microsoft.Cdn/profiles/originGroups/origins@2024-02-01' = [for (s, i) in sites: {
  parent: originGroups[i]
  name: 'origin-${s.key}'
  properties: {
    hostName: s.host
    originHostHeader: s.host
    httpsPort: 443
    priority: 1
    weight: 1000
    // TODO(hardening): use Private Link to the Container Apps environment so apps are not reachable directly.
  }
}]

resource routes 'Microsoft.Cdn/profiles/afdEndpoints/routes@2024-02-01' = [for (s, i) in sites: {
  parent: endpoints[i]
  name: 'route-${s.key}'
  properties: {
    originGroup: { id: originGroups[i].id }
    supportedProtocols: ['Https']
    httpsRedirect: 'Enabled'
    forwardingProtocol: 'HttpsOnly'
    patternsToMatch: ['/*']
    linkToDefaultDomain: 'Enabled'
  }
  dependsOn: [origins[i]]
}]

resource security 'Microsoft.Cdn/profiles/securityPolicies@2024-02-01' = {
  parent: profile
  name: 'waf'
  properties: {
    parameters: {
      type: 'WebApplicationFirewall'
      wafPolicy: { id: waf.id }
      associations: [
        {
          domains: [for (s, i) in sites: { id: endpoints[i].id }]
          patternsToMatch: ['/*']
        }
      ]
    }
  }
}

output webEndpointHost string = endpoints[0].properties.hostName
output apiEndpointHost string = endpoints[1].properties.hostName
