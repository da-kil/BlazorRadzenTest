# terraform-azurerm-Key-Vault
This module is a wrapper for azurerm Key Vault. The module also includes the deployment of a Private Endpoint as well as, an optional 
deployment of the corresponding private DNS Zone.

## Integration 
The integration is designed for [Enterprise Scale](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/enterprise-scale/). But also an integration in Hub -> Spoke is possible, as long as there is a separate connectivity subscription / env.

## Resources included 
- Key Vault: _itself_
- Role Assignment: _Key Vault Admin for the Terraform Service Principal_
- Private Endpoint: _Private connection to the given VNET_

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
| [azurerm_key_vault.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault) | resource |
| [azurerm_monitor_diagnostic_setting.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |
| [azurerm_private_endpoint.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_endpoint) | resource |
| [azurerm_role_assignment.key_vault_administrator](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) | resource |
| [azurerm_role_assignment.screts_users](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) | resource |
| [azurerm_client_config.current](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/client_config) | data source |
| [azurerm_private_dns_zone.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_allowed_ips"></a> [allowed\_ips](#input\_allowed\_ips) | Ips that are allowed to access this KV | `set(string)` | `null` | no |
| <a name="input_central_dns_zone_resource_group_name"></a> [central\_dns\_zone\_resource\_group\_name](#input\_central\_dns\_zone\_resource\_group\_name) | Name of the external private DNS zone that should be used to register private endpoint A record | `string` | n/a | yes |
| <a name="input_component"></a> [component](#input\_component) | Name of the application component, will be used on resource naming | `string` | n/a | yes |
| <a name="input_deployment_index"></a> [deployment\_index](#input\_deployment\_index) | Index counter of this instance, will be used on resource naming | `string` | `"01"` | no |
| <a name="input_diagnostics"></a> [diagnostics](#input\_diagnostics) | Settings required to configure the log analytics integration. Specify the log\_analytics\_workspace\_id property for the target Log Analytics Workspace.<br/>To enable specific log category groups or categories, check the azure docs for specifics: https://learn.microsoft.com/en-us/azure/azure-monitor/platform/diagnostic-settings?WT.mc_id=Portal-Microsoft_Azure_Monitoring&tabs=portal#category-groups | <pre>object({<br/>    log_analytics_workspace_id = string<br/>    log_metrics                = optional(list(string), [])<br/>    log_category_groups        = optional(list(string), ["allLogs"])<br/>    log_categories             = optional(list(string), [])<br/>  })</pre> | `null` | no |
| <a name="input_enable_rbac_authorization"></a> [enable\_rbac\_authorization](#input\_enable\_rbac\_authorization) | n/a | `bool` | `true` | no |
| <a name="input_env"></a> [env](#input\_env) | The deployment environment name (dev / test / prod) | `string` | n/a | yes |
| <a name="input_location"></a> [location](#input\_location) | n/a | `string` | `"switzerlandnorth"` | no |
| <a name="input_private_endpoint_subnet_id"></a> [private\_endpoint\_subnet\_id](#input\_private\_endpoint\_subnet\_id) | Network rules | `string` | `null` | no |
| <a name="input_resource_group_name"></a> [resource\_group\_name](#input\_resource\_group\_name) | n/a | `string` | n/a | yes |
| <a name="input_resource_name_infix"></a> [resource\_name\_infix](#input\_resource\_name\_infix) | Infix for the resource name to make it unique | `string` | n/a | yes |
| <a name="input_secret_users"></a> [secret\_users](#input\_secret\_users) | n/a | `map(string)` | `{}` | no |
| <a name="input_sku"></a> [sku](#input\_sku) | n/a | `string` | `"standard"` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | n/a | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_artifact_identifier"></a> [artifact\_identifier](#output\_artifact\_identifier) | Identifier of the deployed blueprint |
| <a name="output_resource_id"></a> [resource\_id](#output\_resource\_id) | ID of the created key vault |
| <a name="output_resource_name"></a> [resource\_name](#output\_resource\_name) | Name of the created key vault |
<!-- END_TF_DOCS -->

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

module "key_vault" {
  source = "./key_vault"

  providers = {
    azurerm              = azurerm
    azurerm.connectivity = azurerm.connectivity
  }

  component           = "demo"
  env                 = "test"
  deployment_index    = "01"
  resource_group_name = "rg-demo-deployments"

  diagnostics = {
    log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    logMetrics                 = ["AllMetrics"]
    logCategories              = ["all"]
  }

  private_endpoint_subnet_id           = data.azurerm_subnet.private_endpoint.id
  central_dns_zone_resource_group_name = "rg-dns-zones"
  resource_name_infix                  = random_string.this.result
}
```
