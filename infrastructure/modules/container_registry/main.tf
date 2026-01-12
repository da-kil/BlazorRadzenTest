locals {
  artifactName       = "container-registry"
  artifactVersion    = "1.0.0"
  artifactIdentifier = "${local.artifactName}-${local.artifactVersion}"
  merged_tags = merge(var.tags, {
    environment        = var.env
    component          = var.component
    BLUEPRINT_ARTIFACT = local.artifactIdentifier
  })

  # Resource name restrictions: https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules#microsoftcontainerregistry
  resourceName          = "acr${substr(var.component, 0, 30)}${var.resource_name_infix}${var.env}${local.location_short}"
  private_endpoint_name = "pe-${substr(var.component, 0, 31)}-${var.resource_name_infix}-${var.env}-${local.location_short}-${var.deployment_index}"

  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    northeurope      = "eun"
    westeurope       = "euw"
    swedencentral    = "swc"
  }[var.location]
}
