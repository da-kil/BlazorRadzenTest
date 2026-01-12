data "azurerm_private_dns_zone" "postgres" {
  provider            = azurerm.connectivity
  name                = "privatelink.postgres.database.azure.com"
  resource_group_name = var.central_dns_zone_resource_group_name
}

data "azurerm_client_config" "current" {}

resource "azurerm_postgresql_flexible_server" "this" {
  name                          = local.resource_name
  resource_group_name           = var.resource_group_name
  location                      = var.location
  version                       = var.postgres_version
  delegated_subnet_id           = var.delegated_sub_net_id
  private_dns_zone_id           = data.azurerm_private_dns_zone.postgres.id
  public_network_access_enabled = var.public_network_access_enabled
  zone                          = var.deployment_zone
  create_mode                   = "Default"
  sku_name                      = var.sku

  backup_retention_days        = var.backup_retention_days
  geo_redundant_backup_enabled = var.geo_redundant_backup_enabled
  storage_mb                   = var.storage_in_mb
  storage_tier                 = var.storage_tier
  auto_grow_enabled            = var.auto_grow_enabled

  administrator_login    = var.administrator_login
  administrator_password = var.administrator_password

  authentication {
    active_directory_auth_enabled = true
    password_auth_enabled         = var.password_auth_enabled
    tenant_id                     = data.azurerm_client_config.current.tenant_id
  }

  dynamic "customer_managed_key" {
    for_each = var.storage_encryption_key_vault_id != null ? [var.storage_encryption_key_vault_id] : []
    content {
      key_vault_key_id = var.storage_encryption_key_vault_id
    }
  }

  dynamic "high_availability" {
    for_each = [var.high_availability]
    content {
      mode                      = high_availability.value.mode
      standby_availability_zone = high_availability.value.standbyAvailabilityZone
    }
  }

  identity {
    type         = var.identity_type
    identity_ids = var.identity_ids
  }

  dynamic "maintenance_window" {
    for_each = var.maintenance_window != null ? [var.maintenance_window] : []
    content {
      day_of_week  = maintenance_window.value.dayOfWeek
      start_hour   = maintenance_window.value.startHour
      start_minute = maintenance_window.value.startHour
    }
  }

  tags = local.merged_tags
}
