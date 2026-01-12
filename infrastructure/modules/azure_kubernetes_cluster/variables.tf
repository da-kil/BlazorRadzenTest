#
# General variables for the AKS Blueprint
#
variable "env" {
  type        = string
  description = "The deployment environment name"
}

variable "deployment_index" {
  type = string
}

variable "component" {
  type = string

  validation {
    condition     = length(var.component) <= 4
    error_message = "Component can not be longer than 4 characters"
  }
}

variable "location" {
  type    = string
  default = "switzerlandnorth"
}

variable "resource_group_name" {
  type = string
}

variable "random_name_infix" {
  type        = string
  description = "Random resource name infix."

  validation {
    condition     = length(var.random_name_infix) <= 3
    error_message = "Random resource name infix can not be longer than 3 characters"
  }
}

variable "tags" {
  type    = map(string)
  default = {}
}

variable "diagnostics" {
  description = <<DESCRIPTION
Settings required to configure the log analytics integration. Specify the log_analytics_workspace_id property for the target Log Analytics Workspace.
To enable specific log category groups or categories, check the azure docs for specifics: https://learn.microsoft.com/en-us/azure/azure-monitor/platform/diagnostic-settings?WT.mc_id=Portal-Microsoft_Azure_Monitoring&tabs=portal#category-groups
DESCRIPTION
  type = object({
    log_analytics_workspace_id = string
    log_metrics                = optional(list(string), [])
    log_category_groups        = optional(list(string), ["allLogs"])
    log_categories             = optional(list(string), [])
  })
  default = null
}

variable "prox_url" {
  description = "URI of the proxy CA cert"
  default     = null
  type        = string
}

variable "private_cluster_enabled" {
  description = "If true cluster needs to be configured with api server subnet"
  default     = true
  type        = bool
}

variable "private_cluster_outbound_type" {
  description = "Whether the Kubernetes Cluster is allowed to have outbound connections or not. If set to `none`, the Cluster needs to have a private container registry with cache rules enabled"
  default     = "loadBalancer"
  type        = string
}

# Kubernetes Service variables
variable "k8s_version" {
  description = "Version of Kubernetes specified when creating the AKS managed cluster. If not specified, the latest recommended version will be used at provisioning time (but won't auto-upgrade). AKS does not require an exact patch version to be specified, minor version aliases such as 1.22 are also supported. - The minor version's latest GA patch is automatically chosen in that case."
  type        = string
  default     = null
}

variable "automatic_upgrade_channel" {
  description = "The upgrade channel for this Kubernetes Cluster. Possible values are patch, rapid, node-image and stable. Omitting this field sets this value to none."
  type        = string
  default     = null
}

variable "sku_tier" {
  description = "The SKU Tier that should be used for this Kubernetes Cluster. Possible values are Standard (which includes the Uptime SLA) or Premium."
  type        = string
  default     = "Standard"

  validation {
    condition     = var.sku_tier == "Standard" || var.sku_tier == "Premium"
    error_message = "Only Standard or Premium SKU is allowed"
  }
}

variable "support_plan" {
  description = "Specifies the support plan which should be used for this Kubernetes Cluster. Possible values are KubernetesOfficial and AKSLongTermSupport. Defaults to KubernetesOfficial."
  type        = string
  default     = "KubernetesOfficial"

  validation {
    condition     = var.support_plan == "KubernetesOfficial" || var.support_plan == "AKSLongTermSupport"
    error_message = "Only KubernetesOfficial or AKSLongTermSupport Support Plans allowed"
  }
}

variable "cluster_identity" {
  description = "Contains all ids for the managed identity used for the Cluster."
  type = object({
    clientId   = string
    objectId   = string
    resourceId = string
  })
}

variable "kubelet_identity" {
  description = "Contains all ids for the managed identity used for the Kubelet."
  type = object({
    clientId   = string
    objectId   = string
    resourceId = string
  })
}

variable "aci_node_subnet" {
  description = "The subnet name for the virtual nodes to run."
  type        = string
  default     = null
}

variable "admin_group_object_ids" {
  description = "A list of Object IDs of Entra ID Groups which should have Admin Role on the Cluster."
  type        = list(string)
  default     = []
}

variable "sgx_quote_helper_enabled" {
  description = "Should the SGX quote helper be enabled?"
  type        = bool
  default     = false
}

# variable "managed_ingress_dns_zone_ids" {
#   description = "List of DNS Zones used for Managed Ingress Controller"
#   type        = list(string)
#   default     = []
# }
#
# variable "managed_ingress_enabled" {
#   description = "Enables the Managed Ingress Controller. Please note that this requires disabled SSL Interception for management.azure.com for the Node Subnet"
#   type        = bool
#   default     = false
# }

