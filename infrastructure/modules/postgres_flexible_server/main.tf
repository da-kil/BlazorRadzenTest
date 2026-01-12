locals {
  artifactName       = "postgres-flexible-server"
  artifactVersion    = "1.0.0"
  artifactIdentifier = "${local.artifactName}-${local.artifactVersion}"
  merged_tags = merge(var.tags, {
    env                = var.env
    component          = var.component
    BLUEPRINT_ARTIFACT = local.artifactIdentifier
  })

  resource_name_infix = var.resource_name_infix != "" ? var.resource_name_infix : random_string.infix.result
  # Resource name restrictions: https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules#microsoftdbforpostgresql
  resource_name = "pg-${substr(var.component, 0, 44)}-${var.resource_name_infix}-${var.env}-${local.location_short}-${var.deployment_index}"

  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    northeurope      = "eun"
    westeurope       = "euw"
    swedencentral    = "swc"
  }[var.location]
}

resource "random_string" "infix" {
  length  = 3
  special = false
  upper   = false
}
