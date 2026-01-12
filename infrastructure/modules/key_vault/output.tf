output "resource_id" {
  value       = azurerm_key_vault.this.id
  description = "ID of the created key vault"
}

output "resource_name" {
  value       = azurerm_key_vault.this.name
  description = "Name of the created key vault"
}

output "artifact_identifier" {
  description = "Identifier of the deployed blueprint"
  value       = local.artifactIdentifier
}
