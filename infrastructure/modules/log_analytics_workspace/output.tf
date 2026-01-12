output "resource_name" {
  description = "The name of the resource"
  value       = azurerm_log_analytics_workspace.this.name
}

output "resource_id" {
  description = "The identifier of the resource"
  value       = azurerm_log_analytics_workspace.this.id
}

output "artifact_identifier" {
  value = local.artifactIdentifier
}

output "workspace_id" {
  description = "The workspace ID of this Log Analytics Workspace"
  value       = azurerm_log_analytics_workspace.this.workspace_id
}
