locals {
  artifactName       = "kubernetes-service"
  artifactVersion    = "1.0.0"
  artifactIdentifier = "${local.artifactName}-${local.artifactVersion}"
  merged_tags = merge(var.tags, {
    env                = var.env
    component          = var.component
    BLUEPRINT_ARTIFACT = local.artifactIdentifier
  })

  # Resource name restrictions: https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules#microsoftnetworkcloud
  resource_name = "aks-${substr(var.component, 0, 14)}-${var.env}-${local.location_short}-${var.deployment_index}"

  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    northeurope      = "eun"
    westeurope       = "euw"
    swedencentral    = "swc"
  }[var.location]

  customCaCerts = var.prox_url != null ? base64encode(data.http.proxy_cert[0].response_body) : null

  containerRegistryName = regex(".*/registries/([^/]+)$", var.bootstrap_container_registry_id)[0]
  containerRegistryHost = ["${local.containerRegistryName}.azurecr.io", "${local.containerRegistryName}.switzerlandnorth.azurecr.io", "${local.containerRegistryName}.privatelink.azurecr.io", "${local.containerRegistryName}.switzerlandnorth.data.azurecr.io", ".identity.azure.net"]
  combinedNoProxy       = distinct(concat(try(var.http_proxy_config.noProxy, []), local.containerRegistryHost))
}

data "http" "proxy_cert" {
  count = var.prox_url != null ? 1 : 0
  url   = var.prox_url
}

data "azurerm_client_config" "current" {}
