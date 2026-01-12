output "resource_id" {
  value       = azurerm_postgresql_flexible_server.this.id
  description = "The Id from the Key Vault"
}

output "resource_name" {
  value       = azurerm_postgresql_flexible_server.this.name
  description = "The Name from the Key Vault"
}

output "artifact_identifier" {
  value       = local.artifactIdentifier
  description = "The Identifier which identifies the artifact by tag."
}
