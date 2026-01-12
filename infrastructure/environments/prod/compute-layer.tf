# Compute layer configuration for ti8m BeachBreak development environment
# Creates Container Registry and AKS cluster

#=============================================================================
# CONTAINER REGISTRY MODULE
#=============================================================================

module "container_registry" {
  source = "../../modules/container_registry"

  env                 = var.environment
  deployment_index    = var.deployment_index
  component           = local.component_name
  location            = var.location
  resource_group_name = azurerm_resource_group.compute.name
  resource_name_infix = local.random_suffix

  # Registry configuration
  sku                         = var.container_registry_config.sku
  admin_enabled              = false  # Use managed identities instead
  geo_replication_enabled    = var.container_registry_config.geo_replication_enabled

  # Network configuration - private endpoint only
  network_access_mode = "OnlyPrivateEndpoints"
  network_bypass      = ["AzureServices"]
  network_ips_to_whitelist = var.allowed_ip_ranges

  # Private endpoint configuration
  private_endpoint_subnet_id           = azurerm_subnet.private_endpoints.id
  central_dns_zone_resource_group_name = azurerm_resource_group.shared.name

  # Registry cache rules for Microsoft Container Registry
  cache_rules = {
    mcr = {
      name                   = "mcr"
      source_registry        = "mcr.microsoft.com"
      target_registry        = "mcr"
      credential_set_resource_id = null
    }
  }

  # Trust policy
  trust_policy_enabled = true

  # Retention policy (dev environment - shorter retention)
  retention_policy = {
    enabled = true
    days    = 7
  }

  # Monitoring
  diagnostics = {
    log_analytics_workspace_id = module.log_analytics_workspace.resource_id
    log_category_groups        = ["allLogs"]
    log_metrics               = ["AllMetrics"]
  }

  tags = local.common_tags

  depends_on = [
    azurerm_private_dns_zone.container_registry,
    azurerm_private_dns_zone_virtual_network_link.container_registry
  ]
}

#=============================================================================
# AKS CLUSTER MODULE
#=============================================================================

module "aks_cluster" {
  source = "../../modules/azure_kubernetes_cluster"

  env                 = var.environment
  deployment_index    = var.deployment_index
  component           = local.component_name
  location            = var.location
  resource_group_name = azurerm_resource_group.compute.name
  resource_name_infix = local.random_suffix

  # Cluster configuration
  k8s_version              = var.aks_config.kubernetes_version
  automatic_upgrade_channel = "stable"
  sku_tier                 = var.aks_config.sku_tier

  # Admin access configuration
  admin_group_object_ids = var.admin_group_object_ids

  # Private cluster configuration
  private_cluster_enabled      = true
  private_cluster_outbound_type = "loadBalancer"
  api_server_subnet_id        = azurerm_subnet.aks_api.id
  cluster_virtual_network_id  = azurerm_virtual_network.main.id
  cluster_dns_zone_id         = azurerm_private_dns_zone.aks_private.id

  # Identity configuration
  cluster_identity = {
    type                 = "UserAssigned"
    identity_ids         = [azurerm_user_assigned_identity.aks_cluster.id]
    client_id           = azurerm_user_assigned_identity.aks_cluster.client_id
    principal_id        = azurerm_user_assigned_identity.aks_cluster.principal_id
  }

  kubelet_identity = {
    type                 = "UserAssigned"
    identity_ids         = [azurerm_user_assigned_identity.aks_kubelet.id]
    client_id           = azurerm_user_assigned_identity.aks_kubelet.client_id
    principal_id        = azurerm_user_assigned_identity.aks_kubelet.principal_id
  }

  # Default node pool configuration
  default_node_pool = {
    name                         = "system"
    nodePoolSubnetId            = azurerm_subnet.aks_nodes.id
    vmSize                      = var.aks_config.default_node_pool.vm_size
    nodeCount                   = var.aks_config.default_node_pool.node_count
    minNodeCount               = var.aks_config.default_node_pool.min_count
    maxNodeCount               = var.aks_config.default_node_pool.max_count
    availabilityZones          = var.aks_config.default_node_pool.availability_zones
    enableAutoScaling          = true
    node_labels = {
      "role" = "system"
    }
    node_taints = ["CriticalAddonsOnly=true:NoSchedule"]
  }

  # Additional node pools for application workloads
  node_pools = {
    for name, config in var.aks_config.additional_node_pools : name => {
      nodePoolSubnetId   = azurerm_subnet.aks_nodes.id
      vmSize            = config.vm_size
      nodeCount         = config.node_count
      minNodeCount      = config.min_count
      maxNodeCount      = config.max_count
      availabilityZones = config.availability_zones
      enableAutoScaling = true
      node_labels = {
        "role" = "workload"
      }
    }
  }

  # Security configuration
  etcd_encryption_key_vault_id = module.key_vault.resource_id
  etcd_encryption_key_id      = azurerm_key_vault_key.etcd_encryption.id

  # Container registry integration
  bootstrap_container_registry_id = module.container_registry.resource_id

