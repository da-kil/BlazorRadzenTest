# AKS Cluster Module
resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-${var.project_name}-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "${var.project_name}-${var.environment}"
  kubernetes_version  = var.kubernetes_version

  # System node pool
  default_node_pool {
    name                = "system"
    node_count          = var.system_node_count
    vm_size             = var.system_node_vm_size
    vnet_subnet_id      = var.aks_subnet_id
    type                = "VirtualMachineScaleSets"
    zones               = var.availability_zones
    enable_auto_scaling = true
    min_count           = var.system_node_min_count
    max_count           = var.system_node_max_count
    os_disk_size_gb     = 128
    os_disk_type        = "Managed"

    node_labels = {
      "nodepool-type" = "system"
      "environment"   = var.environment
      "nodepoolos"    = "linux"
    }

    tags = merge(var.tags, {
      "NodePool" = "system"
    })
  }

  # Identity configuration - Using SystemAssigned for AKS
  identity {
    type = "SystemAssigned"
  }

  # Network profile - Azure CNI for better control
  network_profile {
    network_plugin    = "azure"
    network_policy    = "azure"
    dns_service_ip    = var.dns_service_ip
    service_cidr      = var.service_cidr
    load_balancer_sku = "standard"
  }

  # Azure Monitor Container Insights
  oms_agent {
    log_analytics_workspace_id = var.log_analytics_workspace_id
  }

  # Azure Active Directory integration
  azure_active_directory_role_based_access_control {
    managed                = true
    azure_rbac_enabled     = true
    admin_group_object_ids = var.aks_admin_group_object_ids
  }

  # Key Vault integration
  key_vault_secrets_provider {
    secret_rotation_enabled  = true
    secret_rotation_interval = "2m"
  }

  # Auto-scaler profile
  auto_scaler_profile {
    balance_similar_node_groups      = true
    expander                         = "random"
    max_graceful_termination_sec     = 600
    max_node_provisioning_time       = "15m"
    max_unready_nodes                = 3
    max_unready_percentage           = 45
    new_pod_scale_up_delay           = "10s"
    scale_down_delay_after_add       = "10m"
    scale_down_delay_after_delete    = "10s"
    scale_down_delay_after_failure   = "3m"
    scan_interval                    = "10s"
    scale_down_unneeded              = "10m"
    scale_down_unready               = "20m"
    scale_down_utilization_threshold = "0.5"
  }

  # Maintenance window
  maintenance_window {
    allowed {
      day   = "Saturday"
      hours = [2, 3, 4]
    }
    allowed {
      day   = "Sunday"
      hours = [2, 3, 4]
    }
  }

  # Security settings
  local_account_disabled = false  # Set to true in production and use Azure AD only
  role_based_access_control_enabled = true

  tags = var.tags

  lifecycle {
    ignore_changes = [
      default_node_pool[0].node_count,
      kubernetes_version
    ]
  }
}

# Application Node Pool (for APIs)
resource "azurerm_kubernetes_cluster_node_pool" "application" {
  name                  = "app"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  vm_size               = var.app_node_vm_size
  node_count            = var.app_node_count
  vnet_subnet_id        = var.aks_subnet_id
  zones                 = var.availability_zones
  enable_auto_scaling   = true
  min_count             = var.app_node_min_count
  max_count             = var.app_node_max_count
  os_disk_size_gb       = 128
  os_disk_type          = "Managed"
  mode                  = "User"

  node_labels = {
    "nodepool-type" = "application"
    "environment"   = var.environment
    "workload"      = "api"
  }

  node_taints = []

  tags = merge(var.tags, {
    "NodePool" = "application"
  })
}

# Frontend Node Pool (for Blazor frontend)
resource "azurerm_kubernetes_cluster_node_pool" "frontend" {
  name                  = "frontend"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  vm_size               = var.frontend_node_vm_size
  node_count            = var.frontend_node_count
  vnet_subnet_id        = var.aks_subnet_id
  zones                 = var.availability_zones
  enable_auto_scaling   = true
  min_count             = var.frontend_node_min_count
  max_count             = var.frontend_node_max_count
  os_disk_size_gb       = 128
  os_disk_type          = "Managed"
  mode                  = "User"

  node_labels = {
    "nodepool-type" = "frontend"
    "environment"   = var.environment
    "workload"      = "web"
  }

  node_taints = []

  tags = merge(var.tags, {
    "NodePool" = "frontend"
  })
}

# Role assignment for AKS to pull images from ACR
resource "azurerm_role_assignment" "aks_acr" {
  count                = var.acr_id != null ? 1 : 0
  principal_id         = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
  role_definition_name = "AcrPull"
  scope                = var.acr_id
  skip_service_principal_aad_check = true
}

# Role assignment for AKS to access VNet
resource "azurerm_role_assignment" "aks_network" {
  principal_id         = azurerm_kubernetes_cluster.main.identity[0].principal_id
  role_definition_name = "Network Contributor"
  scope                = var.vnet_id
  skip_service_principal_aad_check = true
}

# Diagnostic settings for AKS
resource "azurerm_monitor_diagnostic_setting" "aks" {
  name                       = "aks-diagnostics"
  target_resource_id         = azurerm_kubernetes_cluster.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "kube-apiserver"
  }

  enabled_log {
    category = "kube-controller-manager"
  }

  enabled_log {
    category = "kube-scheduler"
  }

  enabled_log {
    category = "kube-audit"
  }

  enabled_log {
    category = "cluster-autoscaler"
  }

  metric {
    category = "AllMetrics"
    enabled  = true
  }
}
