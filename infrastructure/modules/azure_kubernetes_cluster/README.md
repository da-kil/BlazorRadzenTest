# Azure Kubernetes Service

## Features
This blueprint comes with the following features

- Deployment of Azure Kubernetes Service including:
  - Private Cluster with no Outbound Internet Access requirements and Private Endpoint for API Server Data Plane Access
  - At least one Kubernetes node pool
  - etcd Encryption using a Key generated inside Private Azure Key Vault leveraging API Server VNet Integration

## Usage Scenarios
Azure Kubernetes Service (AKS) is a managed Kubernetes service that you can use to deploy and manage containerized applications. You need minimal container orchestration expertise to use AKS. AKS reduces the complexity and operational overhead of managing Kubernetes by offloading much of that responsibility to Azure. AKS is an ideal platform for deploying and managing containerized applications that require high availability, scalability, and portability, and for deploying applications to multiple regions, using open-source tools, and integrating with existing DevOps tools.

## Dependencies
This blueprint has the following dependencies:
- Azure Virtual Network with the following subnets:
  - Node Pool Subnet: Contains all nodes of the cluster, plan big enough to cover application requirements
  - Private Endpoint Subnet: For the Private Endpoint to access the Clusters data plane
  - API Server VNet Integration Subnet: For the cluster to be able to reach the Azure Key Vault for Key Management Service on its private endpoint. This requires a delegation to `Microsoft.ContainerService/managedClusters`.
- User Assigned Managed Identities for Cluster and Kubelet (Required role assignments will be managed by the Blueprint)
- Azure Key Vault: For Key Management Service
  - The Blueprint will assign the Cluster Identity required permission on the Key Vault and generate an etcd Encryption Key
- Azure Container Instance with a **specific** cache rule to Microsoft Artifact Registry
  - This can be configured using the Azure Container Registry Blueprint:
    ```
    containerRegistryCacheRules = [{
      name       = "aks-managed-mcr"
      targetRepo = "aks-managed-repository/*"
      sourceRepo = "mcr.microsoft.com/*"
    }]
    ```
