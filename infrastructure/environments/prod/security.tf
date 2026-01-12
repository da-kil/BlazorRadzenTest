# Security configuration for ti8m BeachBreak development environment
# Creates managed identities, RBAC assignments, and Key Vault

#=============================================================================
# DATA SOURCES
#=============================================================================

# Current Azure client configuration
data "azurerm_client_config" "current" {}

# Current user/service principal for Key Vault admin access
data "azuread_client_config" "current" {}

#=============================================================================
# MANAGED IDENTITIES
#=============================================================================

# AKS Cluster Identity (for Key Vault access)
resource "azurerm_user_assigned_identity" "aks_cluster" {
  name                = local.naming.aks_cluster_identity
  location            = var.location
  resource_group_name = azurerm_resource_group.compute.name

  tags = local.common_tags
}

# AKS Kubelet Identity (for Container Registry access)
resource "azurerm_user_assigned_identity" "aks_kubelet" {
  name                = local.naming.aks_kubelet_identity
  location            = var.location
  resource_group_name = azurerm_resource_group.compute.name

  tags = local.common_tags
}

# Command API Identity (for PostgreSQL and Key Vault access)
resource "azurerm_user_assigned_identity" "command_api" {
  name                = local.naming.command_api_identity
  location            = var.location
  resource_group_name = azurerm_resource_group.compute.name

  tags = local.common_tags
}

# Query API Identity (for PostgreSQL and Key Vault access)
resource "azurerm_user_assigned_identity" "query_api" {
  name                = local.naming.query_api_identity
  location            = var.location
  resource_group_name = azurerm_resource_group.compute.name

  tags = local.common_tags
}

# Frontend Identity (for Key Vault access)
resource "azurerm_user_assigned_identity" "frontend" {
  name                = local.naming.frontend_identity
  location            = var.location
  resource_group_name = azurerm_resource_group.compute.name

  tags = local.common_tags
}

#=============================================================================
# KEY VAULT MODULE
#=============================================================================

module "key_vault" {
  source = "../../modules/key_vault"

  env                   = var.environment
  deployment_index      = var.deployment_index
  component             = local.component_name
  location              = var.location
  resource_group_name   = azurerm_resource_group.shared.name
  resource_name_infix   = local.random_suffix

  # Network configuration - private endpoint required
  private_endpoint_subnet_id           = azurerm_subnet.private_endpoints.id
  central_dns_zone_resource_group_name = azurerm_resource_group.shared.name

  # Key Vault configuration
  sku                                  = var.key_vault_config.sku
  enable_rbac_authorization           = true
  purge_protection_enabled            = var.key_vault_config.purge_protection_enabled
  soft_delete_retention_days          = var.key_vault_config.soft_delete_retention_days

  # Network access - private endpoints only
  allowed_ips = var.allowed_ip_ranges

  # Monitoring
  diagnostics = {
    log_analytics_workspace_id = module.log_analytics_workspace.resource_id
    log_category_groups        = ["allLogs"]
    log_metrics               = ["AllMetrics"]
  }

  tags = local.common_tags

  depends_on = [
    azurerm_private_dns_zone.key_vault,
    azurerm_private_dns_zone_virtual_network_link.key_vault
  ]
}

#=============================================================================
# LOG ANALYTICS WORKSPACE MODULE
#=============================================================================

module "log_analytics_workspace" {
  source = "../../modules/log_analytics_workspace"

  env                 = var.environment
  deployment_index    = var.deployment_index
  component           = local.component_name
  location            = var.location
  resource_group_name = azurerm_resource_group.compute.name
  resource_name_infix = local.random_suffix

  # Workspace configuration
  sku               = var.monitoring_config.log_analytics_sku
  retention_in_days = var.monitoring_config.retention_in_days

  tags = local.common_tags
}

#=============================================================================
# RBAC ROLE ASSIGNMENTS
#=============================================================================

# AKS Cluster to Key Vault - Crypto Service Encryption User (for etcd encryption)
resource "azurerm_role_assignment" "aks_cluster_to_keyvault_crypto" {
  scope                = module.key_vault.resource_id
  role_definition_name = "Key Vault Crypto Service Encryption User"
  principal_id         = azurerm_user_assigned_identity.aks_cluster.principal_id
}

# AKS Kubelet to Container Registry - AcrPull (will be assigned in compute layer)
# This is handled by the container registry module

# Application identities to Key Vault - Key Vault Secrets User
resource "azurerm_role_assignment" "command_api_to_keyvault" {
  scope                = module.key_vault.resource_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.command_api.principal_id
}

