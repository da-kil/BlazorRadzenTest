output "resource_id" {
  value = azurerm_container_registry.this.id
}

output "resource_name" {
  value = azurerm_container_registry.this.name
}

output "artifact_identifier" {
  value = local.artifactIdentifier
}

output "login_server" {
  value       = azurerm_container_registry.this.login_server
  description = "The URL that can be used to log into the container registry"
}

output "credential_set_principal_ids" {
  value       = { for name, cs in azurerm_container_registry_credential_set.this : name => cs.identity[0].principal_id }
  description = "The `principal_id` of the gendered System Identity for the `credential_set` cache rule. <br /> **IMPORTANT!** Needs to be permitted on the configured credential Key Vault."
}
