locals {
  artifactName       = "private-dns-zone"
  artifactVersion    = "1.0.0"
  artifactIdentifier = "${local.artifactName}-${local.artifactVersion}" # Will be used in the Resource Name
  merged_tags = merge(var.tags, {
    ENV             = var.env
    MODULE_ARTIFACT = local.artifactIdentifier # Can be used later, to identify what resources where deployed in which module version
  })
}
