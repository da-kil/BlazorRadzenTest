# PostgreSQL Flexible Server Module
resource "random_password" "postgresql_admin" {
  length  = 32
  special = true
}

resource "azurerm_postgresql_flexible_server" "main" {
  name                   = "psql-${var.project_name}-${var.environment}"
  resource_group_name    = var.resource_group_name
  location               = var.location
  version                = var.postgresql_version
  delegated_subnet_id    = var.postgresql_subnet_id
  private_dns_zone_id    = var.private_dns_zone_id
  administrator_login    = var.administrator_login
  administrator_password = random_password.postgresql_admin.result

  storage_mb   = var.storage_mb
  storage_tier = var.storage_tier

  sku_name = var.sku_name
  zone     = var.zone

  backup_retention_days        = var.backup_retention_days
  geo_redundant_backup_enabled = var.geo_redundant_backup_enabled

  high_availability {
    mode                      = var.high_availability_mode
    standby_availability_zone = var.standby_availability_zone
  }

  maintenance_window {
    day_of_week  = var.maintenance_window_day
    start_hour   = var.maintenance_window_hour
    start_minute = var.maintenance_window_minute
  }

  tags = var.tags

  depends_on = [var.private_dns_zone_id]
}

# PostgreSQL Configuration
resource "azurerm_postgresql_flexible_server_configuration" "max_connections" {
  name      = "max_connections"
  server_id = azurerm_postgresql_flexible_server.main.id
  value     = var.max_connections
}

resource "azurerm_postgresql_flexible_server_configuration" "shared_buffers" {
  name      = "shared_buffers"
  server_id = azurerm_postgresql_flexible_server.main.id
  value     = var.shared_buffers
}

resource "azurerm_postgresql_flexible_server_configuration" "work_mem" {
  name      = "work_mem"
  server_id = azurerm_postgresql_flexible_server.main.id
  value     = var.work_mem
}

# Event Store Database
resource "azurerm_postgresql_flexible_server_database" "eventstore" {
  name      = "eventstore"
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "UTF8"
}

# Read Models Database
resource "azurerm_postgresql_flexible_server_database" "readmodels" {
  name      = "readmodels"
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "UTF8"
}

# Diagnostic settings
resource "azurerm_monitor_diagnostic_setting" "postgresql" {
  name                       = "postgresql-diagnostics"
  target_resource_id         = azurerm_postgresql_flexible_server.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "PostgreSQLLogs"
  }

  metric {
    category = "AllMetrics"
    enabled  = true
  }
}

# Store connection strings in Key Vault
resource "azurerm_key_vault_secret" "eventstore_connection_string" {
  name         = "PostgreSQL-EventStore-ConnectionString"
  value        = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Database=eventstore;Username=${var.administrator_login};Password=${random_password.postgresql_admin.result};SSL Mode=Require;Trust Server Certificate=true"
  key_vault_id = var.key_vault_id

  tags = var.tags
}

resource "azurerm_key_vault_secret" "readmodels_connection_string" {
  name         = "PostgreSQL-ReadModels-ConnectionString"
  value        = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Database=readmodels;Username=${var.administrator_login};Password=${random_password.postgresql_admin.result};SSL Mode=Require;Trust Server Certificate=true"
  key_vault_id = var.key_vault_id

  tags = var.tags
}

resource "azurerm_key_vault_secret" "admin_password" {
  name         = "PostgreSQL-Admin-Password"
  value        = random_password.postgresql_admin.result
  key_vault_id = var.key_vault_id

  tags = var.tags
}
