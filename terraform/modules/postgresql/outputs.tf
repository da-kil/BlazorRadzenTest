# PostgreSQL Module Outputs
output "server_id" {
  description = "PostgreSQL server ID"
  value       = azurerm_postgresql_flexible_server.main.id
}

output "server_name" {
  description = "PostgreSQL server name"
  value       = azurerm_postgresql_flexible_server.main.name
}

output "server_fqdn" {
  description = "PostgreSQL server FQDN"
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "administrator_login" {
  description = "Administrator login username"
  value       = azurerm_postgresql_flexible_server.main.administrator_login
}

output "eventstore_database_name" {
  description = "Event store database name"
  value       = azurerm_postgresql_flexible_server_database.eventstore.name
}

output "readmodels_database_name" {
  description = "Read models database name"
  value       = azurerm_postgresql_flexible_server_database.readmodels.name
}

output "eventstore_connection_string_secret_name" {
  description = "Key Vault secret name for event store connection string"
  value       = azurerm_key_vault_secret.eventstore_connection_string.name
}

output "readmodels_connection_string_secret_name" {
  description = "Key Vault secret name for read models connection string"
  value       = azurerm_key_vault_secret.readmodels_connection_string.name
}
