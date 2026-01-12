# Shared local values for ti8m BeachBreak infrastructure
# Contains naming conventions, common configurations, and resource tagging

locals {
  # Project configuration
  project_name = "beachbreak"
  component_name = "bb"  # Shortened for resources with length limitations (e.g., PostgreSQL)
  location_short = "swn" # Switzerland North abbreviation

  # Common tags applied to all resources
  common_tags = {
    Project     = "ti8m-beachbreak"
    ManagedBy   = "terraform"
    Environment = var.environment
    Owner       = "ti8m"
    CostCenter  = "beachbreak"
  }

  # Resource naming conventions following ti8m standards
  # Pattern: {prefix}-{component}-{workload}-{env}-{location}-{index}
  naming = {
    resource_group_compute = "rg-${local.project_name}-compute-${var.environment}-${local.location_short}-${var.deployment_index}"
    resource_group_data    = "rg-${local.project_name}-data-${var.environment}-${local.location_short}-${var.deployment_index}"
    resource_group_shared  = "rg-${local.project_name}-shared-${var.environment}-${local.location_short}-${var.deployment_index}"

    # Virtual network and subnets
    vnet                   = "vnet-${local.project_name}-${var.environment}-${local.location_short}-${var.deployment_index}"
    subnet_aks_nodes       = "snet-aks-nodes-${var.environment}-${local.location_short}-${var.deployment_index}"
    subnet_aks_api         = "snet-aks-api-${var.environment}-${local.location_short}-${var.deployment_index}"
    subnet_postgres        = "snet-postgres-${var.environment}-${local.location_short}-${var.deployment_index}"
    subnet_private_endpoints = "snet-private-endpoints-${var.environment}-${local.location_short}-${var.deployment_index}"

    # Network security groups
    nsg_aks_nodes          = "nsg-aks-nodes-${var.environment}-${local.location_short}-${var.deployment_index}"
    nsg_private_endpoints  = "nsg-private-endpoints-${var.environment}-${local.location_short}-${var.deployment_index}"

    # Core resources (using component name for ti8m module compatibility)
    aks_cluster            = "aks-${local.component_name}-${var.environment}-${local.location_short}-${var.deployment_index}"
    container_registry     = "acr${local.component_name}${var.environment}${local.location_short}${var.deployment_index}"
    key_vault              = "kv-${local.component_name}-${var.environment}-${local.location_short}-${var.deployment_index}"
    postgres_server        = "psql-${local.component_name}-${var.environment}-${local.location_short}-${var.deployment_index}"
    storage_account        = "st${local.component_name}${var.environment}${local.location_short}${var.deployment_index}"
    log_analytics          = "law-${local.component_name}-${var.environment}-${local.location_short}-${var.deployment_index}"

    # Managed identities
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

  # Private DNS zones required by ti8m modules
  private_dns_zones = {
    container_registry = "privatelink.azurecr.io"
    key_vault         = "privatelink.vaultcore.azure.net"
    storage_blob      = "privatelink.blob.core.windows.net"
    storage_file      = "privatelink.file.core.windows.net"
    storage_queue     = "privatelink.queue.core.windows.net"
    storage_table     = "privatelink.table.core.windows.net"
    postgres          = "privatelink.postgres.database.azure.com"
  }

  # Environment-specific defaults (can be overridden in terraform.tfvars)
  environment_defaults = {
    dev = {
      # Cost-optimized settings for development
      aks = {
        sku_tier = "Standard"
        kubernetes_version = "1.29"
        default_node_pool = {
          vm_size = "Standard_D2s_v5"
          node_count = 2
          min_count = 1
          max_count = 3
          availability_zones = ["1"]
        }
      }
      postgres = {
        sku_name = "B_Standard_B1ms"
        storage_mb = 32768
        backup_retention_days = 7
        geo_redundant_backup_enabled = false
        high_availability_enabled = false
      }
      storage = {
        account_tier = "Standard"
        account_replication_type = "LRS"
        account_kind = "StorageV2"
      }
      container_registry = {
        sku = "Basic"
        geo_replication_enabled = false
      }
    }
    prod = {
      # Performance-optimized settings for production
      aks = {
        sku_tier = "Standard"
        kubernetes_version = "1.29"
        default_node_pool = {
          vm_size = "Standard_D4s_v5"
          node_count = 3
          min_count = 2
          max_count = 10
          availability_zones = ["1"]  # Single zone for cost optimization
        }
        additional_node_pools = {
          workload = {
            vm_size = "Standard_D2s_v5"
            node_count = 2
            min_count = 1
            max_count = 5
            availability_zones = ["1"]
          }
        }
      }
      postgres = {
        sku_name = "GP_Standard_D2s_v3"
        storage_mb = 65536
        backup_retention_days = 30
        geo_redundant_backup_enabled = true
        high_availability_enabled = false  # Single zone strategy
      }
      storage = {
        account_tier = "Standard"
        account_replication_type = "ZRS"
        account_kind = "StorageV2"
      }
      container_registry = {
        sku = "Premium"
        geo_replication_enabled = false
      }
    }
  }

  # Random string for resource uniqueness (required by ti8m modules)
  random_suffix = random_string.unique.result
}