- (Optional) Azure Log Analytics Workspace

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.12.2 |
| <a name="requirement_azapi"></a> [azapi](#requirement\_azapi) | >= 2.6.1 |
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | >= 4.45.0 |
| <a name="requirement_http"></a> [http](#requirement\_http) | >=3.5.0 |
| <a name="requirement_random"></a> [random](#requirement\_random) | >= 3.7.2 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) | >= 4.45.0 |
| <a name="provider_http"></a> [http](#provider\_http) | >=3.5.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [azurerm_kubernetes_cluster.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/kubernetes_cluster) | resource |
| [azurerm_kubernetes_cluster_node_pool.pool](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/kubernetes_cluster_node_pool) | resource |
| [azurerm_monitor_diagnostic_setting.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |
| [azurerm_role_assignment.cluster_to_akv_control](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) | resource |
| [azurerm_role_assignment.cluster_to_akv_data](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) | resource |
| [azurerm_role_assignment.cluster_to_dns](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) | resource |
| [azurerm_role_assignment.cluster_to_kubelet](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) | resource |
| [azurerm_role_assignment.cluster_to_vnt](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) | resource |
| [azurerm_role_assignment.kubelet_to_acr](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) | resource |
| [azurerm_client_config.current](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/client_config) | data source |
| [http_http.proxy_cert](https://registry.terraform.io/providers/hashicorp/http/latest/docs/data-sources/http) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_aci_node_subnet"></a> [aci\_node\_subnet](#input\_aci\_node\_subnet) | The subnet name for the virtual nodes to run. | `string` | `null` | no |
| <a name="input_additional_node_pools"></a> [additional\_node\_pools](#input\_additional\_node\_pools) | List of additional Node Pools deployed in the Cluster | <pre>map(object({<br/>    tags                       = optional(map(string))<br/>    orchestratorVersion        = optional(string, null)<br/>    mode                       = optional(string, "User")<br/>    nodePoolSubnetId           = string<br/>    vmSize                     = optional(string, "Standard_D4s_v6")<br/>    osDiskSizeGB               = optional(number, "128")<br/>    osDiskType                 = optional(string, "Managed")<br/>    enableUltraSSD             = optional(bool, false)<br/>    availabilityZones          = optional(list(string), [])<br/>    enableAutoScaling          = optional(bool, true)<br/>    scaleDownMode              = optional(string, "Delete")<br/>    nodeCount                  = optional(number, 2)<br/>    minNodeCount               = optional(number, 2)<br/>    maxNodeCount               = optional(number, 3)<br/>    maxPods                    = optional(number, 30)<br/>    nodeTaints                 = optional(list(string), null)<br/>    nodeLabels                 = optional(map(string), null)<br/>    capacityReservationGroupId = optional(string, null)<br/><br/>    kubeletDiskType = optional(string, "OS")<br/>    kubeletConfig = optional(object({<br/>      allowedUnsafeSysctls  = optional(list(string), null)<br/>      containerLogMaxLine   = optional(number, null)<br/>      containerLogMaxSizeMb = optional(number, null)<br/>      cpuCfsQuotaEnabled    = optional(bool, null)<br/>      cpuCfsQuotaPeriod     = optional(number, null)<br/>      cpuManagerPolicy      = optional(string, null)<br/>      imageGcHighThreshold  = optional(number, null)<br/>      imageGcLowThreshold   = optional(number, null)<br/>      podMaxPid             = optional(number, null)<br/>      topologyManagerPolicy = optional(string, null)<br/>    }))<br/>  }))</pre> | `{}` | no |
| <a name="input_admin_group_object_ids"></a> [admin\_group\_object\_ids](#input\_admin\_group\_object\_ids) | A list of Object IDs of Entra ID Groups which should have Admin Role on the Cluster. | `list(string)` | `[]` | no |
| <a name="input_api_server_subnet_id"></a> [api\_server\_subnet\_id](#input\_api\_server\_subnet\_id) | The resource id of the Virtual Network Subnet where the API Server gets integrated. | `string` | n/a | yes |
| <a name="input_auto_scaler_profile"></a> [auto\_scaler\_profile](#input\_auto\_scaler\_profile) | Defines the auto scaler profile | <pre>object({<br/>    balanceSimilarNodeGroups                 = optional(string, "false")<br/>    daemonsetEvictionForEmptyNodesEnabled    = optional(bool, false)<br/>    daemonsetEvictionForOccupiedNodesEnabled = optional(bool, true)<br/>    expander                                 = optional(string, "random")<br/>    ignoreDaemonsetsUtilizationEnabled       = optional(bool, false)<br/>    maxGracefulTerminationSec                = optional(string, 600)<br/>    maxNodeProvisioningTime                  = optional(string, "15m")<br/>    maxUnreadyNodes                          = optional(string, 3)<br/>    maxUnreadyPercentage                     = optional(string, 4)<br/>    newPodScaleUpDelay                       = optional(string, "0s")<br/>    scaleDownDelayAfterAdd                   = optional(string, "10m")<br/>    scaleDownDelayAfterDelete                = optional(string, "10s")<br/>    scaleDownDelayAfterFailure               = optional(string, "3m")<br/>    scanInterval                             = optional(string, "10s")<br/>    scaleDownUnneededTime                    = optional(string, "10m")<br/>    scaleDownUnreadyTime                     = optional(string, "20m")<br/>    scaleDownUtilizationThreshold            = optional(string, 0.5)<br/>    emptyBulkDeleteMax                       = optional(string, 10)<br/>    skipNodesWithLocalStorage                = optional(string, false)<br/>    skipNodesWithSystemPods                  = optional(string, true)<br/>  })</pre> | `null` | no |
| <a name="input_auto_upgrade_maintenance_window"></a> [auto\_upgrade\_maintenance\_window](#input\_auto\_upgrade\_maintenance\_window) | Configures the auto upgrade maintenance window for Kubernetes Version. | <pre>object({<br/>    frequency  = string<br/>    interval   = string<br/>    duration   = string<br/>    dayOfWeek  = optional(string)<br/>    dayOfMonth = optional(number)<br/>    weekIndex  = optional(string)<br/>    startTime  = optional(string)<br/>    utcOffset  = optional(string)<br/>    startDate  = optional(string)<br/>    notAllowed = optional(object({<br/>      start = string<br/>      end   = string<br/>    }))<br/>  })</pre> | `null` | no |
| <a name="input_automatic_upgrade_channel"></a> [automatic\_upgrade\_channel](#input\_automatic\_upgrade\_channel) | The upgrade channel for this Kubernetes Cluster. Possible values are patch, rapid, node-image and stable. Omitting this field sets this value to none. | `string` | `null` | no |
| <a name="input_bootstrap_container_registry_id"></a> [bootstrap\_container\_registry\_id](#input\_bootstrap\_container\_registry\_id) | The ID of the Container Registry which should be used for the Kubernetes Cluster. | `string` | n/a | yes |
| <a name="input_cluster_dns_zone_id"></a> [cluster\_dns\_zone\_id](#input\_cluster\_dns\_zone\_id) | The resource id of the Private DNS Zone used for this AKS Cluster is deployed. `**NOTE:**` This needs to be configured if private cluster is used | `string` | `null` | no |
| <a name="input_cluster_identity"></a> [cluster\_identity](#input\_cluster\_identity) | Contains all ids for the managed identity used for the Cluster. | <pre>object({<br/>    clientId   = string<br/>    objectId   = string<br/>    resourceId = string<br/>  })</pre> | n/a | yes |
| <a name="input_cluster_virtual_network_id"></a> [cluster\_virtual\_network\_id](#input\_cluster\_virtual\_network\_id) | The resource id of the Virtual Network where this AKS Cluster is deployed. | `string` | n/a | yes |
| <a name="input_component"></a> [component](#input\_component) | n/a | `string` | n/a | yes |
| <a name="input_default_node_pool"></a> [default\_node\_pool](#input\_default\_node\_pool) | Complete configuration of the default node pool | <pre>object({<br/>    name                       = string<br/>    tags                       = optional(map(string))<br/>    orchestratorVersion        = optional(string, null)<br/>    mode                       = optional(string, "System")<br/>    nodePoolSubnetId           = string<br/>    vmSize                     = optional(string, "Standard_D4s_v6")<br/>    osDiskSizeGB               = optional(number, "128")<br/>    osDiskType                 = optional(string, "Managed")<br/>    enableUltraSSD             = optional(bool, null)<br/>    availabilityZones          = optional(list(string), [])<br/>    enableAutoScaling          = optional(bool, true)<br/>    scaleDownMode              = optional(string, "Delete")<br/>    nodeCount                  = optional(number, 2)<br/>    minNodeCount               = optional(number, 2)<br/>    maxNodeCount               = optional(number, 3)<br/>    maxPods                    = optional(number, 30)<br/>    nodeTaints                 = optional(list(string), null)<br/>    nodeLabels                 = optional(map(string), null)<br/>    capacityReservationGroupId = optional(string, null)<br/>    onlyCriticalAddonsEnabled  = optional(bool, false)<br/><br/>    kubeletDiskType = optional(string, "OS")<br/>    kubeletConfig = optional(object({<br/>      allowedUnsafeSysctls  = optional(list(string), null)<br/>      containerLogMaxLine   = optional(number, null)<br/>      containerLogMaxSizeMb = optional(number, null)<br/>      cpuCfsQuotaEnabled    = optional(bool, null)<br/>      cpuCfsQuotaPeriod     = optional(number, null)<br/>      cpuManagerPolicy      = optional(string, null)<br/>      imageGcHighThreshold  = optional(number, null)<br/>      imageGcLowThreshold   = optional(number, null)<br/>      podMaxPid             = optional(number, null)<br/>      topologyManagerPolicy = optional(string, null)<br/>    }))<br/>  })</pre> | n/a | yes |
| <a name="input_deployment_index"></a> [deployment\_index](#input\_deployment\_index) | n/a | `string` | n/a | yes |
| <a name="input_diagnostics"></a> [diagnostics](#input\_diagnostics) | Settings required to configure the log analytics integration. Specify the log\_analytics\_workspace\_id property for the target Log Analytics Workspace.<br/>To enable specific log category groups or categories, check the azure docs for specifics: https://learn.microsoft.com/en-us/azure/azure-monitor/platform/diagnostic-settings?WT.mc_id=Portal-Microsoft_Azure_Monitoring&tabs=portal#category-groups | <pre>object({<br/>    log_analytics_workspace_id = string<br/>    log_metrics                = optional(list(string), [])<br/>    log_category_groups        = optional(list(string), ["allLogs"])<br/>    log_categories             = optional(list(string), [])<br/>  })</pre> | `null` | no |
| <a name="input_disk_encryption_set_id"></a> [disk\_encryption\_set\_id](#input\_disk\_encryption\_set\_id) | The ID of the Disk Encryption Set which should be used for the Nodes and Volumes. | `string` | `null` | no |
| <a name="input_env"></a> [env](#input\_env) | The deployment environment name | `string` | n/a | yes |
| <a name="input_etcd_encryption_key_id"></a> [etcd\_encryption\_key\_id](#input\_etcd\_encryption\_key\_id) | Id of the Key Vault Key used for etcd encryption | `string` | `null` | no |
| <a name="input_etcd_encryption_key_vault_id"></a> [etcd\_encryption\_key\_vault\_id](#input\_etcd\_encryption\_key\_vault\_id) | Id of the Key Vault containing the key used for etcd encryption | `string` | `null` | no |
| <a name="input_hardware_maintenance_window"></a> [hardware\_maintenance\_window](#input\_hardware\_maintenance\_window) | Configures the hardware maintenance window for the whole Kubernetes Cluster and its resources. | <pre>object({<br/>    allowed = object({<br/>      day   = string<br/>      hours = list(number)<br/>    })<br/>    notAllowed = optional(object({<br/>      start = string<br/>      end   = string<br/>    }))<br/>  })</pre> | <pre>{<br/>  "allowed": {<br/>    "day": "Saturday",<br/>    "hours": [<br/>      1,<br/>      2<br/>    ]<br/>  },<br/>  "not_allowed": null<br/>}</pre> | no |
| <a name="input_http_proxy_config"></a> [http\_proxy\_config](#input\_http\_proxy\_config) | Configures proxy usage for the cluster and the nodes. | <pre>object({<br/>    httpProxy  = string<br/>    httpsProxy = string<br/>    noProxy    = list(string)<br/>  })</pre> | `null` | no |
| <a name="input_k8s_version"></a> [k8s\_version](#input\_k8s\_version) | Version of Kubernetes specified when creating the AKS managed cluster. If not specified, the latest recommended version will be used at provisioning time (but won't auto-upgrade). AKS does not require an exact patch version to be specified, minor version aliases such as 1.22 are also supported. - The minor version's latest GA patch is automatically chosen in that case. | `string` | `null` | no |
| <a name="input_kubelet_identity"></a> [kubelet\_identity](#input\_kubelet\_identity) | Contains all ids for the managed identity used for the Kubelet. | <pre>object({<br/>    clientId   = string<br/>    objectId   = string<br/>    resourceId = string<br/>  })</pre> | n/a | yes |
| <a name="input_location"></a> [location](#input\_location) | n/a | `string` | `"switzerlandnorth"` | no |
| <a name="input_monitor_metrics"></a> [monitor\_metrics](#input\_monitor\_metrics) | Configures the allowed monitor metrics | <pre>object({<br/>    annotationsAllowed = list(string)<br/>    labelsAllowed      = list(string)<br/>  })</pre> | `null` | no |
| <a name="input_node_os_upgrade_channel"></a> [node\_os\_upgrade\_channel](#input\_node\_os\_upgrade\_channel) | The upgrade channel for this Kubernetes Cluster Nodes' OS Image. Possible values are Unmanaged, SecurityPatch, NodeImage and None. Defaults to NodeImage. | `string` | `"NodeImage"` | no |
| <a name="input_node_os_upgrade_maintenance_window"></a> [node\_os\_upgrade\_maintenance\_window](#input\_node\_os\_upgrade\_maintenance\_window) | Configures the maintenance window for the Operating System of the Kubernetes nodes. | <pre>object({<br/>    frequency  = string<br/>    interval   = string<br/>    duration   = string<br/>    dayOfWeek  = optional(string)<br/>    dayOfMonth = optional(number)<br/>    weekIndex  = optional(string)<br/>    startTime  = optional(string)<br/>    utcOffset  = optional(string)<br/>    startDate  = optional(string)<br/>    notAllowed = optional(object({<br/>      start = string<br/>      end   = string<br/>    }))<br/>  })</pre> | `null` | no |
| <a name="input_private_cluster_enabled"></a> [private\_cluster\_enabled](#input\_private\_cluster\_enabled) | If true cluster needs to be configured with api server subnet | `bool` | `true` | no |
| <a name="input_private_cluster_outbound_type"></a> [private\_cluster\_outbound\_type](#input\_private\_cluster\_outbound\_type) | Whether the Kubernetes Cluster is allowed to have outbound connections or not. If set to `none`, the Cluster needs to have a private container registry with cache rules enabled | `string` | `"loadBalancer"` | no |
| <a name="input_prox_url"></a> [prox\_url](#input\_prox\_url) | URI of the proxy CA cert | `string` | `null` | no |
| <a name="input_random_name_infix"></a> [random\_name\_infix](#input\_random\_name\_infix) | Random resource name infix. | `string` | n/a | yes |
| <a name="input_resource_group_name"></a> [resource\_group\_name](#input\_resource\_group\_name) | n/a | `string` | n/a | yes |
| <a name="input_run_command_enabled"></a> [run\_command\_enabled](#input\_run\_command\_enabled) | Whether to enable run command for the cluster or not. Defaults to true. | `bool` | `true` | no |
| <a name="input_sgx_quote_helper_enabled"></a> [sgx\_quote\_helper\_enabled](#input\_sgx\_quote\_helper\_enabled) | Should the SGX quote helper be enabled? | `bool` | `false` | no |
| <a name="input_sku_tier"></a> [sku\_tier](#input\_sku\_tier) | The SKU Tier that should be used for this Kubernetes Cluster. Possible values are Standard (which includes the Uptime SLA) or Premium. | `string` | `"Standard"` | no |
| <a name="input_storage_profile"></a> [storage\_profile](#input\_storage\_profile) | Configures the storage profile for the Kubernetes Cluster. | <pre>object({<br/>    blobDriverEnabled         = optional(bool, false)<br/>    diskDriverEnabled         = optional(bool, true)<br/>    fileDriverEnabled         = optional(bool, true)<br/>    snapshotControllerEnabled = optional(bool, true)<br/>  })</pre> | <pre>{<br/>  "blobDriverEnabled": false,<br/>  "diskDriverEnabled": true,<br/>  "fileDriverEnabled": true,<br/>  "snapshotControllerEnabled": true<br/>}</pre> | no |
| <a name="input_support_plan"></a> [support\_plan](#input\_support\_plan) | Specifies the support plan which should be used for this Kubernetes Cluster. Possible values are KubernetesOfficial and AKSLongTermSupport. Defaults to KubernetesOfficial. | `string` | `"KubernetesOfficial"` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | n/a | `map(string)` | `{}` | no |
| <a name="input_workload_autoscaler_profile"></a> [workload\_autoscaler\_profile](#input\_workload\_autoscaler\_profile) | Configures Keda and Vertical Pod Autoscaler | <pre>object({<br/>    kedaEnabled                  = bool<br/>    verticalPodAutoscalerEnabled = bool<br/>  })</pre> | `null` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_artifact_identifier"></a> [artifact\_identifier](#output\_artifact\_identifier) | Identifier of the deployed blueprint |
| <a name="output_cluster_portal_fqdn"></a> [cluster\_portal\_fqdn](#output\_cluster\_portal\_fqdn) | Full qualified domain name of cluster used in Azure Portal |
| <a name="output_cluster_private_fqdn"></a> [cluster\_private\_fqdn](#output\_cluster\_private\_fqdn) | Full qualified domain name of cluster used for Private Endpoint |
| <a name="output_kube_admin_config"></a> [kube\_admin\_config](#output\_kube\_admin\_config) | Kube Admin config |
| <a name="output_oidc_issuer_url"></a> [oidc\_issuer\_url](#output\_oidc\_issuer\_url) | URL of the OIDC issuer used for Workload Identity |
| <a name="output_resource_id"></a> [resource\_id](#output\_resource\_id) | Resource ID of the Kubernetes Cluster |
| <a name="output_resource_name"></a> [resource\_name](#output\_resource\_name) | Name of the Kubernetes Cluster |
| <a name="output_secret_identity"></a> [secret\_identity](#output\_secret\_identity) | Properties of the managed identity used by the secret provider |
<!-- END_TF_DOCS -->

## Example config

The following example shows how variables can be modified to deploy this blueprint:

> Container Registry, Key Vault and Managed Identities must be pre-deployed. Blueprints can be used for this

> The subnet referenced inside `var.apiServerSubnetId` must be configured with a delegation to `Microsoft.ContainerService/managedClusters`.

```terraform
variable "connectivitySubscriptionId" {
  type    = string
  description = "Subscription Id which contains the Connectivity Hub"
}

provider "azurerm" {
  alias                           = "connectivity"
  subscription_id                 = var.connectivitySubscriptionId
  resource_provider_registrations = "none"
  features {}
}

resource "random_string" "aks" {
  length  = 3
  special = false
  upper   = false
}

module "aks" {
  source  = "./azure_kubernetes_cluster"
  version = "1.0.0"
  providers = {
    azurerm              = azurerm
    azurerm.connectivity = azurerm.connectivity
  }

  depends_on = [module.container_registry]

  env                 = var.env
  deployment_index    = var.deployment_index
  component           = var.component
  resource_group_name = var.resource_group
  location            = var.location
  random_name_infix   = random_string.aks.result
  tags                = local.tags

  api_server_subnet_id       = data.terraform_remote_state.test_base.outputs.aks_integration_subnet_id
  cluster_virtual_network_id = data.azurerm_virtual_network.fully.id
  cluster_dns_zone_id        = data.azurerm_private_dns_zone.aks.id

  etcd_encryption_key_id      = azurerm_key_vault_key.generated.id
  etcd_encryption_key_vault_id = module.key_vault.resource_id

  k8sVersion = "1.33.5"

  cluster_identity = {
    clientId   = azurerm_user_assigned_identity.cluster.client_id
    objectId   = azurerm_user_assigned_identity.cluster.principal_id
    resourceId = azurerm_user_assigned_identity.cluster.id
  }

  kubelet_identity = {
    clientId   = azurerm_user_assigned_identity.kubelet.client_id
    objectId   = azurerm_user_assigned_identity.kubelet.principal_id
    resourceId = azurerm_user_assigned_identity.kubelet.id
  }

  default_node_pool = {
    name             = "default"
    nodePoolSubnetId = data.terraform_remote_state.test_base.outputs.node_pool_subnet_id
  }

  additional_node_pools = {
    testpool = {
      nodePoolSubnetId = data.terraform_remote_state.test_base.outputs.node_pool_subnet_id
    }
  }

  bootstrap_container_registry_id = module.container_registry.resource_id

  diagnostics = {
    log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    log_metrics                = ["AllMetrics"]
  }
}

# #######################################################
#  Dependencies
# #######################################################
data "azurerm_client_config" "current" {}

module "container_registry" {
  source  = "../container_resgistry"
  providers = {
    azurerm              = azurerm
    azurerm.connectivity = azurerm.connectivity
  }
  
  component            = var.component
  env                  = var.env
  deployment_index     = var.deployment_index
  resource_group_name  = var.resource_group
  random_name_infix    = random_string.aks.result
  tags                 = local.tags

  private_endpoint_subnet_id = data.terraform_remote_state.test_base.outputs.private_endpoint_subnet_id

  quarantine_policy_enabled = false

  # Need to have active login credentials 
  container_registry_cache_rules = [{
    name       = "aks-managed-mcr"
    targetRepo = "aks-managed-repository/*"
    sourceRepo = "mcr.microsoft.com/*"
  }]

  diagnostics = {
    log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    log_metrics                = ["AllMetrics"]
  }
}

module "key_vault" {
  source  = "../key_vault"
  version = 1.0.0

  providers = {
    azurerm              = azurerm
    azurerm.connectivity = azurerm.connectivity
  }
  
  component           = substr(var.component, 0, 5)
  env                 = var.env
  deployment_index    = var.deployment_index
  resource_group_name = var.resource_group
  random_name_infix   = random_string.aks.result

  diagnostics = {
    log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    log_metrics                = ["AllMetrics"]
  }

  private_endpoint_subnet_id = data.terraform_remote_state.test_base.outputs.private_endpoint_subnet_id
}

# Need for TF to manage secrets
resource "azurerm_role_assignment" "key_vault_crypto_officer" {
  scope                = module.key_vault.resource_id
  role_definition_name = "Key Vault Crypto Officer"
  principal_id         = data.azurerm_client_config.current.object_id
  description          = "Managed by Terraform 🤖"
  principal_type       = "ServicePrincipal"
}

resource "azurerm_key_vault_key" "generated" {
  depends_on   = [azurerm_role_assignment.key_vault_crypto_officer]
  name         = "etcd-encryption"
  key_vault_id = module.key_vault.resource_id
  key_type     = "RSA"
  key_size     = 2048

  key_opts = [
    "decrypt",
    "encrypt",
    "sign",
    "unwrapKey",
    "verify",
    "wrapKey",
  ]

  rotation_policy {
    automatic {
      time_before_expiry = "P30D"
    }

    expire_after         = "P90D"
    notify_before_expiry = "P29D"
  }
}

module "law" {
  source  = "./log_analytics_workspace"
  version = 1.0.0
  
  providers = {
    azurerm              = azurerm
    azurerm.connectivity = azurerm.connectivity
  }

  env                 = var.env
  deployment_index    = var.deployment_index
  component           = var.component
  resource_group_name = var.resource_group
  location            = var.location
  random_name_infix   = random_string.aks.result
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "cluster" {
  name                = "umi-cluster-${var.component}-${var.env}-${local.location_short}-${var.deploymentIndex}"
  location            = var.location
  resource_group_name = var.resource_group
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "kubelet" {
  name                = "umi-kubelet-${var.component}-${var.env}-${local.location_short}-${var.deploymentIndex}"
  location            = var.location
  resource_group_name = var.resource_group
  tags                = local.tags
}
```
