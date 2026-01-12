output "keyvault_id" {
  description = "ID of the Key Vault"
  value       = azurerm_key_vault.main.id
}

output "keyvault_name" {
  description = "Name of the Key Vault"
  value       = azurerm_key_vault.main.name
}

output "keyvault_uri" {
  description = "URI of the Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

output "etcd_encryption_key_id" {
  description = "ID of the etcd encryption key for AKS"
  value       = azurerm_key_vault_key.etcd_encryption.id
}

output "etcd_encryption_key_version_id" {
  description = "Versioned ID of the etcd encryption key"
  value       = azurerm_key_vault_key.etcd_encryption.versionless_id
}
