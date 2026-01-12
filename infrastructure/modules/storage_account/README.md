# terraform-azurerm-Storage-Account
This Module deploys an Azure Storage Account. The module will deploy the SA network isolated with private endpoints by default. It is possible to whitelist IPs or VNETs in addition to the PE via the params `network_vnet_id_to_whitelist` and `network_ips_to_whitelist`.

> [!IMPORTANT]  
> By default, Shared Access Keys are disabled, which requires `storage_use_azuread` to be enabled in the [provider configuration](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs#storage_use_azuread-1).

## Resources included 
- Storage Account: _itself_ 
- Private Endpoint: _itself_ needs an existing private DNS Zones. The Zone IDs will be selected automatically from the `central_dns_zone_resource_group_name` variable. 
- Storage Containers, File Shares, Tables and Queues: _itself_ will be created as defined in the variables.
- Customer Managed Key encryption: Can optionally be configured with a customer manged encryption key stored in key vault. See test directory for example.

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.13.3, < 2.0.0 |
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | >= 4.57.0, <5.0.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) | >= 4.57.0, <5.0.0 |
| <a name="provider_azurerm.connectivity"></a> [azurerm.connectivity](#provider\_azurerm.connectivity) | >= 4.57.0, <5.0.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [azurerm_monitor_diagnostic_setting.blobs](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |
| [azurerm_monitor_diagnostic_setting.files](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |
| [azurerm_monitor_diagnostic_setting.queues](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |
| [azurerm_monitor_diagnostic_setting.sa](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |
| [azurerm_monitor_diagnostic_setting.tables](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |
| [azurerm_private_endpoint.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_endpoint) | resource |
| [azurerm_storage_account.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/storage_account) | resource |
| [azurerm_storage_account_customer_managed_key.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/storage_account_customer_managed_key) | resource |
| [azurerm_storage_container.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/storage_container) | resource |
| [azurerm_storage_queue.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/storage_queue) | resource |
| [azurerm_storage_share.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/storage_share) | resource |
| [azurerm_storage_table.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/storage_table) | resource |
| [azurerm_client_config.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/client_config) | data source |
| [azurerm_private_dns_zone.blob](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) | data source |
| [azurerm_private_dns_zone.file](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) | data source |
| [azurerm_private_dns_zone.queue](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) | data source |
| [azurerm_private_dns_zone.table](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_access_tier"></a> [access\_tier](#input\_access\_tier) | The access type of the Storage Account. Must be one of `Hot`, `Cool`, `Cold` or `Premium` | `string` | `"Hot"` | no |
| <a name="input_account_kind"></a> [account\_kind](#input\_account\_kind) | The account type. Must be one of `Storage`, `StorageV2`, `BlobStorage`, `BlockBlobStorage` or `FileStorage` | `string` | `"StorageV2"` | no |
| <a name="input_account_replication_type"></a> [account\_replication\_type](#input\_account\_replication\_type) | The Replication type of the Storage Account. Must be one of `LRS`, `GRS`, `RAGRS`, `ZRS`, `GZRS` or `RAGZRS`. | `string` | `"ZRS"` | no |
| <a name="input_account_tier"></a> [account\_tier](#input\_account\_tier) | Defines the Tier to use for this storage account. Valid options are Standard and Premium. For BlockBlobStorage and FileStorage accounts only Premium is valid. Changing this forces a new resource to be created. | `string` | `"Standard"` | no |
| <a name="input_allow_nested_items_to_be_public"></a> [allow\_nested\_items\_to\_be\_public](#input\_allow\_nested\_items\_to\_be\_public) | Allow or disallow nested items within this Account to opt into being public | `bool` | `false` | no |
| <a name="input_central_dns_zone_resource_group_name"></a> [central\_dns\_zone\_resource\_group\_name](#input\_central\_dns\_zone\_resource\_group\_name) | Name of the external private DNS zone resource group that should be used to register private endpoint A record | `string` | n/a | yes |
| <a name="input_component"></a> [component](#input\_component) | Name of the application component, will be used on resource naming | `string` | n/a | yes |
| <a name="input_deployment_index"></a> [deployment\_index](#input\_deployment\_index) | Index counter of this instance, will be used on resource naming | `string` | `"01"` | no |
| <a name="input_diagnostics"></a> [diagnostics](#input\_diagnostics) | Settings required to configure the log analytics integration. Specify the log\_analytics\_workspace\_id property for the target Log Analytics Workspace.<br/>To enable specific log category groups or categories, check the azure docs for specifics: https://learn.microsoft.com/en-us/azure/azure-monitor/platform/diagnostic-settings?WT.mc_id=Portal-Microsoft_Azure_Monitoring&tabs=portal#category-groups | <pre>object(<br/>    {<br/>      storage_account = optional(<br/>        object({<br/>          log_analytics_workspace_id = string<br/>          log_metrics                = optional(list(string), ["Transaction"])<br/>        })<br/>      )<br/>      blobs = optional(<br/>        object({<br/>          log_analytics_workspace_id = string<br/>          log_metrics                = optional(list(string), ["Transaction"])<br/>          log_category_groups        = optional(list(string), ["allLogs"])<br/>          log_categories             = optional(list(string), [])<br/>        })<br/>      )<br/>      files = optional(<br/>        object({<br/>          log_analytics_workspace_id = string<br/>          log_metrics                = optional(list(string), ["Transaction"])<br/>          log_category_groups        = optional(list(string), ["allLogs"])<br/>          log_categories             = optional(list(string), [])<br/>        })<br/>      )<br/>      tables = optional(<br/>        object({<br/>          log_analytics_workspace_id = string<br/>          log_metrics                = optional(list(string), ["Transaction"])<br/>          log_category_groups        = optional(list(string), ["allLogs"])<br/>          log_categories             = optional(list(string), [])<br/>      }))<br/>      queues = optional(<br/>        object({<br/>          log_analytics_workspace_id = string<br/>          log_metrics                = optional(list(string), ["Transaction"])<br/>          log_category_groups        = optional(list(string), ["allLogs"])<br/>          log_categories             = optional(list(string), [])<br/>      }))<br/>    }<br/>  )</pre> | `null` | no |
| <a name="input_env"></a> [env](#input\_env) | The deployment environment name (dev / test / prod) | `string` | n/a | yes |
| <a name="input_https_traffic_only_enabled"></a> [https\_traffic\_only\_enabled](#input\_https\_traffic\_only\_enabled) | Boolean flag which forces HTTPS if enabled. | `bool` | `true` | no |
| <a name="input_large_file_share_enabled"></a> [large\_file\_share\_enabled](#input\_large\_file\_share\_enabled) | Are Large File Shares Enabled? If unset, defaults to false, except for `account_kind` = `FileStorage` where it is true. | `bool` | `null` | no |
| <a name="input_location"></a> [location](#input\_location) | Deployment location | `string` | `"switzerlandnorth"` | no |
| <a name="input_network_access_mode"></a> [network\_access\_mode](#input\_network\_access\_mode) | How network access is permitted. Must be one of `OnlyPrivateEndpoints`, `OnlyAllowed` or `Public` | `string` | `"OnlyPrivateEndpoints"` | no |
| <a name="input_network_bypass"></a> [network\_bypass](#input\_network\_bypass) | Specifies whether traffic is bypassed for Logging/Metrics/AzureServices. Valid options are any combination of `Logging`, `Metrics`, `AzureService`, or `None` | `set(string)` | <pre>[<br/>  "None"<br/>]</pre> | no |
| <a name="input_network_ips_to_whitelist"></a> [network\_ips\_to\_whitelist](#input\_network\_ips\_to\_whitelist) | IPs to whitelist. WARNING! Setting this config will block the deployment due to azure policy setting. Policy exemption needs to be requested first | `list(string)` | `[]` | no |
| <a name="input_network_subnet_id_to_whitelist"></a> [network\_subnet\_id\_to\_whitelist](#input\_network\_subnet\_id\_to\_whitelist) | Subnets to whitelist. WARNING! Setting this config will block the deployment due to azure policy setting. Policy exemption needs to be requested first | `list(string)` | `[]` | no |
| <a name="input_private_endpoint_subnet_id"></a> [private\_endpoint\_subnet\_id](#input\_private\_endpoint\_subnet\_id) | Private endpoint will be deployed by default for all storageTypes configured. If set to null ipWhitelists need to be set. This will only work with Policy exemptions | `string` | n/a | yes |
| <a name="input_resource_group_name"></a> [resource\_group\_name](#input\_resource\_group\_name) | Name of the Resource Group into which this Storage Account will be deployed | `string` | n/a | yes |
| <a name="input_resource_name_infix"></a> [resource\_name\_infix](#input\_resource\_name\_infix) | Random resource name infix. | `string` | n/a | yes |
| <a name="input_sas_enabled"></a> [sas\_enabled](#input\_sas\_enabled) | Indicates whether the storage account permits requests to be authorized with the account access key via Shared Key. If false, then all requests, including shared access signatures, must be authorized with Azure Active Directory (Azure AD). | `bool` | `true` | no |
| <a name="input_storage_containers"></a> [storage\_containers](#input\_storage\_containers) | List of Object of Storage Containers that should be created in this Storage Account | <pre>list(object({<br/>    name        = string<br/>    access_type = optional(string, "private")<br/>    metadata    = optional(map(string))<br/>  }))</pre> | `[]` | no |
| <a name="input_storage_encryption_customer_managed_key"></a> [storage\_encryption\_customer\_managed\_key](#input\_storage\_encryption\_customer\_managed\_key) | Config for using a Customer managed key for encryption | <pre>object({<br/>    user_assigned_identity_id = string<br/>    // We use a uri instead of id to prevent a diff when the key vault is in a different subscription, see azurerm_storage_account_customer_managed_key docs<br/>    key_vault_uri = string<br/>    key_name      = string<br/>  })</pre> | `null` | no |
| <a name="input_storage_file_shares"></a> [storage\_file\_shares](#input\_storage\_file\_shares) | List of Object of Storage File Shares that should be created in this Storage Account | <pre>list(object({<br/>    name        = string<br/>    metadata    = optional(map(string))<br/>    access_tier = optional(string)<br/>    quota_in_gb = number<br/>    protocol    = string<br/>    acl = optional(list(object({<br/>      id = string<br/>      access_policy = optional(object({<br/>        permissions = string<br/>        start       = string<br/>        end         = string<br/>      }))<br/>    })), [])<br/>  }))</pre> | `[]` | no |
| <a name="input_storage_queues"></a> [storage\_queues](#input\_storage\_queues) | List of Object of Storage Queues that should be created in this Storage Account | <pre>list(object({<br/>    name     = string<br/>    metadata = optional(map(string))<br/>  }))</pre> | `[]` | no |
| <a name="input_storage_tables"></a> [storage\_tables](#input\_storage\_tables) | List of Object of Storage Tables that should be created in this Storage Account. Be sure to configure the storage account so that terraform has network access, otherwise creation will fail! | <pre>list(object({<br/>    name     = string<br/>    metadata = optional(map(string))<br/><br/>    acl = optional(list(object({<br/>      id = string<br/>      access_policy = optional(object({<br/>        permissions = string<br/>        start       = string<br/>        end         = string<br/>      }))<br/>    })), [])<br/>  }))</pre> | `[]` | no |
| <a name="input_storage_types"></a> [storage\_types](#input\_storage\_types) | Storage usage type. For each type the according Private DNS Zone name has to be provided in `dns_zone_names`. Allowed are `blob`, `table`, `queue` or `file`. | `set(string)` | <pre>[<br/>  "blob"<br/>]</pre> | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Map of TAGs that will be added to the deployed resources | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_artifact_identifier"></a> [artifact\_identifier](#output\_artifact\_identifier) | Identifier of the deployed blueprint |
| <a name="output_primary_access_key"></a> [primary\_access\_key](#output\_primary\_access\_key) | the primary access key for this storage account |
| <a name="output_resource_id"></a> [resource\_id](#output\_resource\_id) | The Id from the storage account |
| <a name="output_resource_name"></a> [resource\_name](#output\_resource\_name) | The Name from the storage account |
| <a name="output_secondary_access_key"></a> [secondary\_access\_key](#output\_secondary\_access\_key) | the secondary access key for this storage account |
| <a name="output_storage_account_container_ids"></a> [storage\_account\_container\_ids](#output\_storage\_account\_container\_ids) | Map of container names to their resource IDs |
| <a name="output_storage_account_container_names"></a> [storage\_account\_container\_names](#output\_storage\_account\_container\_names) | Map of container keys to their names |
| <a name="output_storage_account_file_ids"></a> [storage\_account\_file\_ids](#output\_storage\_account\_file\_ids) | Map of file share names to their resource IDs |
| <a name="output_storage_account_file_names"></a> [storage\_account\_file\_names](#output\_storage\_account\_file\_names) | Map of file share keys to their names |
| <a name="output_storage_account_primary_blob_endpoint"></a> [storage\_account\_primary\_blob\_endpoint](#output\_storage\_account\_primary\_blob\_endpoint) | Primary endpoint for Blob storage |
| <a name="output_storage_account_primary_file_endpoint"></a> [storage\_account\_primary\_file\_endpoint](#output\_storage\_account\_primary\_file\_endpoint) | Primary endpoint for File storage |
| <a name="output_storage_account_primary_location"></a> [storage\_account\_primary\_location](#output\_storage\_account\_primary\_location) | The primary Azure region where the Storage Account is located |
| <a name="output_storage_account_primary_queue_endpoint"></a> [storage\_account\_primary\_queue\_endpoint](#output\_storage\_account\_primary\_queue\_endpoint) | Primary endpoint for Queue storage |
| <a name="output_storage_account_primary_table_endpoint"></a> [storage\_account\_primary\_table\_endpoint](#output\_storage\_account\_primary\_table\_endpoint) | Primary endpoint for Table storage |
| <a name="output_storage_account_queue_ids"></a> [storage\_account\_queue\_ids](#output\_storage\_account\_queue\_ids) | Map of queue names to their resource IDs |
| <a name="output_storage_account_queue_names"></a> [storage\_account\_queue\_names](#output\_storage\_account\_queue\_names) | Map of queue keys to their names |
| <a name="output_storage_account_secondary_blob_endpoint"></a> [storage\_account\_secondary\_blob\_endpoint](#output\_storage\_account\_secondary\_blob\_endpoint) | Secondary endpoint for Blob storage (if geo-redundancy is enabled) |
| <a name="output_storage_account_secondary_file_endpoint"></a> [storage\_account\_secondary\_file\_endpoint](#output\_storage\_account\_secondary\_file\_endpoint) | Secondary endpoint for File storage (if geo-redundancy is enabled) |
| <a name="output_storage_account_secondary_location"></a> [storage\_account\_secondary\_location](#output\_storage\_account\_secondary\_location) | The secondary Azure region for geo-redundancy of the Storage Account |
| <a name="output_storage_account_secondary_queue_endpoint"></a> [storage\_account\_secondary\_queue\_endpoint](#output\_storage\_account\_secondary\_queue\_endpoint) | Secondary endpoint for Queue storage (if geo-redundancy is enabled) |
| <a name="output_storage_account_secondary_table_endpoint"></a> [storage\_account\_secondary\_table\_endpoint](#output\_storage\_account\_secondary\_table\_endpoint) | Secondary endpoint for Table storage (if geo-redundancy is enabled) |
| <a name="output_storage_account_table_ids"></a> [storage\_account\_table\_ids](#output\_storage\_account\_table\_ids) | Map of table names to their resource IDs |
| <a name="output_storage_account_table_names"></a> [storage\_account\_table\_names](#output\_storage\_account\_table\_names) | Map of table keys to their names |
<!-- END_TF_DOCS -->

## Example config

```terraform
variable "connectivity_subscription_id" {
  type        = string
  description = "Subscription Id which contains the Connectivity Hub"
}

provider "azurerm" {
  alias                           = "connectivity"
  subscription_id                 = var.connectivity_subscription_id
  storage_use_azuread             = true
  features {}
}

module "storage_account" {
  source = "./storage_account"

  providers = {
    azurerm              = azurerm
    azurerm.connectivity = azurerm
  }

  central_dns_zone_resource_group_name = "rg-dns-zones"
  component                            = "dmo"
  env                                  = "prod"
  resource_group_name                  = "rg-demo-deployments"
  private_endpoint_subnet_id           = data.azurerm_subnet.pe.id
  resource_name_infix                  = random_string.this.result

  //TODO only configure diagnostic types accordingly to your needs 
  diagnostics = {
    storage_account = {
      log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    }
    blobs = {
      log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    }
    files = {
      log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    }
    tables = {
      log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    }
    queues = {
      log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    }
  }

  //TODO only deploy storage types accordingly to your needs 
  storage_types = ["blob", "file", "queue", "table"]
  storage_containers = [
    {
      name = "test"
    },
    {
      name = "another-container"
    },
  ]
  storage_file_shares = [
    {
      name        = "test"
      quota_in_gb = 10
      protocol    = "SMB"
      acl = [
        {
          id = "some_uuid"
          access_policy = {
            permissions = "rwdl" # Read, Write, Delete, List
            start       = "2040-04-12T09:38:21.0000000Z" // Date should be in future
            end         = "2040-04-13T09:38:21.0000000Z"
          }
        },
      ]
    },
  ]
  storage_queues = [
    {
      name = "test"
    },
  ]
  storage_tables = [
    {
      name = "test"
    },
  ]
}
```