variable "auto_scaler_profile" {
  description = "Defines the auto scaler profile"
  type = object({
    balanceSimilarNodeGroups                 = optional(string, "false")
    daemonsetEvictionForEmptyNodesEnabled    = optional(bool, false)
    daemonsetEvictionForOccupiedNodesEnabled = optional(bool, true)
    expander                                 = optional(string, "random")
    ignoreDaemonsetsUtilizationEnabled       = optional(bool, false)
    maxGracefulTerminationSec                = optional(string, 600)
    maxNodeProvisioningTime                  = optional(string, "15m")
    maxUnreadyNodes                          = optional(string, 3)
    maxUnreadyPercentage                     = optional(string, 4)
    newPodScaleUpDelay                       = optional(string, "0s")
    scaleDownDelayAfterAdd                   = optional(string, "10m")
    scaleDownDelayAfterDelete                = optional(string, "10s")
    scaleDownDelayAfterFailure               = optional(string, "3m")
    scanInterval                             = optional(string, "10s")
    scaleDownUnneededTime                    = optional(string, "10m")
    scaleDownUnreadyTime                     = optional(string, "20m")
    scaleDownUtilizationThreshold            = optional(string, 0.5)
    emptyBulkDeleteMax                       = optional(string, 10)
    skipNodesWithLocalStorage                = optional(string, false)
    skipNodesWithSystemPods                  = optional(string, true)
  })
  default = null
}

variable "workload_autoscaler_profile" {
  description = "Configures Keda and Vertical Pod Autoscaler"
  type = object({
    kedaEnabled                  = bool
    verticalPodAutoscalerEnabled = bool
  })
  default = null
}

variable "monitor_metrics" {
  description = "Configures the allowed monitor metrics"
  type = object({
    annotationsAllowed = list(string)
    labelsAllowed      = list(string)
  })
  default = null
}

variable "etcd_encryption_key_id" {
  description = "Id of the Key Vault Key used for etcd encryption"
  type        = string
  default     = null
}

variable "etcd_encryption_key_vault_id" {
  description = "Id of the Key Vault containing the key used for etcd encryption"
  type        = string
  default     = null
}

variable "hardware_maintenance_window" {
  description = "Configures the hardware maintenance window for the whole Kubernetes Cluster and its resources."
  type = object({
    allowed = object({
      day   = string
      hours = list(number)
    })
    notAllowed = optional(object({
      start = string
      end   = string
    }))
  })
  default = {
    allowed = {
      day   = "Saturday"
      hours = [1, 2]
    }
    not_allowed = null
  }
}

variable "auto_upgrade_maintenance_window" {
  description = "Configures the auto upgrade maintenance window for Kubernetes Version."
  type = object({
    frequency  = string
    interval   = string
    duration   = string
    dayOfWeek  = optional(string)
    dayOfMonth = optional(number)
    weekIndex  = optional(string)
    startTime  = optional(string)
    utcOffset  = optional(string)
    startDate  = optional(string)
    notAllowed = optional(object({
      start = string
      end   = string
    }))
  })
  default = null
}

variable "node_os_upgrade_channel" {
  description = "The upgrade channel for this Kubernetes Cluster Nodes' OS Image. Possible values are Unmanaged, SecurityPatch, NodeImage and None. Defaults to NodeImage."
  type        = string
  default     = "NodeImage"
}

variable "node_os_upgrade_maintenance_window" {
  description = "Configures the maintenance window for the Operating System of the Kubernetes nodes."
  type = object({
    frequency  = string
    interval   = string
    duration   = string
    dayOfWeek  = optional(string)
    dayOfMonth = optional(number)
    weekIndex  = optional(string)
    startTime  = optional(string)
    utcOffset  = optional(string)
    startDate  = optional(string)
    notAllowed = optional(object({
      start = string
      end   = string
    }))
  })
  default = null
}

variable "http_proxy_config" {
  description = "Configures proxy usage for the cluster and the nodes."
  type = object({
    httpProxy  = string
    httpsProxy = string
    noProxy    = list(string)
  })
  default = null
}

variable "disk_encryption_set_id" {
  description = "The ID of the Disk Encryption Set which should be used for the Nodes and Volumes."
  type        = string
  default     = null
}

variable "run_command_enabled" {
  description = "Whether to enable run command for the cluster or not. Defaults to true."
  type        = bool
  default     = true
}

