# Networking configuration for ti8m BeachBreak development environment
# Creates VNet, subnets, NSGs, and private DNS zones

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.85"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.4"
    }
  }
}

# Import shared configuration
locals {
  shared_config = {
    for file in fileset("${path.root}/../../shared", "*.tf") :
    file => file(join("/", [path.root, "../../shared", file]))
  }
}

# Random string for resource uniqueness
resource "random_string" "unique" {
  length  = 3
  upper   = false
  special = false
  numeric = true
}

#=============================================================================
# VIRTUAL NETWORK AND SUBNETS
#=============================================================================

resource "azurerm_virtual_network" "main" {
  name                = local.naming.vnet
  location            = var.location
  resource_group_name = azurerm_resource_group.shared.name
  address_space       = [local.network.vnet_cidr]

  tags = local.common_tags
}

# AKS Node Pool Subnet
resource "azurerm_subnet" "aks_nodes" {
  name                 = local.network.subnets.aks_nodes.name
  resource_group_name  = azurerm_resource_group.shared.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [local.network.subnets.aks_nodes.cidr]

  # Required for AKS nodes
  private_endpoint_network_policies_enabled = false
}

# AKS API Server VNet Integration Subnet
resource "azurerm_subnet" "aks_api" {
  name                 = local.network.subnets.aks_api.name
  resource_group_name  = azurerm_resource_group.shared.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [local.network.subnets.aks_api.cidr]

  # Required for AKS API server integration
  delegation {
    name = "aks-delegation"
    service_delegation {
      name    = local.network.subnets.aks_api.delegation
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }

  private_endpoint_network_policies_enabled = false
}

# PostgreSQL Subnet with delegation
resource "azurerm_subnet" "postgres" {
  name                 = local.network.subnets.postgres.name
  resource_group_name  = azurerm_resource_group.shared.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [local.network.subnets.postgres.cidr]

  # Required for PostgreSQL Flexible Server
  delegation {
    name = "postgres-delegation"
    service_delegation {
      name    = local.network.subnets.postgres.delegation
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }

  private_endpoint_network_policies_enabled = false
}

# Private Endpoints Subnet
resource "azurerm_subnet" "private_endpoints" {
  name                 = local.network.subnets.private_endpoints.name
  resource_group_name  = azurerm_resource_group.shared.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [local.network.subnets.private_endpoints.cidr]

  # Required for private endpoints
  private_endpoint_network_policies_enabled = false
}

# Future Gateway Subnet (reserved for Application Gateway)
resource "azurerm_subnet" "future_gateway" {
  name                 = local.network.subnets.future_gateway.name
  resource_group_name  = azurerm_resource_group.shared.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [local.network.subnets.future_gateway.cidr]

  private_endpoint_network_policies_enabled = false
}

#=============================================================================
# NETWORK SECURITY GROUPS
#=============================================================================

# NSG for AKS nodes
resource "azurerm_network_security_group" "aks_nodes" {
  name                = local.naming.nsg_aks_nodes
  location            = var.location
  resource_group_name = azurerm_resource_group.shared.name

  # Allow HTTPS from AKS API server subnet
  security_rule {
    name                       = "AllowAKSAPIServerHTTPS"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = local.network.subnets.aks_api.cidr
    destination_address_prefix = local.network.subnets.aks_nodes.cidr
  }

  # Allow container registry access via private endpoint
  security_rule {
    name                       = "AllowACRAccess"
    priority                   = 110
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = local.network.subnets.aks_nodes.cidr
    destination_address_prefix = local.network.subnets.private_endpoints.cidr
  }

  # Allow PostgreSQL access via private connection
  security_rule {
    name                       = "AllowPostgreSQLAccess"
    priority                   = 120
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "5432"
    source_address_prefix      = local.network.subnets.aks_nodes.cidr
    destination_address_prefix = local.network.subnets.postgres.cidr
  }

  # Allow DNS resolution
  security_rule {
    name                       = "AllowDNS"
    priority                   = 130
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Udp"
    source_port_range          = "*"
    destination_port_range     = "53"
    source_address_prefix      = local.network.subnets.aks_nodes.cidr
    destination_address_prefix = "*"
  }

  # Deny all other inbound traffic
  security_rule {
    name                       = "DenyAllInbound"
    priority                   = 4000
    direction                  = "Inbound"
    access                     = "Deny"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  tags = local.common_tags
}

# NSG for private endpoints
resource "azurerm_network_security_group" "private_endpoints" {
  name                = local.naming.nsg_private_endpoints
  location            = var.location
  resource_group_name = azurerm_resource_group.shared.name

  # Allow HTTPS inbound from AKS nodes
  security_rule {
    name                       = "AllowAKSNodes"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = local.network.subnets.aks_nodes.cidr
    destination_address_prefix = local.network.subnets.private_endpoints.cidr
  }

  # Allow DNS resolution inbound
  security_rule {
    name                       = "AllowDNSInbound"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Udp"
    source_port_range          = "*"
    destination_port_range     = "53"
    source_address_prefix      = local.network.vnet_cidr
    destination_address_prefix = local.network.subnets.private_endpoints.cidr
  }

  # Deny all other traffic
  security_rule {
    name                       = "DenyAllOther"
    priority                   = 4000
    direction                  = "Inbound"
    access                     = "Deny"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  tags = local.common_tags
}

# Associate NSGs with subnets
resource "azurerm_subnet_network_security_group_association" "aks_nodes" {
  subnet_id                 = azurerm_subnet.aks_nodes.id
  network_security_group_id = azurerm_network_security_group.aks_nodes.id
}

resource "azurerm_subnet_network_security_group_association" "private_endpoints" {
  subnet_id                 = azurerm_subnet.private_endpoints.id
  network_security_group_id = azurerm_network_security_group.private_endpoints.id
}

#=============================================================================
# PRIVATE DNS ZONES
#=============================================================================

# Private DNS zones for private endpoints
resource "azurerm_private_dns_zone" "container_registry" {
  name                = local.private_dns_zones.container_registry
  resource_group_name = azurerm_resource_group.shared.name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone" "key_vault" {
  name                = local.private_dns_zones.key_vault
  resource_group_name = azurerm_resource_group.shared.name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone" "storage_blob" {
  name                = local.private_dns_zones.storage_blob
  resource_group_name = azurerm_resource_group.shared.name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone" "storage_file" {
  name                = local.private_dns_zones.storage_file
  resource_group_name = azurerm_resource_group.shared.name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone" "storage_queue" {
  name                = local.private_dns_zones.storage_queue
  resource_group_name = azurerm_resource_group.shared.name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone" "storage_table" {
  name                = local.private_dns_zones.storage_table
  resource_group_name = azurerm_resource_group.shared.name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone" "postgres" {
  name                = local.private_dns_zones.postgres
  resource_group_name = azurerm_resource_group.shared.name

  tags = local.common_tags
}

# Link private DNS zones to VNet
resource "azurerm_private_dns_zone_virtual_network_link" "container_registry" {
  name                  = "${azurerm_virtual_network.main.name}-link"
  resource_group_name   = azurerm_resource_group.shared.name
  private_dns_zone_name = azurerm_private_dns_zone.container_registry.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "key_vault" {
  name                  = "${azurerm_virtual_network.main.name}-link"
  resource_group_name   = azurerm_resource_group.shared.name
  private_dns_zone_name = azurerm_private_dns_zone.key_vault.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "storage_blob" {
  name                  = "${azurerm_virtual_network.main.name}-link"
  resource_group_name   = azurerm_resource_group.shared.name
  private_dns_zone_name = azurerm_private_dns_zone.storage_blob.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "storage_file" {
  name                  = "${azurerm_virtual_network.main.name}-link"
  resource_group_name   = azurerm_resource_group.shared.name
  private_dns_zone_name = azurerm_private_dns_zone.storage_file.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "storage_queue" {
  name                  = "${azurerm_virtual_network.main.name}-link"
  resource_group_name   = azurerm_resource_group.shared.name
  private_dns_zone_name = azurerm_private_dns_zone.storage_queue.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "storage_table" {
  name                  = "${azurerm_virtual_network.main.name}-link"
  resource_group_name   = azurerm_resource_group.shared.name
  private_dns_zone_name = azurerm_private_dns_zone.storage_table.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "postgres" {
  name                  = "${azurerm_virtual_network.main.name}-link"
  resource_group_name   = azurerm_resource_group.shared.name
  private_dns_zone_name = azurerm_private_dns_zone.postgres.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false

  tags = local.common_tags
}

#=============================================================================
# OUTPUTS
#=============================================================================

output "vnet_id" {
  description = "Virtual network ID"
  value       = azurerm_virtual_network.main.id
}

output "vnet_name" {
  description = "Virtual network name"
  value       = azurerm_virtual_network.main.name
}

output "subnet_ids" {
  description = "Map of subnet names to IDs"
  value = {
    aks_nodes         = azurerm_subnet.aks_nodes.id
    aks_api           = azurerm_subnet.aks_api.id
    postgres          = azurerm_subnet.postgres.id
    private_endpoints = azurerm_subnet.private_endpoints.id
    future_gateway    = azurerm_subnet.future_gateway.id
  }
}

output "private_dns_zones" {
  description = "Map of private DNS zone names to IDs"
  value = {
    container_registry = azurerm_private_dns_zone.container_registry.id
    key_vault         = azurerm_private_dns_zone.key_vault.id
    storage_blob      = azurerm_private_dns_zone.storage_blob.id
    storage_file      = azurerm_private_dns_zone.storage_file.id
    storage_queue     = azurerm_private_dns_zone.storage_queue.id
    storage_table     = azurerm_private_dns_zone.storage_table.id
    postgres          = azurerm_private_dns_zone.postgres.id
  }
}