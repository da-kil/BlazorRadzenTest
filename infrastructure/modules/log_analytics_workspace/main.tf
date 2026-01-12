locals {
  artifactName       = "log-analytics-workspace"
  artifactVersion    = "1.0.0"
  artifactIdentifier = "${local.artifactName}-${local.artifactVersion}"
  merged_tags = merge(var.tags, {
    env                = var.env
    component          = var.component
    BLUEPRINT_ARTIFACT = local.artifactIdentifier
  })

  # Resource name restrictions: https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/log_analytics_workspace
  resource_name = "ala-${substr(var.component, 0, 43)}-${var.resource_name_infix}-${var.env}-${local.location_short}-${var.deployment_index}"

  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    northeurope      = "eun"
    westeurope       = "euw"
    swedencentral    = "swc"
  }[var.location]
}
