output "storage_account_id" {
  description = "ID of the storage account"
  value       = azurerm_storage_account.main.id
}

output "storage_account_name" {
  description = "Name of the storage account"
  value       = azurerm_storage_account.main.name
}

output "storage_primary_blob_endpoint" {
  description = "Primary blob endpoint"
  value       = azurerm_storage_account.main.primary_blob_endpoint
}

output "storage_primary_connection_string" {
  description = "Primary connection string (requires shared key access)"
  value       = azurerm_storage_account.main.primary_connection_string
  sensitive   = true
}

output "container_ids" {
  description = "Map of container names to IDs"
  value       = { for k, v in azurerm_storage_container.containers : k => v.id }
}

output "container_names" {
  description = "List of container names"
  value       = [for c in azurerm_storage_container.containers : c.name]
}
