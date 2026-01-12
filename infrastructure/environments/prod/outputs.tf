# Output values for ti8m BeachBreak development environment

#=============================================================================
# DEPLOYMENT INFORMATION
#=============================================================================

output "deployment_info" {
  description = "General deployment information"
  value = {
    environment       = var.environment
    location         = var.location
    deployment_index = var.deployment_index
    random_suffix    = random_string.unique.result
    deployed_at      = timestamp()
  }
}

output "resource_groups" {
  description = "Created resource groups"
  value = {
    compute = {
      id       = azurerm_resource_group.compute.id
      name     = azurerm_resource_group.compute.name
      location = azurerm_resource_group.compute.location
    }
    data = {
      id       = azurerm_resource_group.data.id
      name     = azurerm_resource_group.data.name
      location = azurerm_resource_group.data.location
    }
    shared = {
      id       = azurerm_resource_group.shared.id
      name     = azurerm_resource_group.shared.name
      location = azurerm_resource_group.shared.location
    }
  }
}

#=============================================================================
# NETWORKING OUTPUTS
#=============================================================================

output "networking" {
  description = "Network infrastructure information"
  value = {
    virtual_network = {
      id            = azurerm_virtual_network.main.id
      name          = azurerm_virtual_network.main.name
      address_space = azurerm_virtual_network.main.address_space
    }
    subnets = {
      aks_nodes = {
        id               = azurerm_subnet.aks_nodes.id
        name             = azurerm_subnet.aks_nodes.name
        address_prefixes = azurerm_subnet.aks_nodes.address_prefixes
      }
      aks_api = {
        id               = azurerm_subnet.aks_api.id
        name             = azurerm_subnet.aks_api.name
        address_prefixes = azurerm_subnet.aks_api.address_prefixes
      }
      postgres = {
        id               = azurerm_subnet.postgres.id
        name             = azurerm_subnet.postgres.name
        address_prefixes = azurerm_subnet.postgres.address_prefixes
      }
      private_endpoints = {
        id               = azurerm_subnet.private_endpoints.id
        name             = azurerm_subnet.private_endpoints.name
        address_prefixes = azurerm_subnet.private_endpoints.address_prefixes
      }
    }
    private_dns_zones = {
      container_registry = azurerm_private_dns_zone.container_registry.name
      key_vault         = azurerm_private_dns_zone.key_vault.name
      storage_blob      = azurerm_private_dns_zone.storage_blob.name
      postgres          = azurerm_private_dns_zone.postgres.name
    }
  }
}

#=============================================================================
# SECURITY OUTPUTS
#=============================================================================

output "security" {
  description = "Security infrastructure information"
  value = {
    key_vault = {
      id   = module.key_vault.resource_id
      name = module.key_vault.resource_name
      uri  = "https://${module.key_vault.resource_name}.vault.azure.net/"
    }
    managed_identities = {
      aks_cluster = {
        id           = azurerm_user_assigned_identity.aks_cluster.id
        name         = azurerm_user_assigned_identity.aks_cluster.name
        client_id    = azurerm_user_assigned_identity.aks_cluster.client_id
        principal_id = azurerm_user_assigned_identity.aks_cluster.principal_id
      }
      aks_kubelet = {
        id           = azurerm_user_assigned_identity.aks_kubelet.id
        name         = azurerm_user_assigned_identity.aks_kubelet.name
        client_id    = azurerm_user_assigned_identity.aks_kubelet.client_id
        principal_id = azurerm_user_assigned_identity.aks_kubelet.principal_id
      }
      command_api = {
        id           = azurerm_user_assigned_identity.command_api.id
        name         = azurerm_user_assigned_identity.command_api.name
        client_id    = azurerm_user_assigned_identity.command_api.client_id
        principal_id = azurerm_user_assigned_identity.command_api.principal_id
      }
      query_api = {
        id           = azurerm_user_assigned_identity.query_api.id
        name         = azurerm_user_assigned_identity.query_api.name
        client_id    = azurerm_user_assigned_identity.query_api.client_id
        principal_id = azurerm_user_assigned_identity.query_api.principal_id
      }
      frontend = {
        id           = azurerm_user_assigned_identity.frontend.id
        name         = azurerm_user_assigned_identity.frontend.name
        client_id    = azurerm_user_assigned_identity.frontend.client_id
        principal_id = azurerm_user_assigned_identity.frontend.principal_id
      }
    }
  }
}

#=============================================================================
# DATA LAYER OUTPUTS
#=============================================================================

output "data_layer" {
  description = "Data layer infrastructure information"
  value = {
    postgresql = {
      id               = module.postgres_flexible_server.resource_id
      name             = module.postgres_flexible_server.resource_name
      fqdn             = "${module.postgres_flexible_server.resource_name}.postgres.database.azure.com"
      version          = "16"
      authentication   = "Azure AD"
      databases = {
        events     = azurerm_postgresql_flexible_server_database.events.name
        readmodels = azurerm_postgresql_flexible_server_database.readmodels.name
      }
    }
    storage_account = {
      id                    = module.storage_account.resource_id
      name                  = module.storage_account.resource_name
      primary_blob_endpoint = module.storage_account.primary_blob_endpoint
      primary_file_endpoint = module.storage_account.primary_file_endpoint
      containers           = keys(module.storage_account.storage_containers)
      file_shares          = keys(module.storage_account.storage_file_shares)
      queues               = keys(module.storage_account.storage_queues)
      tables               = keys(module.storage_account.storage_tables)
    }
  }
}

