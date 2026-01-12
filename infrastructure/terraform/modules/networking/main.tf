# Local variables for consistent naming
locals {
  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    northeurope      = "eun"
    westeurope       = "euw"
    swedencentral    = "swc"
  }
  loc = lookup(local.location_short, var.location, "swn")

  common_tags = merge(var.tags, {
    Environment = var.env
    ManagedBy   = "Terraform"
  })
}

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "vnet-beachbreak-${var.env}-${local.loc}-01"
  location            = var.location
  resource_group_name = var.resource_group_name
  address_space       = var.vnet_address_space

  tags = local.common_tags
}

# AKS Node Pool Subnet
resource "azurerm_subnet" "aks_node_pool" {
  name                 = "snet-aks-nodes-${var.env}-${local.loc}-01"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [var.aks_node_pool_subnet_cidr]

  # No delegation needed for node pool subnet
}

# AKS API Server Subnet (VNet Integration)
resource "azurerm_subnet" "aks_api_server" {
  name                 = "snet-aks-api-${var.env}-${local.loc}-01"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [var.aks_api_server_subnet_cidr]

  # Delegation required for AKS API server VNet integration
  delegation {
    name = "aks-delegation"

    service_delegation {
      name    = "Microsoft.ContainerService/managedClusters"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}

# Private Endpoint Subnet
resource "azurerm_subnet" "private_endpoint" {
  name                 = "snet-pe-${var.env}-${local.loc}-01"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [var.private_endpoint_subnet_cidr]

  # Disable private endpoint network policies
  private_endpoint_network_policies_enabled = false
}

# PostgreSQL Subnet with Delegation
resource "azurerm_subnet" "postgresql" {
  name                 = "snet-pg-${var.env}-${local.loc}-01"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [var.postgresql_subnet_cidr]

  # Delegation required for PostgreSQL Flexible Server
  delegation {
    name = "postgresql-delegation"

    service_delegation {
      name = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action",
      ]
    }
  }
}

# Network Security Group for AKS Node Pool
resource "azurerm_network_security_group" "aks_node_pool" {
  name                = "nsg-aks-nodes-${var.env}-${local.loc}-01"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Associate NSG with AKS Node Pool Subnet
resource "azurerm_subnet_network_security_group_association" "aks_node_pool" {
  subnet_id                 = azurerm_subnet.aks_node_pool.id
  network_security_group_id = azurerm_network_security_group.aks_node_pool.id
}

# Network Security Group for Private Endpoints
resource "azurerm_network_security_group" "private_endpoint" {
  name                = "nsg-pe-${var.env}-${local.loc}-01"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Associate NSG with Private Endpoint Subnet
resource "azurerm_subnet_network_security_group_association" "private_endpoint" {
  subnet_id                 = azurerm_subnet.private_endpoint.id
  network_security_group_id = azurerm_network_security_group.private_endpoint.id
}

# Private DNS Zone for PostgreSQL
resource "azurerm_private_dns_zone" "postgresql" {
  name                = "privatelink.postgres.database.azure.com"
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Link PostgreSQL DNS Zone to VNet
resource "azurerm_private_dns_zone_virtual_network_link" "postgresql" {
  name                  = "pdnslink-pg-${var.env}-${local.loc}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.postgresql.name
  virtual_network_id    = azurerm_virtual_network.main.id

  tags = local.common_tags
}

# Private DNS Zone for Storage Account (Blob)
resource "azurerm_private_dns_zone" "storage_blob" {
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Link Storage Blob DNS Zone to VNet
resource "azurerm_private_dns_zone_virtual_network_link" "storage_blob" {
  name                  = "pdnslink-blob-${var.env}-${local.loc}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.storage_blob.name
  virtual_network_id    = azurerm_virtual_network.main.id

  tags = local.common_tags
}

# Private DNS Zone for Key Vault
resource "azurerm_private_dns_zone" "keyvault" {
  name                = "privatelink.vaultcore.azure.net"
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Link Key Vault DNS Zone to VNet
resource "azurerm_private_dns_zone_virtual_network_link" "keyvault" {
  name                  = "pdnslink-kv-${var.env}-${local.loc}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.keyvault.name
  virtual_network_id    = azurerm_virtual_network.main.id

  tags = local.common_tags
}

# Private DNS Zone for Container Registry
resource "azurerm_private_dns_zone" "acr" {
  name                = "privatelink.azurecr.io"
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Link Container Registry DNS Zone to VNet
resource "azurerm_private_dns_zone_virtual_network_link" "acr" {
  name                  = "pdnslink-acr-${var.env}-${local.loc}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.acr.name
  virtual_network_id    = azurerm_virtual_network.main.id

  tags = local.common_tags
}
