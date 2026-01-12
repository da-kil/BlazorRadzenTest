resource "azurerm_storage_account_customer_managed_key" "this" {
  count              = var.storage_encryption_customer_managed_key != null ? 1 : 0
  storage_account_id = azurerm_storage_account.this.id
  key_vault_uri      = var.storage_encryption_customer_managed_key.key_vault_uri
  key_name           = var.storage_encryption_customer_managed_key.key_name

  user_assigned_identity_id = var.storage_encryption_customer_managed_key.user_assigned_identity_id

  lifecycle {
    ignore_changes = [
      key_vault_id
    ]
  }
}