#=============================================================================
# COMPUTE LAYER OUTPUTS
#=============================================================================

output "compute_layer" {
  description = "Compute layer infrastructure information"
  value = {
    aks_cluster = {
      id                    = module.aks_cluster.resource_id
      name                  = module.aks_cluster.resource_name
      fqdn                  = module.aks_cluster.cluster_portal_fqdn
      private_fqdn          = module.aks_cluster.cluster_private_fqdn
      kubernetes_version    = var.aks_config.kubernetes_version
      oidc_issuer_url       = module.aks_cluster.oidc_issuer_url
      node_resource_group   = module.aks_cluster.node_resource_group
    }
    container_registry = {
      id           = module.container_registry.resource_id
      name         = module.container_registry.resource_name
      login_server = module.container_registry.login_server
      sku          = var.container_registry_config.sku
    }
    application_insights = {
      id                 = azurerm_application_insights.main.id
      name              = azurerm_application_insights.main.name
      instrumentation_key = azurerm_application_insights.main.instrumentation_key
      app_id            = azurerm_application_insights.main.app_id
    }
  }
  sensitive = true  # Contains sensitive Application Insights data
}

#=============================================================================
# MONITORING OUTPUTS
#=============================================================================

output "monitoring" {
  description = "Monitoring infrastructure information"
  value = {
    log_analytics_workspace = {
      id           = module.log_analytics_workspace.resource_id
      name         = module.log_analytics_workspace.resource_name
      workspace_id = module.log_analytics_workspace.workspace_id
    }
    application_insights = {
      id   = azurerm_application_insights.main.id
      name = azurerm_application_insights.main.name
    }
  }
}

#=============================================================================
# CONNECTION INFORMATION FOR APPLICATIONS
#=============================================================================

output "application_configuration" {
  description = "Configuration information for BeachBreak applications"
  value = {
    key_vault_uri = "https://${module.key_vault.resource_name}.vault.azure.net/"

    # Service URLs (internal Kubernetes services)
    service_urls = {
      command_api = "http://command-api"  # Internal Kubernetes service
      query_api   = "http://query-api"    # Internal Kubernetes service
      frontend    = "http://frontend"     # Internal Kubernetes service
    }

    # Database information
    databases = {
      events_database     = "events"
      readmodels_database = "readmodels"
      postgres_server     = "${module.postgres_flexible_server.resource_name}.postgres.database.azure.com"
    }

    # Identity configuration
    workload_identities = {
      command_api_client_id = azurerm_user_assigned_identity.command_api.client_id
      query_api_client_id   = azurerm_user_assigned_identity.query_api.client_id
      frontend_client_id    = azurerm_user_assigned_identity.frontend.client_id
    }
  }
  sensitive = true
}

#=============================================================================
# COST ESTIMATION
#=============================================================================

output "cost_estimation" {
  description = "Estimated monthly costs for the development environment"
  value = {
    aks_cluster         = "~150 CHF (2x D2s_v5 nodes)"
    postgresql         = "~45 CHF (B_Standard_B1ms)"
    storage_account    = "~25 CHF (LRS storage)"
    container_registry = "~5 CHF (Basic SKU)"
    key_vault          = "~3 CHF (Standard SKU)"
    log_analytics      = "~30 CHF (PerGB2018, 30-day retention)"
    networking         = "~25 CHF (VNet, private endpoints)"
    total_estimated    = "~283 CHF/month"
    savings_vs_prod    = "~557 CHF/month (66% savings)"
    note              = "Development environment with single-zone deployment and cost optimization"
  }
}

#=============================================================================
# NEXT STEPS
#=============================================================================

output "next_steps" {
  description = "Next steps after infrastructure deployment"
  value = {
    kubernetes_access = [
      "1. Install kubectl and Azure CLI",
      "2. Run: az aks get-credentials --resource-group ${azurerm_resource_group.compute.name} --name ${module.aks_cluster.resource_name}",
      "3. Test access: kubectl get nodes"
    ]
    container_registry = [
      "1. Build and tag images: docker build -t ${module.container_registry.login_server}/beachbreak-commandapi:latest .",
      "2. Login to ACR: az acr login --name ${module.container_registry.resource_name}",
      "3. Push images: docker push ${module.container_registry.login_server}/beachbreak-commandapi:latest"
    ]
    database_setup = [
      "1. Connect using Azure AD authentication",
      "2. Create necessary schemas and tables",
      "3. Test connectivity from AKS pods using managed identities"
    ]
    monitoring = [
      "1. Configure Application Insights in your applications",
      "2. Set up Log Analytics queries and alerts",
      "3. Configure Grafana dashboards (optional)"
    ]
  }
}