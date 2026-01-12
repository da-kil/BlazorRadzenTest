# PostgreSQL Flexible Server

## Supported by this Product

List service specific SKUs: See [Compute options in Azure Database for PostgreSQL flexible server](https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-compute) 

## Usage Scenarios

This blueprint can be used to deploy a Azure PostgreSQL Flexible Server.

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.12.0 |
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | >= 4.57.0, <5.0.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) | >= 4.57.0, <5.0.0 |
| <a name="provider_azurerm.connectivity"></a> [azurerm.connectivity](#provider\_azurerm.connectivity) | >= 4.57.0, <5.0.0 |
| <a name="provider_random"></a> [random](#provider\_random) | n/a |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [azurerm_monitor_diagnostic_setting.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |
| [azurerm_postgresql_flexible_server.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/postgresql_flexible_server) | resource |
| [random_string.infix](https://registry.terraform.io/providers/hashicorp/random/latest/docs/resources/string) | resource |
| [azurerm_client_config.current](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/client_config) | data source |
| [azurerm_private_dns_zone.postgres](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_administrator_login"></a> [administrator\_login](#input\_administrator\_login) | The local admin username. Configuring this only works in conjunction with `passwordAuthEnabled=true` | `string` | `null` | no |
| <a name="input_administrator_password"></a> [administrator\_password](#input\_administrator\_password) | The local admin password. Configuring this only works in conjunction with `passwordAuthEnabled=true` | `string` | `null` | no |
| <a name="input_auto_grow_enabled"></a> [auto\_grow\_enabled](#input\_auto\_grow\_enabled) | Is the storage auto grow for PostgreSQL Flexible Server enabled | `bool` | `false` | no |
| <a name="input_backup_retention_days"></a> [backup\_retention\_days](#input\_backup\_retention\_days) | The backup retention days for the PostgreSQL Flexible Server | `number` | `7` | no |
| <a name="input_central_dns_zone_resource_group_name"></a> [central\_dns\_zone\_resource\_group\_name](#input\_central\_dns\_zone\_resource\_group\_name) | Name of the external private DNS zone that should be used to register private endpoint A record | `string` | n/a | yes |
| <a name="input_component"></a> [component](#input\_component) | n/a | `string` | n/a | yes |
| <a name="input_delegated_sub_net_id"></a> [delegated\_sub\_net\_id](#input\_delegated\_sub\_net\_id) | Resource ID of the Subnet that should be used for private conneciton | `string` | n/a | yes |
| <a name="input_deployment_index"></a> [deployment\_index](#input\_deployment\_index) | n/a | `string` | n/a | yes |
| <a name="input_deployment_zone"></a> [deployment\_zone](#input\_deployment\_zone) | Azure Zone the PostgresDB should be deployed into. `IMPORTANT!:` can **ONLY** be set duing creation time. | `string` | `1` | no |
| <a name="input_diagnostics"></a> [diagnostics](#input\_diagnostics) | Settings required to configure the log analytics integration. Specify the log\_analytics\_workspace\_id property for the target Log Analytics Workspace.<br/>To enable specific log category groups or categories, check the azure docs for specifics: https://learn.microsoft.com/en-us/azure/azure-monitor/platform/diagnostic-settings?WT.mc_id=Portal-Microsoft_Azure_Monitoring&tabs=portal#category-groups | <pre>object({<br/>    log_analytics_workspace_id = string<br/>    log_metrics                = optional(list(string), [])<br/>    log_category_groups        = optional(list(string), ["allLogs"])<br/>    log_categories             = optional(list(string), [])<br/>  })</pre> | `null` | no |
| <a name="input_env"></a> [env](#input\_env) | The deployment environment name | `string` | n/a | yes |
| <a name="input_geo_redundant_backup_enabled"></a> [geo\_redundant\_backup\_enabled](#input\_geo\_redundant\_backup\_enabled) | Is Geo-Redundant backup enabled on the PostgreSQL Flexible Server. Backup will be stored in the switzerland west region | `bool` | `false` | no |
| <a name="input_high_availability"></a> [high\_availability](#input\_high\_availability) | If provided, PostgreSQL Flexible Server will be deploy with a active replication to another Azure zone | <pre>object({<br/>    mode                    = optional(string, "ZoneRedundant")<br/>    standbyAvailabilityZone = optional(number, 2)<br/>  })</pre> | `{}` | no |
| <a name="input_identity_ids"></a> [identity\_ids](#input\_identity\_ids) | Specifies a list of user managed identity ids to be assigned. Required if type is UserAssigned | `list(string)` | `null` | no |
| <a name="input_identity_type"></a> [identity\_type](#input\_identity\_type) | Specifies the identity type of the App Service. Possible values are: `SystemAssigned` (where Azure will generate a Service Principal for you), `UserAssigned` where you can specify the Service Principal IDs in the identity\_ids field, and `SystemAssigned, UserAssigned` which assigns both a system managed identity as well as the specified user assigned identities. | `string` | `"SystemAssigned"` | no |
| <a name="input_location"></a> [location](#input\_location) | n/a | `string` | `"switzerlandnorth"` | no |
| <a name="input_maintenance_window"></a> [maintenance\_window](#input\_maintenance\_window) | The timeframe azure is allowed to do maintenance on the Postgres Server or hardware. This should be configured for prod env | <pre>object({<br/>    dayOfWeek    = string<br/>    startHour    = string<br/>    start_minute = string<br/>  })</pre> | `null` | no |
| <a name="input_password_auth_enabled"></a> [password\_auth\_enabled](#input\_password\_auth\_enabled) | whether local accounts are enabled. Setting this to `true` requires a security exemption | `bool` | `false` | no |
| <a name="input_postgres_version"></a> [postgres\_version](#input\_postgres\_version) | The version of this PostgreSQL Flexible Server | `string` | `16` | no |
| <a name="input_public_network_access_enabled"></a> [public\_network\_access\_enabled](#input\_public\_network\_access\_enabled) | Whether public access is enabled. Setting this to `true` requires a security exemption | `bool` | `false` | no |
| <a name="input_resource_group_name"></a> [resource\_group\_name](#input\_resource\_group\_name) | n/a | `string` | n/a | yes |
| <a name="input_resource_name_infix"></a> [resource\_name\_infix](#input\_resource\_name\_infix) | Random resource name infix. | `string` | n/a | yes |
| <a name="input_sku"></a> [sku](#input\_sku) | Specifies the SKU for this PostgresSQL | `string` | `"GP_Standard_D2s_v3"` | no |
| <a name="input_storage_encryption_key_vault_id"></a> [storage\_encryption\_key\_vault\_id](#input\_storage\_encryption\_key\_vault\_id) | Encryption | `string` | `null` | no |
| <a name="input_storage_in_mb"></a> [storage\_in\_mb](#input\_storage\_in\_mb) | The max storage allowed for the PostgreSQL Flexible Server | `string` | `"32768"` | no |
| <a name="input_storage_tier"></a> [storage\_tier](#input\_storage\_tier) | The name of storage performance tier. | `string` | `"P4"` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | n/a | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_artifact_identifier"></a> [artifact\_identifier](#output\_artifact\_identifier) | The Identifier which identifies the artifact by tag. |
| <a name="output_resource_id"></a> [resource\_id](#output\_resource\_id) | The Id from the Key Vault |
| <a name="output_resource_name"></a> [resource\_name](#output\_resource\_name) | The Name from the Key Vault |
<!-- END_TF_DOCS -->

## Example config

```terraform
variable "connectivitySubscriptionId" {
  type    = string
  description = "Subscription Id which contains the Connectivity Hub"
}

provider "azurerm" {
  alias                           = "connectivity"
  subscription_id                 = var.connectivitySubscriptionId
  features {}
}

module "example_flexible_server" {
  source  = "../postgres_flexible_server"

  providers = {
    azurerm              = azurerm
    azurerm.connectivity = azurerm
  }

  component           = "dmo"
  env                 = "prod"
  deployment_index    = "01"
  resource_group_name = "rg-demo-deployments"

  central_dns_zone_resource_group_name = "rg-dns-zones"
  delegated_sub_net_id = data.azurerm_subnet.pg_flex.id
  resource_name_infix = "042"

  diagnostics = {
    log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    log_metrics = ["AllMetrics"]
  }
}
```
