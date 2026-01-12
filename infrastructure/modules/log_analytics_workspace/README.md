# terraform-azurerm-Log-Analytics-Workspace

## Features
This module comes with the following features

- Deployment of Azure Log Analytics Workspace

## Usage Scenarios
A Log Analytics workspace is a unique environment for log data from Azure Monitor and other Azure services, such as Microsoft Sentinel and Microsoft Defender for Cloud. Each workspace has its own data repository and configuration but might combine data from multiple services.

## Dependencies
This module has no dependencies.

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

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [azurerm_log_analytics_workspace.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/log_analytics_workspace) | resource |
| [azurerm_monitor_diagnostic_setting.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_allow_public_ingestion_access"></a> [allow\_public\_ingestion\_access](#input\_allow\_public\_ingestion\_access) | The network access type for accessing Log Analytics ingestion. - Enabled or Disabled | `bool` | `true` | no |
| <a name="input_allow_public_query_access"></a> [allow\_public\_query\_access](#input\_allow\_public\_query\_access) | The network access type for accessing Log Analytics query. - Enabled or Disabled | `bool` | `false` | no |
| <a name="input_component"></a> [component](#input\_component) | The app component name, will be used for resource naming | `string` | n/a | yes |
| <a name="input_daily_quota_gb"></a> [daily\_quota\_gb](#input\_daily\_quota\_gb) | The workspace daily quota for ingestion in GB. Defaults to -1 (unlimited). When sku is set to Free this field can be set to a maximum of 0.5 (GB) | `any` | `null` | no |
| <a name="input_deployment_index"></a> [deployment\_index](#input\_deployment\_index) | The deployment index, will be used for resource naming | `string` | `"01"` | no |
| <a name="input_diagnostics"></a> [diagnostics](#input\_diagnostics) | Settings required to configure the log analytics integration. Specify the log\_analytics\_workspace\_id property for the target Log Analytics Workspace.<br/>To enable specific log category groups or categories, check the azure docs for specifics: https://learn.microsoft.com/en-us/azure/azure-monitor/platform/diagnostic-settings?WT.mc_id=Portal-Microsoft_Azure_Monitoring&tabs=portal#category-groups | <pre>object({<br/>    log_analytics_workspace_id = string<br/>    log_metrics                = optional(list(string), [])<br/>    log_category_groups        = optional(list(string), ["allLogs"])<br/>    log_categories             = optional(list(string), [])<br/>  })</pre> | `null` | no |
| <a name="input_env"></a> [env](#input\_env) | The deployment environment name, will be used for resource naming | `string` | n/a | yes |
| <a name="input_local_auth_enabled"></a> [local\_auth\_enabled](#input\_local\_auth\_enabled) | Whether LAW local authentication should be enabled. | `bool` | `false` | no |
| <a name="input_location"></a> [location](#input\_location) | Deployment location, will be used for resource naming | `string` | `"switzerlandnorth"` | no |
| <a name="input_resource_group_name"></a> [resource\_group\_name](#input\_resource\_group\_name) | The target resource group for the deployment | `string` | n/a | yes |
| <a name="input_resource_name_infix"></a> [resource\_name\_infix](#input\_resource\_name\_infix) | Random resource name infix. | `string` | n/a | yes |
| <a name="input_retention_days"></a> [retention\_days](#input\_retention\_days) | The workspace data retention in days. Possible values are either 7 (Free Tier only) or range between 30 and 730. Defaults to 30 | `number` | `30` | no |
| <a name="input_sku"></a> [sku](#input\_sku) | Specifies the Sku of the Log Analytics Workspace. Possible values are Free, PerNode, Premium, Standard, Standalone, Unlimited, and PerGB2018 (new Sku as of 2018-04-03). Defaults to PerGB2018. | `string` | `"PerGB2018"` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | n/a | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_artifact_identifier"></a> [artifact\_identifier](#output\_artifact\_identifier) | n/a |
| <a name="output_resource_id"></a> [resource\_id](#output\_resource\_id) | The identifier of the resource |
| <a name="output_resource_name"></a> [resource\_name](#output\_resource\_name) | The name of the resource |
| <a name="output_workspace_id"></a> [workspace\_id](#output\_workspace\_id) | The workspace ID of this Log Analytics Workspace |
<!-- END_TF_DOCS -->

## Example config

The following example shows how variables can be modified to deploy this module:

```terraform
variable "connectivity_subscription_id" {
  type        = string
  description = "Subscription Id which contains the Connectivity Hub"
}

provider "azurerm" {
  alias                      = "connectivity"
  subscription_id            = var.connectivity_subscription_id
  features {}
}

module "log_analytics_workspace" {
  source = "./log_analytics_workspace"

  component           = "demo"
  env                 = "prod"
  deployment_index    = "01"
  resource_group_name = "rg-demo-deployments"
  location            = "switzerlandnorth"
}
```
