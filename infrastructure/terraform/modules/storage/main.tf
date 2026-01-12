# Local variables
locals {
  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    northeurope      = "eun"
    westeurope       = "euw"
    swedencentral    = "swc"
  }
  loc = lookup(local.location_short, var.location, "swn")

  common_tags = merge(var.tags, {
    Environment = var.env
    ManagedBy   = "Terraform"
  })
}

# Random string for unique storage account naming
resource "random_string" "storage_suffix" {
  length  = 6
  special = false
  upper   = false
}

# Storage Account
resource "azurerm_storage_account" "main" {
  name                     = "sabb${var.env}${local.loc}${random_string.storage_suffix.result}"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = var.account_tier
  account_replication_type = var.account_replication_type
  account_kind             = "StorageV2"
  access_tier              = "Hot"

  # Enable HTTPS only
  enable_https_traffic_only = true

  # Minimum TLS version
  min_tls_version = "TLS1_2"

  # Disable public network access
  public_network_access_enabled = false

  # Disable shared key access (use Azure AD only)
  shared_access_key_enabled = false

  # Blob properties
  blob_properties {
    versioning_enabled  = true
    change_feed_enabled = true

    delete_retention_policy {
      days = 30
    }

    container_delete_retention_policy {
      days = 30
    }
  }

  tags = local.common_tags
}

# Storage Containers
resource "azurerm_storage_container" "containers" {
  for_each              = toset(var.containers)
  name                  = each.value
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

# Private Endpoint for Storage Blob
resource "azurerm_private_endpoint" "storage_blob" {
  name                = "pe-${azurerm_storage_account.main.name}-blob"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "psc-${azurerm_storage_account.main.name}-blob"
    private_connection_resource_id = azurerm_storage_account.main.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [var.private_dns_zone_id]
  }

  tags = local.common_tags
}

# Diagnostic Settings for Storage Account
resource "azurerm_monitor_diagnostic_setting" "storage" {
  count                      = var.diagnostics_workspace_id != "" ? 1 : 0
  name                       = "diag-${azurerm_storage_account.main.name}"
  target_resource_id         = azurerm_storage_account.main.id
  log_analytics_workspace_id = var.diagnostics_workspace_id

  metric {
    category = "Transaction"
    enabled  = true
  }
}

# Diagnostic Settings for Storage Blob Service
resource "azurerm_monitor_diagnostic_setting" "storage_blob" {
  count                      = var.diagnostics_workspace_id != "" ? 1 : 0
  name                       = "diag-${azurerm_storage_account.main.name}-blob"
  target_resource_id         = "${azurerm_storage_account.main.id}/blobServices/default"
  log_analytics_workspace_id = var.diagnostics_workspace_id

  enabled_log {
    category = "StorageRead"
  }

  enabled_log {
    category = "StorageWrite"
  }

  enabled_log {
    category = "StorageDelete"
  }

  metric {
    category = "Transaction"
    enabled  = true
  }
}
