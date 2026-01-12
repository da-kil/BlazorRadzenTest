output "postgresql_server_id" {
  description = "ID of the PostgreSQL server"
  value       = azurerm_postgresql_flexible_server.main.id
}

output "postgresql_server_name" {
  description = "Name of the PostgreSQL server"
  value       = azurerm_postgresql_flexible_server.main.name
}

output "postgresql_server_fqdn" {
  description = "FQDN of the PostgreSQL server"
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "postgresql_admin_username" {
  description = "Admin username for PostgreSQL"
  value       = azurerm_postgresql_flexible_server.main.administrator_login
}

output "postgresql_admin_password" {
  description = "Admin password for PostgreSQL"
  value       = random_password.pg_admin.result
  sensitive   = true
}

output "postgresql_connection_string" {
  description = "Connection string for PostgreSQL"
  value       = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Database=postgres;Username=${azurerm_postgresql_flexible_server.main.administrator_login};Password=${random_password.pg_admin.result};SSL Mode=Require"
  sensitive   = true
}
