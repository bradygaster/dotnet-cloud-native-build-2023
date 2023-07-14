targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Specifies if the publisher app exists')
param storeAppExists bool = false

@description('Specifies if the orders app exists')
param ordersAppExists bool = false

@description('Specifies if the products app exists')
param productsAppExists bool = false

@description('Specifies if the proxy app exists')
param proxyAppExists bool = false

@description('Specifies if the orderprocessor app exists')
param orderprocessorAppExists bool = false

var tags = { 'azd-env-name': environmentName, 'azd-template-name': 'Contoso Online 2023' }
var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: tags
}

module containerApps 'core/host/container-apps.bicep' = {
  name: 'container-apps'
  scope: resourceGroup
  params: {
    name: 'app'
    containerAppsEnvironmentName: '${abbrs.appManagedEnvironments}${resourceToken}'
    containerRegistryName: '${abbrs.containerRegistryRegistries}${resourceToken}'
    location: location
    logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsWorkspaceName
  }
}

module monitoring 'core/monitor/monitoring.bicep' = {
  name: 'monitoring'
  scope: resourceGroup
  params: {
    location: location
    tags: tags
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
    applicationInsightsDashboardName: '${abbrs.portalDashboards}${resourceToken}'
  }
}

module postgres 'core/host/container-app.bicep' = {
  name: 'postgres'
  scope: resourceGroup
  params: {
    name: 'postgres'
    location: location
    tags: tags
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    serviceType: 'postgres'
  }
}

module store 'app/store.bicep' = {
  name: 'store'
  scope: resourceGroup
  params: {
    name: 'store'
    location: location
    tags: tags
    exists: storeAppExists
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    applicationInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
  }
}

module proxy 'app/proxy.bicep' = {
  name: 'proxy'
  scope: resourceGroup
  params: {
    name: 'proxy'
    location: location
    tags: tags
    exists: proxyAppExists
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    applicationInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
  }
}

module orderprocessor 'app/orderprocessor.bicep' = {
  name: 'orderprocessor'
  scope: resourceGroup
  params: {
    name: 'orderprocessor'
    location: location
    tags: tags
    exists: orderprocessorAppExists
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    applicationInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
  }
}

module products 'app/products.bicep' = {
  name: 'products'
  scope: resourceGroup
  params: {
    name: 'products'
    location: location
    tags: tags
    exists: productsAppExists
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    applicationInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
  }
}

module orders 'app/orders.bicep' = {
  name: 'orders'
  scope: resourceGroup
  params: {
    name: 'orders'
    location: location
    tags: tags
    exists: ordersAppExists
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    applicationInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
    serviceBinds: [
      {
        serviceId: postgres.outputs.id
        name: postgres.name
      }
    ]
  }
}

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerApps.outputs.registryLoginServer
output AZURE_CONTAINER_REGISTRY_NAME string = containerApps.outputs.registryName
output ACA_ENVIRONMENT_NAME string = containerApps.outputs.environmentName
