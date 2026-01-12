resource "azurerm_role_assignment" "screts_users" {
  for_each = var.secret_users

  role_definition_name = "Key Vault Secrets User"
  principal_id         = each.value
  scope                = azurerm_key_vault.this.id
  description          = "Managed by Terraform 🤖"
}
