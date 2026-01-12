resource "azurerm_key_vault" "this" {
  name                        = local.resource_name
  resource_group_name         = var.resource_group_name
  location                    = var.location
  sku_name                    = var.sku
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  enabled_for_disk_encryption = true
  rbac_authorization_enabled  = var.enable_rbac_authorization

  purge_protection_enabled   = true
  soft_delete_retention_days = 30

  public_network_access_enabled = var.allowed_ips != null && length(var.allowed_ips) > 0

  network_acls {
    bypass         = "AzureServices"
    default_action = "Deny"
    ip_rules       = var.allowed_ips
  }

  tags = local.merged_tags
}

//TF role assignment
resource "azurerm_role_assignment" "key_vault_administrator" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azurerm_client_config.current.object_id
  description          = "Managed by Terraform 🤖"
}
