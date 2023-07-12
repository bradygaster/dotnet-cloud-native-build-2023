param name string
param location string = resourceGroup().location
param tags object = {}
param containerAppsEnvironmentName string
param containerRegistryName string
param exists bool
param serviceName string = 'orderprocessor'
param identityName string = '${serviceName}Identity'

@description('Should external HTTP/S access be permitted')
param allowExternalIngress bool = false

@description('The port HTTP/S traffic should be forwarded to')
param targetPort int = 8080

@description('An array of service binds')
param serviceBinds array = []

// identity
module identity '../core/security/user-assigned-identity.bicep' = {
  scope: resourceGroup()
  name: '${serviceName}Identity'
  params: {
    identityName: identityName
    location: location
  }
}

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
    env: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'Development'
      }
    ]
    ingressEnabled: false
    external: allowExternalIngress
    targetPort: targetPort
    serviceBinds: serviceBinds
  }
}

output SERVICE_ORDERPROCESSOR_NAME string = app.outputs.name
output SERVICE_ORDERPROCESSOR_URI string = app.outputs.uri
output SERVICE_ORDERPROCESSOR_IMAGE_NAME string = app.outputs.imageName
