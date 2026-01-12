resource "azurerm_resource_group" "this" {
  name     = local.resource_name
  location = var.location

  tags = local.merged_tags
}
