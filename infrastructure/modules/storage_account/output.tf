output "resource_id" {
  value       = azurerm_storage_account.this.id
  description = "The Id from the storage account"
}

output "resource_name" {
  value       = azurerm_storage_account.this.name
  description = "The Name from the storage account"
}

output "primary_access_key" {
  value       = azurerm_storage_account.this.primary_access_key
  description = "the primary access key for this storage account"
  sensitive   = true
}

output "secondary_access_key" {
  value       = azurerm_storage_account.this.secondary_access_key
  description = "the secondary access key for this storage account"
  sensitive   = true
}

output "storage_account_primary_location" {
  description = "The primary Azure region where the Storage Account is located"
  value       = azurerm_storage_account.this.primary_location
}

output "storage_account_secondary_location" {
  description = "The secondary Azure region for geo-redundancy of the Storage Account"
  value       = azurerm_storage_account.this.secondary_location
}

output "artifact_identifier" {
  description = "Identifier of the deployed blueprint"
  value       = local.artifactIdentifier
}

#
# Storage endpoints
#
output "storage_account_primary_blob_endpoint" {
  description = "Primary endpoint for Blob storage"
  value       = azurerm_storage_account.this.primary_blob_endpoint
}

output "storage_account_secondary_blob_endpoint" {
  description = "Secondary endpoint for Blob storage (if geo-redundancy is enabled)"
  value       = azurerm_storage_account.this.secondary_blob_endpoint
}

output "storage_account_primary_file_endpoint" {
  description = "Primary endpoint for File storage"
  value       = azurerm_storage_account.this.primary_file_endpoint
}

output "storage_account_secondary_file_endpoint" {
  description = "Secondary endpoint for File storage (if geo-redundancy is enabled)"
  value       = azurerm_storage_account.this.secondary_file_endpoint
}

output "storage_account_primary_table_endpoint" {
  description = "Primary endpoint for Table storage"
  value       = azurerm_storage_account.this.primary_table_endpoint
}

output "storage_account_secondary_table_endpoint" {
  description = "Secondary endpoint for Table storage (if geo-redundancy is enabled)"
  value       = azurerm_storage_account.this.secondary_table_endpoint
}

output "storage_account_primary_queue_endpoint" {
  description = "Primary endpoint for Queue storage"
  value       = azurerm_storage_account.this.primary_queue_endpoint
}

output "storage_account_secondary_queue_endpoint" {
  description = "Secondary endpoint for Queue storage (if geo-redundancy is enabled)"
  value       = azurerm_storage_account.this.secondary_queue_endpoint
}

#
# Shares
#
output "storage_account_container_ids" {
  description = "Map of container names to their resource IDs"
  value       = { for k, v in azurerm_storage_container.this : k => v.id }
}

output "storage_account_container_names" {
  description = "Map of container keys to their names"
  value       = { for k, v in azurerm_storage_container.this : k => v.name }
}

output "storage_account_table_ids" {
  description = "Map of table names to their resource IDs"
  value       = { for k, v in azurerm_storage_table.this : k => v.id }
}

output "storage_account_table_names" {
  description = "Map of table keys to their names"
  value       = { for k, v in azurerm_storage_table.this : k => v.name }
}

output "storage_account_queue_ids" {
  description = "Map of queue names to their resource IDs"
  value       = { for k, v in azurerm_storage_queue.this : k => v.id }
}

output "storage_account_queue_names" {
  description = "Map of queue keys to their names"
  value       = { for k, v in azurerm_storage_queue.this : k => v.name }
}

output "storage_account_file_ids" {
  description = "Map of file share names to their resource IDs"
  value       = { for k, v in azurerm_storage_share.this : k => v.id }
}

output "storage_account_file_names" {
  description = "Map of file share keys to their names"
  value       = { for k, v in azurerm_storage_share.this : k => v.name }
}
