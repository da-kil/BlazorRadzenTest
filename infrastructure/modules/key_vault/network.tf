data "azurerm_private_dns_zone" "this" {
  provider            = azurerm.connectivity
  name                = "privatelink.vaultcore.azure.net"
  resource_group_name = var.central_dns_zone_resource_group_name
}

resource "azurerm_private_endpoint" "this" {
  count               = null != var.private_endpoint_subnet_id ? 1 : 0
  name                = "pe-key-vault-${azurerm_key_vault.this.name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "psc-key-vault-${azurerm_key_vault.this.name}"
    is_manual_connection           = false
    subresource_names              = ["vault"]
    private_connection_resource_id = azurerm_key_vault.this.id
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.this.id]
  }

  tags = local.merged_tags
}