variable "api_server_subnet_id" {
  description = "The resource id of the Virtual Network Subnet where the API Server gets integrated."
  type        = string
}

variable "cluster_virtual_network_id" {
  description = "The resource id of the Virtual Network where this AKS Cluster is deployed."
  type        = string
}

variable "cluster_dns_zone_id" {
  description = "The resource id of the Private DNS Zone used for this AKS Cluster is deployed. `**NOTE:**` This needs to be configured if private cluster is used"
  type        = string
  default     = null
}

variable "storage_profile" {
  description = "Configures the storage profile for the Kubernetes Cluster."
  type = object({
    blobDriverEnabled         = optional(bool, false)
    diskDriverEnabled         = optional(bool, true)
    fileDriverEnabled         = optional(bool, true)
    snapshotControllerEnabled = optional(bool, true)
  })
  default = {
    blobDriverEnabled         = false
    diskDriverEnabled         = true
    fileDriverEnabled         = true
    snapshotControllerEnabled = true
  }
}

variable "bootstrap_container_registry_id" {
  description = "The ID of the Container Registry which should be used for the Kubernetes Cluster."
  type        = string
}

variable "default_node_pool" {
  description = "Complete configuration of the default node pool"
  type = object({
    name                       = string
    tags                       = optional(map(string))
    orchestratorVersion        = optional(string, null)
    mode                       = optional(string, "System")
    nodePoolSubnetId           = string
    vmSize                     = optional(string, "Standard_D4s_v6")
    osDiskSizeGB               = optional(number, "128")
    osDiskType                 = optional(string, "Managed")
    enableUltraSSD             = optional(bool, null)
    availabilityZones          = optional(list(string), [])
    enableAutoScaling          = optional(bool, true)
    scaleDownMode              = optional(string, "Delete")
    nodeCount                  = optional(number, 2)
    minNodeCount               = optional(number, 2)
    maxNodeCount               = optional(number, 3)
    maxPods                    = optional(number, 30)
    nodeTaints                 = optional(list(string), null)
    nodeLabels                 = optional(map(string), null)
    capacityReservationGroupId = optional(string, null)
    onlyCriticalAddonsEnabled  = optional(bool, false)

    kubeletDiskType = optional(string, "OS")
    kubeletConfig = optional(object({
      allowedUnsafeSysctls  = optional(list(string), null)
      containerLogMaxLine   = optional(number, null)
      containerLogMaxSizeMb = optional(number, null)
      cpuCfsQuotaEnabled    = optional(bool, null)
      cpuCfsQuotaPeriod     = optional(number, null)
      cpuManagerPolicy      = optional(string, null)
      imageGcHighThreshold  = optional(number, null)
      imageGcLowThreshold   = optional(number, null)
      podMaxPid             = optional(number, null)
      topologyManagerPolicy = optional(string, null)
    }))
  })
}

variable "additional_node_pools" {
  description = "List of additional Node Pools deployed in the Cluster"
  type = map(object({
    tags                       = optional(map(string))
    orchestratorVersion        = optional(string, null)
    mode                       = optional(string, "User")
    nodePoolSubnetId           = string
    vmSize                     = optional(string, "Standard_D4s_v6")
    osDiskSizeGB               = optional(number, "128")
    osDiskType                 = optional(string, "Managed")
    enableUltraSSD             = optional(bool, false)
    availabilityZones          = optional(list(string), [])
    enableAutoScaling          = optional(bool, true)
    scaleDownMode              = optional(string, "Delete")
    nodeCount                  = optional(number, 2)
    minNodeCount               = optional(number, 2)
    maxNodeCount               = optional(number, 3)
    maxPods                    = optional(number, 30)
    nodeTaints                 = optional(list(string), null)
    nodeLabels                 = optional(map(string), null)
    capacityReservationGroupId = optional(string, null)

    kubeletDiskType = optional(string, "OS")
    kubeletConfig = optional(object({
      allowedUnsafeSysctls  = optional(list(string), null)
      containerLogMaxLine   = optional(number, null)
      containerLogMaxSizeMb = optional(number, null)
      cpuCfsQuotaEnabled    = optional(bool, null)
      cpuCfsQuotaPeriod     = optional(number, null)
      cpuManagerPolicy      = optional(string, null)
      imageGcHighThreshold  = optional(number, null)
      imageGcLowThreshold   = optional(number, null)
      podMaxPid             = optional(number, null)
      topologyManagerPolicy = optional(string, null)
    }))
  }))
  default = {}
}
