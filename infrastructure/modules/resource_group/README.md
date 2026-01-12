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
| [azurerm_resource_group.this](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/resource_group) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_component"></a> [component](#input\_component) | Name of the application component, will be used on resource naming | `string` | n/a | yes |
| <a name="input_deployment_index"></a> [deployment\_index](#input\_deployment\_index) | Index counter of this instance, will be used on resource naming | `string` | `"01"` | no |
| <a name="input_env"></a> [env](#input\_env) | The deployment environment name (dev / test / prod) | `string` | n/a | yes |
| <a name="input_location"></a> [location](#input\_location) | Deployment location | `string` | `"switzerlandnorth"` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Map of TAGs that will be added to the deployed resources | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_artifact_identifier"></a> [artifact\_identifier](#output\_artifact\_identifier) | Identifier of the deployed blueprint |
| <a name="output_resource_group_name"></a> [resource\_group\_name](#output\_resource\_group\_name) | Name of the created resource group |
<!-- END_TF_DOCS -->
