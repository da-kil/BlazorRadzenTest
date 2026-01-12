locals {
  artifactName       = "resource-group"
  artifactVersion    = "1.0.0"
  artifactIdentifier = "${local.artifactName}-${local.artifactVersion}" # Will be used in the Resource Name
  merged_tags = merge(var.tags, {
    ENV             = var.env
    MODULE_ARTIFACT = local.artifactIdentifier # Can be used later, to identify what resources where deployed in which module version
  })

  # Resource name restrictions: https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules#microsoftresources
  resource_name = "rg-${substr(var.component, 0, 75)}-${var.env}-${local.location_short}-${var.deployment_index}"

  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    westeurope       = "weu"
    northeurope      = "neu"
    swedencentral    = "swc"
  }[var.location]
}
