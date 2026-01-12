output "vnet_id" {
  description = "ID of the virtual network"
  value       = azurerm_virtual_network.main.id
}

output "vnet_name" {
  description = "Name of the virtual network"
  value       = azurerm_virtual_network.main.name
}

output "aks_node_pool_subnet_id" {
  description = "ID of the AKS node pool subnet"
  value       = azurerm_subnet.aks_node_pool.id
}

output "aks_node_pool_subnet_name" {
  description = "Name of the AKS node pool subnet"
  value       = azurerm_subnet.aks_node_pool.name
}

output "aks_api_server_subnet_id" {
  description = "ID of the AKS API server subnet (VNet integration)"
  value       = azurerm_subnet.aks_api_server.id
}

output "aks_api_server_subnet_name" {
  description = "Name of the AKS API server subnet"
  value       = azurerm_subnet.aks_api_server.name
}

output "private_endpoint_subnet_id" {
  description = "ID of the private endpoint subnet"
  value       = azurerm_subnet.private_endpoint.id
}

output "private_endpoint_subnet_name" {
  description = "Name of the private endpoint subnet"
  value       = azurerm_subnet.private_endpoint.name
}

output "postgresql_subnet_id" {
  description = "ID of the PostgreSQL delegated subnet"
  value       = azurerm_subnet.postgresql.id
}

output "postgresql_subnet_name" {
  description = "Name of the PostgreSQL subnet"
  value       = azurerm_subnet.postgresql.name
}

output "private_dns_zone_ids" {
  description = "Map of private DNS zone IDs"
  value = {
    postgresql    = azurerm_private_dns_zone.postgresql.id
    storage_blob  = azurerm_private_dns_zone.storage_blob.id
    keyvault      = azurerm_private_dns_zone.keyvault.id
    acr           = azurerm_private_dns_zone.acr.id
  }
}

output "private_dns_zone_names" {
  description = "Map of private DNS zone names"
  value = {
    postgresql    = azurerm_private_dns_zone.postgresql.name
    storage_blob  = azurerm_private_dns_zone.storage_blob.name
    keyvault      = azurerm_private_dns_zone.keyvault.name
    acr           = azurerm_private_dns_zone.acr.name
  }
}
