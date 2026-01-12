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

  # Generate a random suffix for Key Vault name uniqueness
  kv_name = "kv-bb-${var.env}-${local.loc}-01"

  common_tags = merge(var.tags, {
    Environment = var.env
    ManagedBy   = "Terraform"
  })
}

# Data source for current client
data "azurerm_client_config" "current" {}

# Random string for unique naming
resource "random_string" "kv_suffix" {
  length  = 4
  special = false
  upper   = false
}

# Key Vault
resource "azurerm_key_vault" "main" {
  name                       = "${local.kv_name}-${random_string.kv_suffix.result}"
  location                   = var.location
  resource_group_name        = var.resource_group_name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = var.sku
  soft_delete_retention_days = var.soft_delete_retention_days
  purge_protection_enabled   = var.purge_protection_enabled

  # Enable RBAC authorization
  enable_rbac_authorization = true

  # Network ACLs - deny by default, use private endpoint
  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
  }

  tags = local.common_tags
}

# Private Endpoint for Key Vault
resource "azurerm_private_endpoint" "keyvault" {
  name                = "pe-${local.kv_name}-${random_string.kv_suffix.result}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "psc-${local.kv_name}"
    private_connection_resource_id = azurerm_key_vault.main.id
    is_manual_connection           = false
    subresource_names              = ["vault"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [var.private_dns_zone_id]
  }

  tags = local.common_tags
}

# Role Assignment: Current Terraform executor as Key Vault Administrator
resource "azurerm_role_assignment" "terraform_kv_admin" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azurerm_client_config.current.object_id
}

# Create etcd encryption key for AKS
resource "azurerm_key_vault_key" "etcd_encryption" {
  name         = "etcd-encryption-key"
  key_vault_id = azurerm_key_vault.main.id
  key_type     = "RSA"
  key_size     = 2048

  key_opts = [
    "decrypt",
    "encrypt",
    "sign",
    "unwrapKey",
    "verify",
    "wrapKey",
  ]

  depends_on = [azurerm_role_assignment.terraform_kv_admin]

  tags = local.common_tags
}

# Diagnostic Settings for Key Vault
resource "azurerm_monitor_diagnostic_setting" "keyvault" {
  count                      = var.diagnostics_workspace_id != "" ? 1 : 0
  name                       = "diag-${local.kv_name}"
  target_resource_id         = azurerm_key_vault.main.id
  log_analytics_workspace_id = var.diagnostics_workspace_id

  enabled_log {
    category = "AuditEvent"
  }

  enabled_log {
    category = "AzurePolicyEvaluationDetails"
  }

  metric {
    category = "AllMetrics"
    enabled  = true
  }
}
