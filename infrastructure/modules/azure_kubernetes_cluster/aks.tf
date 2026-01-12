# Kubernetes Cluster creation - azurerm code saved for future use
resource "azurerm_kubernetes_cluster" "this" {
  name                = local.resource_name
  resource_group_name = var.resource_group_name
  node_resource_group = "${var.resource_group_name}-node"
  location            = var.location
  tags                = local.merged_tags

  dns_prefix              = local.resource_name
  private_cluster_enabled = var.private_cluster_enabled

  kubernetes_version        = var.k8s_version
  automatic_upgrade_channel = var.automatic_upgrade_channel != "none" ? var.automatic_upgrade_channel : null
  sku_tier                  = var.sku_tier
  support_plan              = var.support_plan
  cost_analysis_enabled     = true

  oms_agent {
    log_analytics_workspace_id      = var.diagnostics.log_analytics_workspace_id
    msi_auth_for_monitoring_enabled = true
  }

  dynamic "monitor_metrics" {
    for_each = var.monitor_metrics != null ? [1] : []
    content {
      annotations_allowed = var.monitor_metrics.annotationsAllowed
      labels_allowed      = var.monitor_metrics.labelsAllowed
    }
  }

  image_cleaner_enabled        = true
  image_cleaner_interval_hours = 24
  disk_encryption_set_id       = var.disk_encryption_set_id

  # key_management_service {
  #   key_vault_key_id         = var.etcd_encryption_key_id
  #   key_vault_network_access = "Private"
  # }

  key_vault_secrets_provider {
    secret_rotation_enabled  = true
    secret_rotation_interval = "10m"
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [var.cluster_identity.resourceId]
  }

  kubelet_identity {
    client_id                 = var.kubelet_identity.clientId
    object_id                 = var.kubelet_identity.objectId
    user_assigned_identity_id = var.kubelet_identity.resourceId
  }

  azure_policy_enabled              = true
  role_based_access_control_enabled = true
  local_account_disabled            = true
  oidc_issuer_enabled               = true
  workload_identity_enabled         = true

  azure_active_directory_role_based_access_control {
    admin_group_object_ids = var.admin_group_object_ids
    azure_rbac_enabled     = true
  }

  dynamic "confidential_computing" {
    for_each = var.sgx_quote_helper_enabled == true ? [1] : []
    content {
      sgx_quote_helper_enabled = var.sgx_quote_helper_enabled
    }
  }

  dynamic "auto_scaler_profile" {
    for_each = var.auto_scaler_profile != null ? [1] : []
    content {
      balance_similar_node_groups                   = var.auto_scaler_profile.balanceSimilarNodeGroups
      daemonset_eviction_for_empty_nodes_enabled    = var.auto_scaler_profile.daemonsetEvictionForEmptyNodesEnabled
      daemonset_eviction_for_occupied_nodes_enabled = var.auto_scaler_profile.daemonsetEvictionForOccupiedNodesEnabled
      expander                                      = var.auto_scaler_profile.expander
      ignore_daemonsets_utilization_enabled         = var.auto_scaler_profile.ignoreDaemonsetsUtilizationEnabled
      max_graceful_termination_sec                  = var.auto_scaler_profile.maxGracefulTerminationSec
      max_node_provisioning_time                    = var.auto_scaler_profile.maxNodeProvisioningTime
      max_unready_nodes                             = var.auto_scaler_profile.maxUnreadyNodes
      max_unready_percentage                        = var.auto_scaler_profile.maxUnreadyPercentage
      new_pod_scale_up_delay                        = var.auto_scaler_profile.newPodScaleUpDelay
      scale_down_delay_after_add                    = var.auto_scaler_profile.scaleDownDelayAfterAdd
      scale_down_delay_after_delete                 = var.auto_scaler_profile.scaleDownDelayAfterDelete
      scale_down_delay_after_failure                = var.auto_scaler_profile.scaleDownDelayAfterFailure
      scan_interval                                 = var.auto_scaler_profile.scanInterval
      scale_down_unneeded                           = var.auto_scaler_profile.scaleDownUnneededTime
      scale_down_unready                            = var.auto_scaler_profile.scaleDownUnreadyTime
      scale_down_utilization_threshold              = var.auto_scaler_profile.scaleDownUtilizationThreshold
      empty_bulk_delete_max                         = var.auto_scaler_profile.emptyBulkDeleteMax
      skip_nodes_with_local_storage                 = var.auto_scaler_profile.skipNodesWithLocalStorage
      skip_nodes_with_system_pods                   = var.auto_scaler_profile.skipNodesWithSystemPods
    }
  }

  dynamic "workload_autoscaler_profile" {
    for_each = var.workload_autoscaler_profile != null ? [1] : []
    content {
      keda_enabled                    = var.workload_autoscaler_profile.kedaEnabled
      vertical_pod_autoscaler_enabled = var.workload_autoscaler_profile.verticalPodAutoscalerEnabled
    }
  }

  maintenance_window {
    allowed {
      day   = var.hardware_maintenance_window.allowed.day
      hours = var.hardware_maintenance_window.allowed.hours
    }
    dynamic "not_allowed" {
      for_each = var.hardware_maintenance_window.notAllowed != null ? [1] : []
      content {
        start = var.hardware_maintenance_window.notAllowed.start
        end   = var.hardware_maintenance_window.notAllowed.end
      }
    }
  }

  dynamic "maintenance_window_auto_upgrade" {
    for_each = var.auto_upgrade_maintenance_window != null ? [1] : []
    content {
      frequency    = var.auto_upgrade_maintenance_window.frequency
      interval     = var.auto_upgrade_maintenance_window.interval
      duration     = var.auto_upgrade_maintenance_window.duration
      day_of_week  = var.auto_upgrade_maintenance_window.dayOfWeek
      day_of_month = var.auto_upgrade_maintenance_window.dayOfMonth
      week_index   = var.auto_upgrade_maintenance_window.weekIndex
      start_time   = var.auto_upgrade_maintenance_window.startTime
      utc_offset   = var.auto_upgrade_maintenance_window.utcOffset
      start_date   = var.auto_upgrade_maintenance_window.startDate

      dynamic "not_allowed" {
        for_each = var.auto_upgrade_maintenance_window.notAllowed != null ? [1] : []
        content {
          start = var.auto_upgrade_maintenance_window.notAllowed.start
          end   = var.auto_upgrade_maintenance_window.notAllowed.end
        }
      }
    }
  }

  node_os_upgrade_channel = var.node_os_upgrade_channel

  dynamic "maintenance_window_node_os" {
    for_each = var.node_os_upgrade_maintenance_window != null ? [1] : []
    content {
      frequency    = var.node_os_upgrade_maintenance_window.frequency
      interval     = var.node_os_upgrade_maintenance_window.interval
      duration     = var.node_os_upgrade_maintenance_window.duration
      day_of_week  = var.node_os_upgrade_maintenance_window.dayOfWeek
      day_of_month = var.node_os_upgrade_maintenance_window.dayOfMonth
      week_index   = var.node_os_upgrade_maintenance_window.weekIndex
      start_time   = var.node_os_upgrade_maintenance_window.startTime
      utc_offset   = var.node_os_upgrade_maintenance_window.utcOffset
      start_date   = var.node_os_upgrade_maintenance_window.startDate

      dynamic "not_allowed" {
        for_each = var.node_os_upgrade_maintenance_window.notAllowed != null ? [1] : []
        content {
          start = var.node_os_upgrade_maintenance_window.notAllowed.start
          end   = var.node_os_upgrade_maintenance_window.notAllowed.end
        }
      }
    }
  }

  dynamic "http_proxy_config" {
    for_each = var.http_proxy_config != null ? [1] : []
    content {
      http_proxy  = var.http_proxy_config.httpProxy
      https_proxy = var.http_proxy_config.httpsProxy
      no_proxy    = local.combinedNoProxy
      trusted_ca  = local.customCaCerts
    }
  }

  storage_profile {
    blob_driver_enabled         = var.storage_profile.blobDriverEnabled
    disk_driver_enabled         = var.storage_profile.diskDriverEnabled
    file_driver_enabled         = var.storage_profile.fileDriverEnabled
    snapshot_controller_enabled = var.storage_profile.snapshotControllerEnabled
  }

  private_cluster_public_fqdn_enabled = var.private_cluster_enabled
  private_dns_zone_id                 = var.cluster_dns_zone_id

  network_profile {
    network_plugin      = "azure"
    network_plugin_mode = "overlay"
    outbound_type       = var.private_cluster_outbound_type
    pod_cidr            = "10.248.0.0/16"
    service_cidr        = "10.249.0.0/16"
    dns_service_ip      = "10.249.254.254"
    network_data_plane  = "cilium"
    ip_versions         = ["IPv4"]
    load_balancer_sku   = "standard"
  }

  bootstrap_profile {
    artifact_source       = "Cache"
    container_registry_id = var.bootstrap_container_registry_id
  }

  run_command_enabled = var.run_command_enabled

  default_node_pool {
    name                          = var.default_node_pool.name
    min_count                     = var.default_node_pool.minNodeCount
    max_count                     = var.default_node_pool.maxNodeCount
    auto_scaling_enabled          = var.default_node_pool.enableAutoScaling
    max_pods                      = var.default_node_pool.maxPods
    orchestrator_version          = var.default_node_pool.orchestratorVersion
    vnet_subnet_id                = var.default_node_pool.nodePoolSubnetId
    node_public_ip_enabled        = false
    host_encryption_enabled       = true
    type                          = "VirtualMachineScaleSets"
    os_sku                        = "Ubuntu"
    os_disk_size_gb               = var.default_node_pool.osDiskSizeGB
    os_disk_type                  = var.default_node_pool.osDiskType
    ultra_ssd_enabled             = var.default_node_pool.enableUltraSSD
    vm_size                       = var.default_node_pool.vmSize
    zones                         = var.default_node_pool.availabilityZones
    node_labels                   = var.default_node_pool.nodeLabels
    only_critical_addons_enabled  = var.default_node_pool.onlyCriticalAddonsEnabled
    tags                          = merge(local.merged_tags, try(var.default_node_pool.tags, null))
    capacity_reservation_group_id = var.default_node_pool.capacityReservationGroupId
    kubelet_disk_type             = var.default_node_pool.kubeletDiskType

    dynamic "kubelet_config" {
      for_each = var.default_node_pool.kubeletConfig != null ? [1] : []
      content {
        allowed_unsafe_sysctls    = var.default_node_pool.kubeletConfig.allowedUnsafeSysctls
        container_log_max_line    = var.default_node_pool.kubeletConfig.containerLogMaxLine
        container_log_max_size_mb = var.default_node_pool.kubeletConfig.containerLogMaxSizeMb
        cpu_cfs_quota_enabled     = var.default_node_pool.kubeletConfig.cpuCfsQuotaEnabled
        cpu_cfs_quota_period      = var.default_node_pool.kubeletConfig.cpuCfsQuotaPeriod
        cpu_manager_policy        = var.default_node_pool.kubeletConfig.cpuManagerPolicy
        image_gc_high_threshold   = var.default_node_pool.kubeletConfig.imageGcHighThreshold
        image_gc_low_threshold    = var.default_node_pool.kubeletConfig.imageGcLowThreshold
        pod_max_pid               = var.default_node_pool.kubeletConfig.podMaxPid
        topology_manager_policy   = var.default_node_pool.kubeletConfig.topologyManagerPolicy
      }
    }
  }

  dynamic "aci_connector_linux" {
    for_each = var.aci_node_subnet != null ? [1] : []
    content {
      subnet_name = var.aci_node_subnet
    }
  }

  lifecycle {
    ignore_changes = [microsoft_defender, key_management_service, api_server_access_profile, default_node_pool[0].upgrade_settings]
  }
}
