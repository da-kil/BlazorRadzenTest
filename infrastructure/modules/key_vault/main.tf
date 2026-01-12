data "azurerm_client_config" "current" {}

locals {
  artifactName       = "key-vault"
  artifactVersion    = "1.0.0"
  artifactIdentifier = "${local.artifactName}-${local.artifactVersion}"
  merged_tags = merge(var.tags, {
    env                = var.env
    component          = var.component
    BLUEPRINT_ARTIFACT = local.artifactIdentifier
  })

  # Resource name restrictions: https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules#microsoftkeyvault
  resource_name = "kv${substr(var.component, 0, 8)}-${var.resource_name_infix}-${var.env}${local.location_short}-${var.deployment_index}"

  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    northeurope      = "eun"
    westeurope       = "euw"
    swedencentral    = "swc"
  }[var.location]
}
