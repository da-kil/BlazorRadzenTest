# Main orchestration file for ti8m BeachBreak development environment
# This file coordinates all infrastructure components and manages dependencies

terraform {
  required_version = ">= 1.5"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.85"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.47"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.4"
    }
  }

  # Backend configuration - loaded from backend.hcl during init
  backend "azurerm" {
    # Configuration loaded from backend.hcl file
  }
}

# Configure Azure Provider
provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = false
      recover_soft_deleted_key_vaults = true
    }
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
    virtual_machine {
      delete_os_disk_on_deletion     = true
      graceful_shutdown             = false
      skip_shutdown_and_force_delete = false
    }
  }
}

provider "azuread" {
  # Use environment variables or Azure CLI authentication
}

provider "random" {
  # No specific configuration required
}

#=============================================================================
# IMPORT SHARED CONFIGURATION
#=============================================================================

# Import shared locals and variables
# The locals and variables are referenced directly from the shared files

# Include shared locals.tf content
locals {
  # Import all shared configuration
  project_name    = "beachbreak"
  component_name  = "bb"
  location_short  = "swn"
  random_suffix   = random_string.unique.result

  # Common tags applied to all resources
  common_tags = {
    Project     = "ti8m-beachbreak"
    ManagedBy   = "terraform"
    Environment = var.environment
    Owner       = "ti8m"
    CostCenter  = "beachbreak"
    DeployedBy  = "terraform"
    DeployedAt  = timestamp()
  }

  # Resource naming conventions
  naming = {
    resource_group_compute = "rg-${local.project_name}-compute-${var.environment}-${local.location_short}-${var.deployment_index}"
    resource_group_data    = "rg-${local.project_name}-data-${var.environment}-${local.location_short}-${var.deployment_index}"
    resource_group_shared  = "rg-${local.project_name}-shared-${var.environment}-${local.location_short}-${var.deployment_index}"

    vnet                   = "vnet-${local.project_name}-${var.environment}-${local.location_short}-${var.deployment_index}"
    subnet_aks_nodes       = "snet-aks-nodes-${var.environment}-${local.location_short}-${var.deployment_index}"
    subnet_aks_api         = "snet-aks-api-${var.environment}-${local.location_short}-${var.deployment_index}"
    subnet_postgres        = "snet-postgres-${var.environment}-${local.location_short}-${var.deployment_index}"
    subnet_private_endpoints = "snet-private-endpoints-${var.environment}-${local.location_short}-${var.deployment_index}"

    nsg_aks_nodes          = "nsg-aks-nodes-${var.environment}-${local.location_short}-${var.deployment_index}"
    nsg_private_endpoints  = "nsg-private-endpoints-${var.environment}-${local.location_short}-${var.deployment_index}"

    aks_cluster            = "aks-${local.component_name}-${var.environment}-${local.location_short}-${var.deployment_index}"
    container_registry     = "acr${local.component_name}${var.environment}${local.location_short}${var.deployment_index}"
    key_vault              = "kv-${local.component_name}-${var.environment}-${local.location_short}-${var.deployment_index}"
    postgres_server        = "psql-${local.component_name}-${var.environment}-${local.location_short}-${var.deployment_index}"
    storage_account        = "st${local.component_name}${var.environment}${local.location_short}${var.deployment_index}"
    log_analytics          = "law-${local.component_name}-${var.environment}-${local.location_short}-${var.deployment_index}"

    aks_cluster_identity   = "umi-aks-cluster-${var.environment}-${local.location_short}-${var.deployment_index}"
    aks_kubelet_identity   = "umi-aks-kubelet-${var.environment}-${local.location_short}-${var.deployment_index}"
    command_api_identity   = "umi-commandapi-${var.environment}-${local.location_short}-${var.deployment_index}"
    query_api_identity     = "umi-queryapi-${var.environment}-${local.location_short}-${var.deployment_index}"
    frontend_identity      = "umi-frontend-${var.environment}-${local.location_short}-${var.deployment_index}"
  }

  # Network configuration
  network = {
    vnet_cidr = "10.0.0.0/16"
    subnets = {
      aks_nodes = {
        cidr = "10.0.1.0/24"
        name = local.naming.subnet_aks_nodes
      }
      aks_api = {
        cidr = "10.0.2.0/27"
        name = local.naming.subnet_aks_api
        delegation = "Microsoft.ContainerService/managedClusters"
      }
      postgres = {
        cidr = "10.0.3.0/27"
        name = local.naming.subnet_postgres
        delegation = "Microsoft.DBforPostgreSQL/flexibleServers"
      }
      private_endpoints = {
        cidr = "10.0.4.0/27"
        name = local.naming.subnet_private_endpoints
      }
      future_gateway = {
        cidr = "10.0.5.0/27"
        name = "snet-future-gateway-${var.environment}-${local.location_short}-${var.deployment_index}"
      }
    }
  }

  # Private DNS zones
  private_dns_zones = {
    container_registry = "privatelink.azurecr.io"
    key_vault         = "privatelink.vaultcore.azure.net"
    storage_blob      = "privatelink.blob.core.windows.net"
    storage_file      = "privatelink.file.core.windows.net"
    storage_queue     = "privatelink.queue.core.windows.net"
    storage_table     = "privatelink.table.core.windows.net"
    postgres          = "privatelink.postgres.database.azure.com"
  }
}

