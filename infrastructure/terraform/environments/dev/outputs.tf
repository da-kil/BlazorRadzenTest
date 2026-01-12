# Resource Group Outputs
output "k8s_resource_group_name" {
  description = "Name of the Kubernetes resource group"
  value       = azurerm_resource_group.k8s.name
}

output "db_resource_group_name" {
  description = "Name of the database resource group"
  value       = azurerm_resource_group.db.name
}

# Networking Outputs
output "vnet_id" {
  description = "ID of the virtual network"
  value       = module.networking.vnet_id
}

output "vnet_name" {
  description = "Name of the virtual network"
  value       = module.networking.vnet_name
}

# AKS Outputs
output "aks_cluster_name" {
  description = "Name of the AKS cluster"
  value       = module.aks.cluster_name
}

output "aks_cluster_fqdn" {
  description = "FQDN of the AKS cluster (private)"
  value       = module.aks.cluster_fqdn
}

output "aks_oidc_issuer_url" {
  description = "OIDC issuer URL for workload identity"
  value       = module.aks.oidc_issuer_url
}

# PostgreSQL Outputs
output "postgresql_server_name" {
  description = "Name of the PostgreSQL server"
  value       = module.postgresql.postgresql_server_name
}

output "postgresql_fqdn" {
  description = "FQDN of the PostgreSQL server"
  value       = module.postgresql.postgresql_server_fqdn
}

output "postgresql_admin_username" {
  description = "Admin username for PostgreSQL"
  value       = module.postgresql.postgresql_admin_username
}

output "postgresql_admin_password" {
  description = "Admin password for PostgreSQL"
  value       = module.postgresql.postgresql_admin_password
  sensitive   = true
}

output "postgresql_connection_string" {
  description = "Connection string for PostgreSQL"
  value       = module.postgresql.postgresql_connection_string
  sensitive   = true
}

# Storage Outputs
output "storage_account_name" {
  description = "Name of the storage account"
  value       = module.storage.storage_account_name
}

output "storage_primary_blob_endpoint" {
  description = "Primary blob endpoint"
  value       = module.storage.storage_primary_blob_endpoint
}

output "storage_primary_connection_string" {
  description = "Primary connection string for storage"
  value       = module.storage.storage_primary_connection_string
  sensitive   = true
}

# Key Vault Outputs
output "keyvault_name" {
  description = "Name of the Key Vault"
  value       = module.keyvault.keyvault_name
}

output "keyvault_uri" {
  description = "URI of the Key Vault"
  value       = module.keyvault.keyvault_uri
}

# Container Registry Outputs
output "acr_name" {
  description = "Name of the Container Registry"
  value       = azurerm_container_registry.main.name
}

output "acr_login_server" {
  description = "Login server for the Container Registry"
  value       = azurerm_container_registry.main.login_server
}

# Monitoring Outputs
output "log_analytics_workspace_id" {
  description = "ID of the Log Analytics workspace"
  value       = module.monitoring.log_analytics_workspace_id
}

output "application_insights_instrumentation_key" {
  description = "Instrumentation key for Application Insights"
  value       = module.monitoring.application_insights_instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Connection string for Application Insights"
  value       = module.monitoring.application_insights_connection_string
  sensitive   = true
}
