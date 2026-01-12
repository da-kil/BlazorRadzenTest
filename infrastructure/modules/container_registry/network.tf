data "azurerm_private_dns_zone" "azurecr" {
  provider            = azurerm.connectivity
  name                = "privatelink.azurecr.io"
  resource_group_name = var.central_dns_zone_resource_group_name
}

resource "azurerm_private_endpoint" "this" {
  name                          = "${local.private_endpoint_name}-acr-pe"
  location                      = var.private_endpoint_location
  resource_group_name           = azurerm_container_registry.this.resource_group_name
  subnet_id                     = var.private_endpoint_subnet_id
  custom_network_interface_name = "${azurerm_container_registry.this.name}-nic-pe"

  private_service_connection {
    name                           = "pe"
    is_manual_connection           = false
    private_connection_resource_id = azurerm_container_registry.this.id
    subresource_names              = ["registry"]
  }

  private_dns_zone_group {
    name                 = "pe-group-registry-${azurerm_container_registry.this.name}"
    private_dns_zone_ids = [data.azurerm_private_dns_zone.azurecr.id]
  }

  dynamic "ip_configuration" {
    for_each = var.private_endpoint_ip_addresses != null ? var.private_endpoint_ip_addresses : {}
    content {
      name               = ip_configuration.key
      private_ip_address = ip_configuration.value
      subresource_name   = "registry"
      member_name        = ip_configuration.key
    }
  }

  tags = local.merged_tags
}
