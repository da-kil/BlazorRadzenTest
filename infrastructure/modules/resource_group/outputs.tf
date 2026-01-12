output "artifact_identifier" {
  description = "Identifier of the deployed blueprint"
  value       = local.artifactIdentifier
}

output "resource_group_name" {
  description = "Name of the created resource group"
  value       = azurerm_resource_group.this.name
}