  # Auto-scaler profile (cost-optimized for development)
  auto_scaler_profile = {
    balance_similar_node_groups      = false
    expander                        = "random"
    max_graceful_termination_sec    = "600"
    max_node_provisioning_time      = "15m"
    max_unready_nodes              = 3
    max_unready_percentage         = 45
    new_pod_scale_up_delay         = "10s"
    scale_down_delay_after_add     = "10m"
    scale_down_delay_after_delete  = "10s"
    scale_down_delay_after_failure = "3m"
    scale_down_unneeded            = "10m"
    scale_down_unready             = "20m"
    scale_down_utilization_threshold = "0.5"
    scan_interval                  = "10s"
    skip_nodes_with_local_storage  = false
    skip_nodes_with_system_pods    = true
  }

  # Storage profile
  storage_profile = {
    blob_driver_enabled         = true
    disk_driver_enabled        = true
    disk_driver_version        = "v1"
    file_driver_enabled        = true
    snapshot_controller_enabled = true
  }

  # Workload autoscaler profile
  workload_autoscaler_profile = {
    keda_enabled                    = true
    vertical_pod_autoscaler_enabled = false  # Disabled for dev environment
  }

  # Azure integrations
  azure_policy_enabled      = true
  cost_analysis_enabled     = true
  oms_agent_enabled         = true
  workload_identity_enabled = true
  oidc_issuer_enabled       = true

  # Networking configuration
  network_profile = {
    network_plugin    = "azure"
    network_policy    = "azure"
    service_cidr      = "10.10.0.0/16"
    dns_service_ip    = "10.10.0.10"
    outbound_type     = "loadBalancer"
    load_balancer_sku = "standard"
  }

  # Monitoring
  diagnostics = {
    log_analytics_workspace_id = module.log_analytics_workspace.resource_id
    log_category_groups        = ["allLogs"]
    log_metrics               = ["AllMetrics"]
  }

  tags = local.common_tags

  depends_on = [
    module.container_registry,
    azurerm_key_vault_key.etcd_encryption,
    azurerm_role_assignment.aks_cluster_to_keyvault_crypto
  ]
}

#=============================================================================
# AKS PRIVATE DNS ZONE
#=============================================================================

resource "azurerm_private_dns_zone" "aks_private" {
  name                = "${local.naming.aks_cluster}.privatelink.${var.location}.azmk8s.io"
  resource_group_name = azurerm_resource_group.shared.name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "aks_private" {
  name                  = "${azurerm_virtual_network.main.name}-link"
  resource_group_name   = azurerm_resource_group.shared.name
  private_dns_zone_name = azurerm_private_dns_zone.aks_private.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false

  tags = local.common_tags
}

#=============================================================================
# RBAC ASSIGNMENTS
#=============================================================================

# AKS Kubelet to Container Registry - AcrPull
resource "azurerm_role_assignment" "aks_kubelet_to_acr" {
  scope                = module.container_registry.resource_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.aks_kubelet.principal_id
}

# AKS cluster to virtual network - Network Contributor (for load balancer)
resource "azurerm_role_assignment" "aks_cluster_to_vnet" {
  scope                = azurerm_virtual_network.main.id
  role_definition_name = "Network Contributor"
  principal_id         = azurerm_user_assigned_identity.aks_cluster.principal_id
}

#=============================================================================
# APPLICATION INSIGHTS
#=============================================================================

resource "azurerm_application_insights" "main" {
  name                = "appi-${local.component_name}-${var.environment}-${local.location_short}-${var.deployment_index}"
  location            = var.location
  resource_group_name = azurerm_resource_group.compute.name
  workspace_id        = module.log_analytics_workspace.resource_id
  application_type    = var.application_insights_config.application_type
  retention_in_days   = var.application_insights_config.retention_in_days

  tags = local.common_tags
}

# Update Application Insights connection string in Key Vault
resource "azurerm_key_vault_secret" "app_insights_connection_string_update" {
  name         = "ApplicationInsights--ConnectionString"
  value        = azurerm_application_insights.main.connection_string
  key_vault_id = module.key_vault.resource_id

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.current_user_keyvault_admin
  ]
}

#=============================================================================
# OUTPUTS
#=============================================================================

output "container_registry" {
  description = "Container Registry information"
  value = {
    id           = module.container_registry.resource_id
    name         = module.container_registry.resource_name
    login_server = module.container_registry.login_server
    admin_enabled = false
  }
}

output "aks_cluster" {
  description = "AKS cluster information"
  value = {
    id                    = module.aks_cluster.resource_id
    name                  = module.aks_cluster.resource_name
    fqdn                  = module.aks_cluster.cluster_portal_fqdn
    private_fqdn          = module.aks_cluster.cluster_private_fqdn
    oidc_issuer_url       = module.aks_cluster.oidc_issuer_url
    kubelet_identity      = module.aks_cluster.kubelet_identity
    node_resource_group   = module.aks_cluster.node_resource_group
  }
}

output "application_insights" {
  description = "Application Insights information"
  value = {
    id                 = azurerm_application_insights.main.id
    name              = azurerm_application_insights.main.name
    instrumentation_key = azurerm_application_insights.main.instrumentation_key
    connection_string  = azurerm_application_insights.main.connection_string
    app_id            = azurerm_application_insights.main.app_id
  }
  sensitive = true
}