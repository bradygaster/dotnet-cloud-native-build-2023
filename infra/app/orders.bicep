param name string
param location string = resourceGroup().location
param tags object = {}
param containerAppsEnvironmentName string
param containerRegistryName string
param applicationInsightsConnectionString string
param exists bool
param serviceName string = 'orders'
param identityName string = '${serviceName}Identity'

@description('An array of service binds')
param serviceBinds array = []

module app '../core/host/container-app-upsert.bicep' = {
  name: '${serviceName}-container-app'
  dependsOn: [
    identity
  ]
  params: {
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': serviceName })
    identityType: 'UserAssigned'
    identityName: identityName
    exists: exists
    containerAppsEnvironmentName: containerAppsEnvironmentName
    containerRegistryName: containerRegistryName
    serviceBinds: serviceBinds
    ingressEnabled: true
    external: false
    targetPort: 8080
    env: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'Development'
      }
      {
        name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
        value: applicationInsightsConnectionString
      }
    ]
  }
}

module identity '../core/security/user-assigned-identity.bicep' = {
  scope: resourceGroup()
  name: '${serviceName}Identity'
  params: {
    identityName: identityName
    location: location
  }
}

output SERVICE_ORDERS_NAME string = app.outputs.name
output SERVICE_ORDERS_URI string = app.outputs.uri
output SERVICE_ORDERS_IMAGE_NAME string = app.outputs.imageName

