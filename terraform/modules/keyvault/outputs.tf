# Key Vault Module Outputs
output "id" {
  description = "Key Vault resource ID"
  value       = azurerm_key_vault.main.id
}

output "name" {
  description = "Key Vault name"
  value       = azurerm_key_vault.main.name
}

output "vault_uri" {
  description = "Key Vault URI"
  value       = azurerm_key_vault.main.vault_uri
}

output "tenant_id" {
  description = "Tenant ID"
  value       = azurerm_key_vault.main.tenant_id
}
