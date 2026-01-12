# Local variables
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

# AKS Cluster
resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-bb-${var.env}-${local.loc}-01"
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "aks-bb-${var.env}-${local.loc}"
  kubernetes_version  = var.kubernetes_version

  # Private cluster configuration
  private_cluster_enabled = true

  # API Server VNet Integration
  api_server_access_profile {
    vnet_integration_enabled = true
    subnet_id                = var.aks_api_server_subnet_id
  }

  # Default system node pool
  default_node_pool {
    name                = "system"
    node_count          = var.default_node_pool_node_count
    vm_size             = var.default_node_pool_vm_size
    vnet_subnet_id      = var.aks_node_pool_subnet_id
    type                = "VirtualMachineScaleSets"
    os_disk_type        = "Managed"
    os_disk_size_gb     = 128
    max_pods            = 110
    enable_auto_scaling = false

    upgrade_settings {
      max_surge = "10%"
    }
  }

  # Cluster identity
  identity {
    type         = "UserAssigned"
    identity_ids = [var.cluster_identity_id]
  }

  # Kubelet identity
  kubelet_identity {
    client_id                 = var.kubelet_identity_client_id
    object_id                 = var.kubelet_identity_principal_id
    user_assigned_identity_id = var.kubelet_identity_id
  }

  # Network profile
  network_profile {
    network_plugin      = "azure"
    network_policy      = "azure"
    service_cidr        = "10.1.0.0/16"
    dns_service_ip      = "10.1.0.10"
    load_balancer_sku   = "standard"
    outbound_type       = "loadBalancer"
  }

  # Azure AD integration with RBAC
  azure_active_directory_role_based_access_control {
    managed                = true
    azure_rbac_enabled     = true
  }

  # Workload Identity (OIDC)
  oidc_issuer_enabled       = true
  workload_identity_enabled = true

  # Key Vault Secrets Provider (for CSI driver)
  key_vault_secrets_provider {
    secret_rotation_enabled  = true
    secret_rotation_interval = "2m"
  }

  # Maintenance window
  maintenance_window {
    allowed {
      day   = "Sunday"
      hours = [2, 3, 4]
    }
  }

  # Auto-upgrade channel
  automatic_channel_upgrade = "patch"

  # Azure Monitor integration
  monitor_metrics {
  }

  tags = local.common_tags
}

# User node pool (auto-scaling)
resource "azurerm_kubernetes_cluster_node_pool" "user" {
  name                  = "user"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  vm_size               = var.user_node_pool_vm_size
  vnet_subnet_id        = var.aks_node_pool_subnet_id

  # Auto-scaling configuration
  enable_auto_scaling = true
  min_count           = var.user_node_pool_min_count
  max_count           = var.user_node_pool_max_count

  os_disk_type    = "Managed"
  os_disk_size_gb = 128
  max_pods        = 110
  mode            = "User"

  upgrade_settings {
    max_surge = "33%"
  }

  tags = local.common_tags
}

# Diagnostic Settings for AKS
resource "azurerm_monitor_diagnostic_setting" "aks" {
  count                      = var.diagnostics_workspace_id != "" ? 1 : 0
  name                       = "diag-${azurerm_kubernetes_cluster.main.name}"
  target_resource_id         = azurerm_kubernetes_cluster.main.id
  log_analytics_workspace_id = var.diagnostics_workspace_id

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

# Role Assignment: Kubelet identity needs Key Vault Crypto User if etcd encryption is enabled
resource "azurerm_role_assignment" "kubelet_kv_crypto" {
  count                = var.etcd_encryption_key_id != "" ? 1 : 0
  scope                = var.etcd_encryption_key_id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = var.cluster_identity_principal_id
}