resource "azurerm_role_assignment" "query_api_to_keyvault" {
  scope                = module.key_vault.resource_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.query_api.principal_id
}

resource "azurerm_role_assignment" "frontend_to_keyvault" {
  scope                = module.key_vault.resource_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.frontend.principal_id
}

# Current user/service principal as Key Vault Administrator for management
resource "azurerm_role_assignment" "current_user_keyvault_admin" {
  scope                = module.key_vault.resource_id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azuread_client_config.current.object_id
}

#=============================================================================
# KEY VAULT KEYS AND SECRETS
#=============================================================================

# etcd encryption key for AKS
resource "azurerm_key_vault_key" "etcd_encryption" {
  name         = "etcd-encryption-${var.environment}"
  key_vault_id = module.key_vault.resource_id
  key_type     = "RSA"
  key_size     = 2048
  key_opts     = ["decrypt", "encrypt"]

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.current_user_keyvault_admin
  ]
}

# Application Insights connection string (placeholder - will be set after App Insights creation)
resource "azurerm_key_vault_secret" "app_insights_connection_string" {
  name         = "ApplicationInsights--ConnectionString"
  value        = "placeholder-will-be-updated"
  key_vault_id = module.key_vault.resource_id

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.current_user_keyvault_admin
  ]

  lifecycle {
    ignore_changes = [value]
  }
}

# Azure AD tenant ID for authentication
resource "azurerm_key_vault_secret" "azure_ad_tenant_id" {
  name         = "AzureAd--TenantId"
  value        = data.azurerm_client_config.current.tenant_id
  key_vault_id = module.key_vault.resource_id

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.current_user_keyvault_admin
  ]
}

# PostgreSQL connection strings (placeholders - will be set after PostgreSQL creation)
resource "azurerm_key_vault_secret" "postgres_command_connection" {
  name         = "ConnectionStrings--CommandConnection"
  value        = "placeholder-will-be-updated"
  key_vault_id = module.key_vault.resource_id

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.current_user_keyvault_admin
  ]

  lifecycle {
    ignore_changes = [value]
  }
}

resource "azurerm_key_vault_secret" "postgres_query_connection" {
  name         = "ConnectionStrings--QueryConnection"
  value        = "placeholder-will-be-updated"
  key_vault_id = module.key_vault.resource_id

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.current_user_keyvault_admin
  ]

  lifecycle {
    ignore_changes = [value]
  }
}

#=============================================================================
# OUTPUTS
#=============================================================================

output "managed_identities" {
  description = "Map of managed identities"
  value = {
    aks_cluster = {
      id           = azurerm_user_assigned_identity.aks_cluster.id
      client_id    = azurerm_user_assigned_identity.aks_cluster.client_id
      principal_id = azurerm_user_assigned_identity.aks_cluster.principal_id
    }
    aks_kubelet = {
      id           = azurerm_user_assigned_identity.aks_kubelet.id
      client_id    = azurerm_user_assigned_identity.aks_kubelet.client_id
      principal_id = azurerm_user_assigned_identity.aks_kubelet.principal_id
    }
    command_api = {
      id           = azurerm_user_assigned_identity.command_api.id
      client_id    = azurerm_user_assigned_identity.command_api.client_id
      principal_id = azurerm_user_assigned_identity.command_api.principal_id
    }
    query_api = {
      id           = azurerm_user_assigned_identity.query_api.id
      client_id    = azurerm_user_assigned_identity.query_api.client_id
      principal_id = azurerm_user_assigned_identity.query_api.principal_id
    }
    frontend = {
      id           = azurerm_user_assigned_identity.frontend.id
      client_id    = azurerm_user_assigned_identity.frontend.client_id
      principal_id = azurerm_user_assigned_identity.frontend.principal_id
    }
  }
}

output "key_vault" {
  description = "Key Vault information"
  value = {
    id   = module.key_vault.resource_id
    name = module.key_vault.resource_name
    uri  = "https://${module.key_vault.resource_name}.vault.azure.net/"
  }
}

output "log_analytics_workspace" {
  description = "Log Analytics Workspace information"
  value = {
    id               = module.log_analytics_workspace.resource_id
    name             = module.log_analytics_workspace.resource_name
    workspace_id     = module.log_analytics_workspace.workspace_id
  }
}

output "etcd_encryption_key" {
  description = "etcd encryption key information for AKS"
  value = {
    id           = azurerm_key_vault_key.etcd_encryption.id
    key_vault_id = azurerm_key_vault_key.etcd_encryption.key_vault_id
    name         = azurerm_key_vault_key.etcd_encryption.name
  }
}