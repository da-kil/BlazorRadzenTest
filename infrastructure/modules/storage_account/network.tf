data "azurerm_private_dns_zone" "blob" {
  provider            = azurerm.connectivity
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = var.central_dns_zone_resource_group_name
}

data "azurerm_private_dns_zone" "file" {
  provider            = azurerm.connectivity
  name                = "privatelink.file.core.windows.net"
  resource_group_name = var.central_dns_zone_resource_group_name
}

data "azurerm_private_dns_zone" "queue" {
  provider            = azurerm.connectivity
  name                = "privatelink.queue.core.windows.net"
  resource_group_name = var.central_dns_zone_resource_group_name
}

data "azurerm_private_dns_zone" "table" {
  provider            = azurerm.connectivity
  name                = "privatelink.table.core.windows.net"
  resource_group_name = var.central_dns_zone_resource_group_name
}

locals {
  private_dns_zone_ids = {
    blob  = data.azurerm_private_dns_zone.blob.id
    table = data.azurerm_private_dns_zone.table.id
    queue = data.azurerm_private_dns_zone.queue.id
    file  = data.azurerm_private_dns_zone.file.id
  }
}

resource "azurerm_private_endpoint" "this" {
  for_each                      = var.storage_types
  name                          = "${azurerm_storage_account.this.name}-${each.key}-pe"
  location                      = azurerm_storage_account.this.location
  resource_group_name           = azurerm_storage_account.this.resource_group_name
  subnet_id                     = var.private_endpoint_subnet_id
  custom_network_interface_name = "${azurerm_storage_account.this.name}-${each.key}-nic-pe"

  private_service_connection {
    name                           = "pe-${each.key}"
    is_manual_connection           = false
    private_connection_resource_id = azurerm_storage_account.this.id
    subresource_names              = [each.key]
  }

  private_dns_zone_group {
    name                 = "pe-group-${each.key}-${azurerm_storage_account.this.name}"
    private_dns_zone_ids = [local.private_dns_zone_ids[each.key]]
  }
}
