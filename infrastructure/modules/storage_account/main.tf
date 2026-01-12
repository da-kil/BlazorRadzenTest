locals {
  artifactName       = "storage-account"
  artifactVersion    = "2.0.0"
  artifactIdentifier = "${local.artifactName}-${local.artifactVersion}"
  merged_tags = merge(var.tags, {
    env                = var.env
    component          = var.component
    BLUEPRINT_ARTIFACT = local.artifactIdentifier
  })

  # Resource name restrictions: https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules#microsoftstorage
  resource_name = "sa${substr(var.component, 0, 13)}${var.resource_name_infix}${var.env}${local.location_short}${var.deployment_index}"

  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    northeurope      = "eun"
    westeurope       = "euw"
    swedencentral    = "swc"
  }[var.location]
}

data "azurerm_client_config" "this" {}
