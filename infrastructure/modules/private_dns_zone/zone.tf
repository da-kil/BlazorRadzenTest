resource "azurerm_private_dns_zone" "this" {
  name                = var.name
  resource_group_name = var.resource_group_name

  tags = local.merged_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "this" {
  for_each = var.linked_vnet_ids

  name                  = substr("pdnsvnetlink-${var.name}-${each.key}", 0, 80)
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = var.name
  virtual_network_id    = each.value
}
