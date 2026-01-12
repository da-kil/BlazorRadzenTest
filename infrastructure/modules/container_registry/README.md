# RCH IaC Certified Azure Container Resgitry

## Features

This blueprint comes with the following features: 

- Encryption at rest (CMK)
- Private Endpoint
- Image Cache Rule

## Usage Scenarios

This blueprint can be used to deploy a Azure Container Registry with private endpoint: 

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.12.2 |
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
| [azurerm_container_registry.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/container_registry) | resource |
| [azurerm_container_registry_cache_rule.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/container_registry_cache_rule) | resource |
| [azurerm_container_registry_credential_set.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/container_registry_credential_set) | resource |
| [azurerm_monitor_diagnostic_setting.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_diagnostic_setting) | resource |
| [azurerm_private_endpoint.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_endpoint) | resource |
| [azurerm_private_dns_zone.azurecr](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_acr_encryption_key_vault_id"></a> [acr\_encryption\_key\_vault\_id](#input\_acr\_encryption\_key\_vault\_id) | The ID of the Key Vault that contains the encryption key. | `string` | `null` | no |
| <a name="input_acr_encryption_user_identity_id"></a> [acr\_encryption\_user\_identity\_id](#input\_acr\_encryption\_user\_identity\_id) | The Client ID of the user assigned managed identity that has key access on the Key Vault. | `string` | `null` | no |
| <a name="input_anonymous_pull_enabled"></a> [anonymous\_pull\_enabled](#input\_anonymous\_pull\_enabled) | Whether anonymous pull on this registry is enabled. Setting this to `true` requires a Policy exemption | `bool` | `false` | no |
| <a name="input_central_dns_zone_resource_group_name"></a> [central\_dns\_zone\_resource\_group\_name](#input\_central\_dns\_zone\_resource\_group\_name) | Name of the external private DNS zone that should be used to register private endpoint A record | `string` | n/a | yes |
| <a name="input_component"></a> [component](#input\_component) | n/a | `string` | n/a | yes |
| <a name="input_container_registry_cache_rules"></a> [container\_registry\_cache\_rules](#input\_container\_registry\_cache\_rules) | List of objects containing the `source` and `target` repo of with Images should be imported / cached. | <pre>list(object({<br/>    name              = string<br/>    targetRepo        = string<br/>    sourceRepo        = string<br/>    credentialSetName = optional(string)<br/>  }))</pre> | `[]` | no |
| <a name="input_container_registry_credential_sets"></a> [container\_registry\_credential\_sets](#input\_container\_registry\_credential\_sets) | "List of objects containing the login credentials of a foreign container registry. Used in conjunction with containerRegistryCacheRules. <br/><br/>  **IMPORTANT!:** `usernameSecretId` and `passwordSecretId` need to be the **versionless\_id** of the key vault secret" | <pre>list(object({<br/>    name             = string<br/>    loginServer      = string<br/>    usernameSecretId = string<br/>    passwordSecretId = string<br/>  }))</pre> | `[]` | no |
| <a name="input_deployment_index"></a> [deployment\_index](#input\_deployment\_index) | Index counter of this instance | `string` | n/a | yes |
| <a name="input_diagnostics"></a> [diagnostics](#input\_diagnostics) | Settings required to configure the log analytics integration. Specify the log\_analytics\_workspace\_id property for the target Log Analytics Workspace.<br/>To enable specific log category groups or categories, check the azure docs for specifics: https://learn.microsoft.com/en-us/azure/azure-monitor/platform/diagnostic-settings?WT.mc_id=Portal-Microsoft_Azure_Monitoring&tabs=portal#category-groups | <pre>object({<br/>    log_analytics_workspace_id = string<br/>    log_metrics                = optional(list(string), [])<br/>    log_category_groups        = optional(list(string), [])<br/>    log_categories             = optional(list(string), [])<br/>  })</pre> | `null` | no |
| <a name="input_env"></a> [env](#input\_env) | The deployment environment name | `string` | n/a | yes |
| <a name="input_export_policy_enabled"></a> [export\_policy\_enabled](#input\_export\_policy\_enabled) | Whether export policy is enabled for this Container Registry. Setting this to true, requires a Policy exemption. | `bool` | `false` | no |
| <a name="input_geo_replications"></a> [geo\_replications](#input\_geo\_replications) | A list of location where the container registry should be geo-replicated to, as well as whether zone redundancy is enabled for this replicatied location. | <pre>list(object({<br/>    location                = string<br/>    zone_redundancy_enabled = bool<br/>  }))</pre> | `[]` | no |
| <a name="input_location"></a> [location](#input\_location) | Deployment Region | `string` | `"switzerlandnorth"` | no |
| <a name="input_network_ips_to_whitelist"></a> [network\_ips\_to\_whitelist](#input\_network\_ips\_to\_whitelist) | IPs to whitelist. WARNING! Setting this config will block the deployment due to azure policy setting. Policy exemption needs to be requested first | `list(string)` | `[]` | no |
| <a name="input_private_endpoint_ip_addresses"></a> [private\_endpoint\_ip\_addresses](#input\_private\_endpoint\_ip\_addresses) | Manually specify the private endpoint IP addresses of the registry and data endpoint. Must match the address space of `var.privateEndpointSubnetId`. | <pre>object({<br/>    registry                       = string<br/>    registry_data_switzerlandnorth = string<br/>  })</pre> | `null` | no |
| <a name="input_private_endpoint_location"></a> [private\_endpoint\_location](#input\_private\_endpoint\_location) | Location of the private endpoint subnet | `string` | `"switzerlandnorth"` | no |
| <a name="input_private_endpoint_subnet_id"></a> [private\_endpoint\_subnet\_id](#input\_private\_endpoint\_subnet\_id) | Private endpoint will be deployed by default. If set to null ipWhitelists need to be set. This will only work with Policy exemptions. | `string` | n/a | yes |
| <a name="input_public_network_access_enabled"></a> [public\_network\_access\_enabled](#input\_public\_network\_access\_enabled) | Whether public network access is allowed for this container registry. Requires Azure Policy exemption if set to `true`. | `bool` | `false` | no |
| <a name="input_quarantine_policy_enabled"></a> [quarantine\_policy\_enabled](#input\_quarantine\_policy\_enabled) | Whether quarantine policy is enabled for this Container Registry. | `bool` | `true` | no |
| <a name="input_resource_group_name"></a> [resource\_group\_name](#input\_resource\_group\_name) | Name of the Resource Group the Storage Account should be deployed in | `string` | n/a | yes |
| <a name="input_resource_name_infix"></a> [resource\_name\_infix](#input\_resource\_name\_infix) | Infix for the resource name to make it unique. If left empty, a random string will be generated | `string` | `""` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Azure resource Tags | `map(string)` | `{}` | no |
| <a name="input_trust_policy_enabled"></a> [trust\_policy\_enabled](#input\_trust\_policy\_enabled) | Whether trust policy is enabled for this Container Registry. If set to `true` clients with content trust enabled, will only be able to see and pull signed images.<br/>  See [container-registry-content-trust](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-content-trust#how-content-trust-works) for more details. | `bool` | `false` | no |
| <a name="input_user_assigned_identity_id"></a> [user\_assigned\_identity\_id](#input\_user\_assigned\_identity\_id) | ID of the UMI that will be used for KeyVault Access in case of CMK is used. | `string` | `null` | no |
| <a name="input_zone_redundancy_enabled"></a> [zone\_redundancy\_enabled](#input\_zone\_redundancy\_enabled) | Whether zone redundancy is enabled for this Container Registry. Should be set to `true` for production deployments. | `bool` | `true` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_artifact_identifier"></a> [artifact\_identifier](#output\_artifact\_identifier) | n/a |
| <a name="output_credential_set_principal_ids"></a> [credential\_set\_principal\_ids](#output\_credential\_set\_principal\_ids) | The `principal_id` of the gendered System Identity for the `credential_set` cache rule. <br /> **IMPORTANT!** Needs to be permitted on the configured credential Key Vault. |
| <a name="output_login_server"></a> [login\_server](#output\_login\_server) | The URL that can be used to log into the container registry |
| <a name="output_resource_id"></a> [resource\_id](#output\_resource\_id) | n/a |
| <a name="output_resource_name"></a> [resource\_name](#output\_resource\_name) | n/a |
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
  resource_provider_registrations = "none"
  features {}
}

resource "random_string" "infix" {
  length  = 3
  special = false
  upper   = false
}

module "container_registry" {
  source = "./container_registry"

  providers = {
    azurerm              = azurerm
    azurerm.connectivity = azurerm.connectivity
  }

  component           = "demo"
  env                 = "prod"
  deployment_index    = "01"
  resource_group_name = "rg-demo-deployments"

  private_endpoint_subnet_id = data.azurerm_subnet.private_endpoint.id

  quarantine_policy_enabled = false

  # Need to have active login credentials
  container_registry_cache_rules = [{
    name       = "aks-managed-mcr"
    targetRepo = "aks-managed-repository/*"
    sourceRepo = "mcr.microsoft.com/*"
  }]

  diagnostics = {
    log_analytics_workspace_id = data.azurerm_log_analytics_workspace.ala_test.id
    log_categories             = ["ContainerRegistryLoginEvents"]
    log_metrics                = ["AllMetrics"]
  }
  central_dns_zone_resource_group_name = "rg-dns-zones"
}
```
