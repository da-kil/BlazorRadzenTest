# Data layer configuration for ti8m BeachBreak development environment
# Creates PostgreSQL Flexible Server and Storage Account

#=============================================================================
# POSTGRESQL FLEXIBLE SERVER MODULE
#=============================================================================

module "postgres_flexible_server" {
  source = "../../modules/postgres_flexible_server"

  env                 = var.environment
  deployment_index    = var.deployment_index
  component           = local.component_name
  location            = var.location
  resource_group_name = azurerm_resource_group.data.name
  resource_name_infix = local.random_suffix

  # Server configuration
  sku            = var.postgres_config.sku_name
  postgres_version = "16"
  storage_in_mb  = var.postgres_config.storage_mb
  storage_tier   = "P4"
  deployment_zone = "1"  # Single zone deployment for cost optimization

  # Backup configuration
  backup_retention_days            = var.postgres_config.backup_retention_days
  geo_redundant_backup_enabled     = var.postgres_config.geo_redundant_backup_enabled
  auto_grow_enabled               = true

  # High availability configuration (disabled for cost optimization)
  high_availability = var.postgres_config.high_availability_enabled ? {
    mode                      = "ZoneRedundant"
    standby_availability_zone = "2"
  } : null

  # Security configuration - Entra ID authentication only
  public_network_access_enabled = false
  password_auth_enabled         = false  # Only Azure AD authentication
  identity_type                = "SystemAssigned"

  # Network configuration - private connectivity via delegated subnet
  delegated_sub_net_id                 = azurerm_subnet.postgres.id
  central_dns_zone_resource_group_name = azurerm_resource_group.shared.name

  # Monitoring
  diagnostics = {
    log_analytics_workspace_id = module.log_analytics_workspace.resource_id
    log_category_groups        = ["allLogs"]
    log_metrics               = ["AllMetrics"]
  }

  tags = local.common_tags

  depends_on = [
    azurerm_private_dns_zone.postgres,
    azurerm_private_dns_zone_virtual_network_link.postgres
  ]
}

#=============================================================================
# POSTGRESQL RBAC ASSIGNMENTS FOR APPLICATIONS
#=============================================================================

# Command API to PostgreSQL - PostgreSQL Flexible Server Admin
resource "azurerm_role_assignment" "command_api_to_postgres" {
  scope                = module.postgres_flexible_server.resource_id
  role_definition_name = "PostgreSQL Flexible Server Admin"
  principal_id         = azurerm_user_assigned_identity.command_api.principal_id
}

# Query API to PostgreSQL - PostgreSQL Flexible Server Admin
resource "azurerm_role_assignment" "query_api_to_postgres" {
  scope                = module.postgres_flexible_server.resource_id
  role_definition_name = "PostgreSQL Flexible Server Admin"
  principal_id         = azurerm_user_assigned_identity.query_api.principal_id
}

#=============================================================================
# POSTGRESQL DATABASE CREATION
#=============================================================================

