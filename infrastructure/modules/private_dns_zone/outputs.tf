output "artifact_identifier" {
  description = "Identifier of the deployed blueprint"
  value       = local.artifactIdentifier
}

output "private_dns_zone_id" {
  description = "Identifier of the deployed dns zone"
  value       = azurerm_private_dns_zone.this.id
}

output "private_dns_zone_name" {
  description = "Name of the deployed dns zone"
  value       = azurerm_private_dns_zone.this.name
}
