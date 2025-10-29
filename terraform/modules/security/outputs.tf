# Security Module Outputs
output "commandapi_identity_id" {
  description = "CommandApi managed identity resource ID"
  value       = azurerm_user_assigned_identity.commandapi.id
}

output "commandapi_identity_principal_id" {
  description = "CommandApi managed identity principal ID"
  value       = azurerm_user_assigned_identity.commandapi.principal_id
}

output "commandapi_identity_client_id" {
  description = "CommandApi managed identity client ID"
  value       = azurerm_user_assigned_identity.commandapi.client_id
}

output "queryapi_identity_id" {
  description = "QueryApi managed identity resource ID"
  value       = azurerm_user_assigned_identity.queryapi.id
}

output "queryapi_identity_principal_id" {
  description = "QueryApi managed identity principal ID"
  value       = azurerm_user_assigned_identity.queryapi.principal_id
}

output "queryapi_identity_client_id" {
  description = "QueryApi managed identity client ID"
  value       = azurerm_user_assigned_identity.queryapi.client_id
}

output "frontend_identity_id" {
  description = "Frontend managed identity resource ID"
  value       = azurerm_user_assigned_identity.frontend.id
}

output "frontend_identity_principal_id" {
  description = "Frontend managed identity principal ID"
  value       = azurerm_user_assigned_identity.frontend.principal_id
}

output "frontend_identity_client_id" {
  description = "Frontend managed identity client ID"
  value       = azurerm_user_assigned_identity.frontend.client_id
}