# Events database (for event sourcing)
resource "azurerm_postgresql_flexible_server_database" "events" {
  name      = "events"
  server_id = module.postgres_flexible_server.resource_id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# Read models database (for CQRS query side)
resource "azurerm_postgresql_flexible_server_database" "readmodels" {
  name      = "readmodels"
  server_id = module.postgres_flexible_server.resource_id
  collation = "en_US.utf8"
  charset   = "utf8"
}

#=============================================================================
# STORAGE ACCOUNT MODULE
#=============================================================================

module "storage_account" {
  source = "../../modules/storage_account"

  env                 = var.environment
  deployment_index    = var.deployment_index
  component           = local.component_name
  location            = var.location
  resource_group_name = azurerm_resource_group.data.name
  resource_name_infix = local.random_suffix

  # Storage account configuration
  account_tier             = var.storage_config.account_tier
  account_replication_type = var.storage_config.account_replication_type
  account_kind            = var.storage_config.account_kind
  access_tier             = "Hot"

  # Security configuration
  https_traffic_only_enabled        = true
  sas_enabled                      = false  # Disable shared access signatures
  allow_nested_items_to_be_public  = false

  # Network configuration - private endpoints only
  network_access_mode = "OnlyPrivateEndpoints"
  network_bypass      = ["AzureServices"]
  network_ips_to_whitelist = var.allowed_ip_ranges
  network_subnet_id_to_whitelist = []

  # Private endpoint configuration
  private_endpoint_subnet_id           = azurerm_subnet.private_endpoints.id
  central_dns_zone_resource_group_name = azurerm_resource_group.shared.name

  # Storage services to enable
  storage_types = ["blob", "file", "queue", "table"]

  # Storage containers for the application
  storage_containers = {
    "application-logs" = {
      container_access_type = "private"
    }
    "file-uploads" = {
      container_access_type = "private"
    }
    "backups" = {
      container_access_type = "private"
    }
    "temp-files" = {
      container_access_type = "private"
    }
  }

  # File shares for persistent storage
  storage_file_shares = {
    "app-data" = {
      quota = 100  # 100 GB
    }
    "logs" = {
      quota = 50   # 50 GB
    }
  }

  # Queues for application messaging
  storage_queues = {
    "command-processing" = {}
    "event-notifications" = {}
    "background-tasks" = {}
  }

  # Tables for NoSQL data
  storage_tables = {
    "audit-logs" = {}
    "sessions" = {}
    "cache" = {}
  }

  # Customer-managed key encryption (optional - using Key Vault key)
  storage_encryption_customer_managed_key = {
    key_vault_key_id                    = azurerm_key_vault_key.storage_encryption.id
    user_assigned_identity_id           = azurerm_user_assigned_identity.storage.id
    infrastructure_encryption_enabled   = true
  }

  # Monitoring
  diagnostics = {
    log_analytics_workspace_id = module.log_analytics_workspace.resource_id
    log_category_groups        = ["allLogs"]
    log_metrics               = ["AllMetrics"]
  }

  tags = local.common_tags

  depends_on = [
    azurerm_private_dns_zone.storage_blob,
    azurerm_private_dns_zone.storage_file,
    azurerm_private_dns_zone.storage_queue,
    azurerm_private_dns_zone.storage_table,
    azurerm_private_dns_zone_virtual_network_link.storage_blob,
    azurerm_private_dns_zone_virtual_network_link.storage_file,
    azurerm_private_dns_zone_virtual_network_link.storage_queue,
    azurerm_private_dns_zone_virtual_network_link.storage_table
  ]
}

#=============================================================================
# STORAGE ENCRYPTION RESOURCES
#=============================================================================

# Managed identity for storage account encryption
resource "azurerm_user_assigned_identity" "storage" {
  name                = "umi-storage-${var.environment}-${local.location_short}-${var.deployment_index}"
  location            = var.location
  resource_group_name = azurerm_resource_group.data.name

  tags = local.common_tags
}

# Storage encryption key in Key Vault
resource "azurerm_key_vault_key" "storage_encryption" {
  name         = "storage-encryption-${var.environment}"
  key_vault_id = module.key_vault.resource_id
  key_type     = "RSA"
  key_size     = 2048
  key_opts     = ["decrypt", "encrypt", "wrapKey", "unwrapKey"]

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.current_user_keyvault_admin
  ]
}

# Storage identity to Key Vault - Key Vault Crypto Service Encryption User
resource "azurerm_role_assignment" "storage_to_keyvault_crypto" {
  scope                = module.key_vault.resource_id
  role_definition_name = "Key Vault Crypto Service Encryption User"
  principal_id         = azurerm_user_assigned_identity.storage.principal_id
}

#=============================================================================
# POSTGRESQL CONNECTION STRING UPDATES
#=============================================================================

# Update PostgreSQL connection strings in Key Vault
resource "azurerm_key_vault_secret" "postgres_command_connection_update" {
  name         = "ConnectionStrings--CommandConnection"
  value        = "Host=${module.postgres_flexible_server.resource_name}.postgres.database.azure.com;Database=events;Username=${azurerm_user_assigned_identity.command_api.client_id};SslMode=Require"
  key_vault_id = module.key_vault.resource_id

  tags = local.common_tags

  depends_on = [
    module.postgres_flexible_server,
    azurerm_role_assignment.current_user_keyvault_admin
  ]
}

resource "azurerm_key_vault_secret" "postgres_query_connection_update" {
  name         = "ConnectionStrings--QueryConnection"
  value        = "Host=${module.postgres_flexible_server.resource_name}.postgres.database.azure.com;Database=readmodels;Username=${azurerm_user_assigned_identity.query_api.client_id};SslMode=Require"
  key_vault_id = module.key_vault.resource_id

  tags = local.common_tags

  depends_on = [
    module.postgres_flexible_server,
    azurerm_role_assignment.current_user_keyvault_admin
  ]
}

#=============================================================================
# OUTPUTS
#=============================================================================

output "postgres" {
  description = "PostgreSQL server information"
  value = {
    id               = module.postgres_flexible_server.resource_id
    name             = module.postgres_flexible_server.resource_name
    fqdn             = "${module.postgres_flexible_server.resource_name}.postgres.database.azure.com"
    administrator_login = null  # Using Azure AD authentication only
  }
}

output "storage_account" {
  description = "Storage account information"
  value = {
    id                    = module.storage_account.resource_id
    name                  = module.storage_account.resource_name
    primary_blob_endpoint = module.storage_account.primary_blob_endpoint
    primary_file_endpoint = module.storage_account.primary_file_endpoint
    containers           = module.storage_account.storage_containers
    file_shares          = module.storage_account.storage_file_shares
    queues               = module.storage_account.storage_queues
    tables               = module.storage_account.storage_tables
  }
}

output "databases" {
  description = "PostgreSQL databases"
  value = {
    events = {
      id   = azurerm_postgresql_flexible_server_database.events.id
      name = azurerm_postgresql_flexible_server_database.events.name
    }
    readmodels = {
      id   = azurerm_postgresql_flexible_server_database.readmodels.id
      name = azurerm_postgresql_flexible_server_database.readmodels.name
    }
  }
}