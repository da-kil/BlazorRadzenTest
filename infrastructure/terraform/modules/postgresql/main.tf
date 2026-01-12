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

# Random password for PostgreSQL admin
resource "random_password" "pg_admin" {
  length  = 32
  special = true
}

# Random string for unique naming
resource "random_string" "pg_suffix" {
  length  = 4
  special = false
  upper   = false
}

# PostgreSQL Flexible Server
resource "azurerm_postgresql_flexible_server" "main" {
  name                = "pg-bbdb-${var.env}-${local.loc}-${random_string.pg_suffix.result}"
  resource_group_name = var.resource_group_name
  location            = var.location
  version             = var.postgres_version
  sku_name            = var.sku_name

  # Admin credentials
  administrator_login    = "psqladmin"
  administrator_password = random_password.pg_admin.result

  # Network configuration - private only
  delegated_subnet_id = var.delegated_subnet_id
  private_dns_zone_id = var.private_dns_zone_id

  # Storage
  storage_mb   = var.storage_mb
  auto_grow_enabled = true

  # Backup
  backup_retention_days        = var.backup_retention_days
  geo_redundant_backup_enabled = var.geo_redundant_backup_enabled

  # High Availability (zone-redundant)
  dynamic "high_availability" {
    for_each = var.high_availability_enabled ? [1] : []
    content {
      mode = "ZoneRedundant"
    }
  }

  # Authentication
  authentication {
    active_directory_auth_enabled = true
    password_auth_enabled         = true
  }

  tags = local.common_tags
}

# PostgreSQL Flexible Server Configuration - TLS
resource "azurerm_postgresql_flexible_server_configuration" "tls_version" {
  name      = "ssl_min_protocol_version"
  server_id = azurerm_postgresql_flexible_server.main.id
  value     = "TLSv1.2"
}

# PostgreSQL Flexible Server Configuration - Connections
resource "azurerm_postgresql_flexible_server_configuration" "max_connections" {
  name      = "max_connections"
  server_id = azurerm_postgresql_flexible_server.main.id
  value     = "200"
}

# Diagnostic Settings for PostgreSQL
resource "azurerm_monitor_diagnostic_setting" "postgresql" {
  count                      = var.diagnostics_workspace_id != "" ? 1 : 0
  name                       = "diag-${azurerm_postgresql_flexible_server.main.name}"
  target_resource_id         = azurerm_postgresql_flexible_server.main.id
  log_analytics_workspace_id = var.diagnostics_workspace_id

  enabled_log {
    category = "PostgreSQLLogs"
  }

  metric {
    category = "AllMetrics"
    enabled  = true
  }
}
