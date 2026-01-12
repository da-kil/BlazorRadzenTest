resource "azurerm_kubernetes_cluster_node_pool" "pool" {
  for_each = var.additional_node_pools

  depends_on = [azurerm_kubernetes_cluster.this]
  lifecycle {
    ignore_changes = [upgrade_settings]
  }

  name                    = each.key
  tags                    = merge(local.merged_tags, each.value.tags)
  kubernetes_cluster_id   = azurerm_kubernetes_cluster.this.id
  orchestrator_version    = each.value.orchestratorVersion
  mode                    = each.value.mode
  node_public_ip_enabled  = false
  vnet_subnet_id          = each.value.nodePoolSubnetId
  vm_size                 = each.value.vmSize
  os_disk_size_gb         = each.value.osDiskSizeGB
  os_disk_type            = each.value.osDiskType
  os_sku                  = "Ubuntu"
  os_type                 = "Linux"
  ultra_ssd_enabled       = var.default_node_pool.enableUltraSSD
  zones                   = each.value.availabilityZones
  auto_scaling_enabled    = each.value.enableAutoScaling
  scale_down_mode         = each.value.scaleDownMode
  min_count               = each.value.minNodeCount
  max_count               = each.value.maxNodeCount
  max_pods                = each.value.maxPods
  node_taints             = each.value.nodeTaints
  node_labels             = each.value.nodeLabels
  host_encryption_enabled = true

  capacity_reservation_group_id = each.value.capacityReservationGroupId

  kubelet_disk_type = each.value.kubeletDiskType
  dynamic "kubelet_config" {
    for_each = var.default_node_pool.kubeletConfig != null ? [1] : []
    content {
      allowed_unsafe_sysctls    = each.value.kubeletConfig.allowedUnsafeSysctls
      container_log_max_line    = each.value.kubeletConfig.containerLogMaxLine
      container_log_max_size_mb = each.value.kubeletConfig.containerLogMaxSizeMb
      cpu_cfs_quota_enabled     = each.value.kubeletConfig.cpuCfsQuotaEnabled
      cpu_cfs_quota_period      = each.value.kubeletConfig.cpuCfsQuotaPeriod
      cpu_manager_policy        = each.value.kubeletConfig.cpuManagerPolicy
      image_gc_high_threshold   = each.value.kubeletConfig.imageGcHighThreshold
      image_gc_low_threshold    = each.value.kubeletConfig.imageGcLowThreshold
      pod_max_pid               = each.value.kubeletConfig.podMaxPid
      topology_manager_policy   = each.value.kubeletConfig.topologyManagerPolicy
    }
  }
}