#=============================================================================
# RESOURCE GROUPS
#=============================================================================

# Compute resource group (AKS, Container Registry, Log Analytics)
resource "azurerm_resource_group" "compute" {
  name     = local.naming.resource_group_compute
  location = var.location

  tags = local.common_tags
}

# Data resource group (PostgreSQL, Storage Account)
resource "azurerm_resource_group" "data" {
  name     = local.naming.resource_group_data
  location = var.location

  tags = local.common_tags
}

# Shared resource group (Key Vault, Virtual Network, Private DNS)
resource "azurerm_resource_group" "shared" {
  name     = local.naming.resource_group_shared
  location = var.location

  tags = local.common_tags
}

#=============================================================================
# RANDOM SUFFIX FOR UNIQUENESS
#=============================================================================

resource "random_string" "unique" {
  length  = 3
  upper   = false
  special = false
  numeric = true
}

#=============================================================================
# DEPLOYMENT SUMMARY
#=============================================================================

# Local values for deployment tracking
locals {
  deployment_summary = {
    environment          = var.environment
    location            = var.location
    deployment_index    = var.deployment_index
    component_name      = local.component_name
    random_suffix       = local.random_suffix

    # Resource group names
    resource_groups = {
      compute = azurerm_resource_group.compute.name
      data    = azurerm_resource_group.data.name
      shared  = azurerm_resource_group.shared.name
    }

    # Network configuration
    network_config = {
      vnet_cidr = local.network.vnet_cidr
      subnets   = local.network.subnets
    }

    # Deployment timestamp
    deployed_at = timestamp()
  }
}

#=============================================================================
# OUTPUTS
#=============================================================================

output "deployment_summary" {
  description = "Summary of the deployed infrastructure"
  value = local.deployment_summary
}

output "resource_groups" {
  description = "Created resource groups"
  value = {
    compute = {
      id   = azurerm_resource_group.compute.id
      name = azurerm_resource_group.compute.name
    }
    data = {
      id   = azurerm_resource_group.data.id
      name = azurerm_resource_group.data.name
    }
    shared = {
      id   = azurerm_resource_group.shared.id
      name = azurerm_resource_group.shared.name
    }
  }
}

output "environment_info" {
  description = "Environment configuration information"
  value = {
    environment      = var.environment
    location        = var.location
    deployment_index = var.deployment_index
    random_suffix   = local.random_suffix
  }
}

# Network outputs (from networking.tf)
# Note: These are referenced from the networking.tf outputs

# Security outputs (from security.tf)
# Note: These are referenced from the security.tf outputs

# Data layer outputs (from data-layer.tf)
# Note: These are referenced from the data-layer.tf outputs

# Compute layer outputs (from compute-layer.tf)
# Note: These are referenced from the compute-layer.tf outputs

#=============================================================================
# COST ESTIMATION
#=============================================================================

locals {
  estimated_monthly_costs = {
    aks_cluster         = "~150 CHF"
    postgresql         = "~45 CHF"
    storage_account    = "~25 CHF"
    container_registry = "~5 CHF"
    key_vault          = "~3 CHF"
    log_analytics      = "~30 CHF"
    networking         = "~25 CHF"
    total_estimated    = "~283 CHF"
    note              = "Development environment with cost optimization"
  }
}

output "cost_estimation" {
  description = "Estimated monthly costs for the development environment"
  value = local.estimated_monthly_costs
}

#=============================================================================
# VALIDATION
#=============================================================================

# Validate environment configuration
check "environment_validation" {
  assert {
    condition     = var.environment == "dev"
    error_message = "This configuration is specifically for the development environment."
  }
}

# Validate naming conventions
check "naming_validation" {
  assert {
    condition = length(local.naming.storage_account) <= 24 && length(local.naming.storage_account) >= 3
    error_message = "Storage account name must be between 3 and 24 characters."
  }
}

# Validate network configuration
check "network_validation" {
  assert {
    condition = can(cidrnetmask(local.network.vnet_cidr))
    error_message = "VNet CIDR must be a valid CIDR block."
  }
}