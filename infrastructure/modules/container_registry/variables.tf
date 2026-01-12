#
# GENERAL
#
variable "env" {
  type        = string
  description = "The deployment environment name"
}

variable "deployment_index" {
  type        = string
  description = "Index counter of this instance"
}

variable "component" {
  type = string

  validation {
    condition     = length(var.component) < 25
    error_message = "Maximum length for component is 25 characters to avoid reaching naming limit for Container Registry"
  }
}

variable "location" {
  type        = string
  default     = "switzerlandnorth"
  description = "Deployment Region"
}

variable "resource_group_name" {
  type        = string
  description = "Name of the Resource Group the Storage Account should be deployed in"
}

variable "resource_name_infix" {
  type        = string
  description = "Infix for the resource name to make it unique. If left empty, a random string will be generated"
  nullable    = true
  default     = ""

  validation {
    condition     = length(var.resource_name_infix) <= 3
    error_message = "Random resource name infix can not be longer than 3 characters"
  }
}

variable "tags" {
  type        = map(string)
  default     = {}
  description = "Azure resource Tags"
}

variable "diagnostics" {
  description = <<DESCRIPTION
Settings required to configure the log analytics integration. Specify the log_analytics_workspace_id property for the target Log Analytics Workspace.
To enable specific log category groups or categories, check the azure docs for specifics: https://learn.microsoft.com/en-us/azure/azure-monitor/platform/diagnostic-settings?WT.mc_id=Portal-Microsoft_Azure_Monitoring&tabs=portal#category-groups
DESCRIPTION
  type = object({
    log_analytics_workspace_id = string
    log_metrics                = optional(list(string), [])
    log_category_groups        = optional(list(string), [])
    log_categories             = optional(list(string), [])
  })
  default = null
}

variable "geo_replications" {
  description = "A list of location where the container registry should be geo-replicated to, as well as whether zone redundancy is enabled for this replicatied location."
  type = list(object({
    location                = string
    zone_redundancy_enabled = bool
  }))
  default = []
}

variable "export_policy_enabled" {
  description = "Whether export policy is enabled for this Container Registry. Setting this to true, requires a Policy exemption."
  type        = bool
  default     = false
}

variable "zone_redundancy_enabled" {
  description = "Whether zone redundancy is enabled for this Container Registry. Should be set to `true` for production deployments."
  type        = bool
  default     = true
}

variable "trust_policy_enabled" {
  description = <<DESCRIPTION
  Whether trust policy is enabled for this Container Registry. If set to `true` clients with content trust enabled, will only be able to see and pull signed images.
  See [container-registry-content-trust](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-content-trust#how-content-trust-works) for more details.
DESCRIPTION
  type        = bool
  default     = false
}

variable "quarantine_policy_enabled" {
  description = "Whether quarantine policy is enabled for this Container Registry."
  type        = bool
  default     = true
}

variable "user_assigned_identity_id" {
  description = "ID of the UMI that will be used for KeyVault Access in case of CMK is used."
  type        = string
  default     = null
}

variable "anonymous_pull_enabled" {
  description = "Whether anonymous pull on this registry is enabled. Setting this to `true` requires a Policy exemption"
  type        = bool
  default     = false
}

#
# Network rules
#
variable "private_endpoint_subnet_id" {
  type        = string
  description = "Private endpoint will be deployed by default. If set to null ipWhitelists need to be set. This will only work with Policy exemptions."
}

variable "private_endpoint_location" {
  type        = string
  description = "Location of the private endpoint subnet"
  default     = "switzerlandnorth"
}

variable "network_ips_to_whitelist" {
  type        = list(string)
  default     = []
  description = "IPs to whitelist. WARNING! Setting this config will block the deployment due to azure policy setting. Policy exemption needs to be requested first"
}

variable "public_network_access_enabled" {
  type        = bool
  default     = false
  description = "Whether public network access is allowed for this container registry. Requires Azure Policy exemption if set to `true`."
}

variable "private_endpoint_ip_addresses" {
  type = object({
    registry                       = string
    registry_data_switzerlandnorth = string
  })
  description = "Manually specify the private endpoint IP addresses of the registry and data endpoint. Must match the address space of `var.privateEndpointSubnetId`."
  default     = null
}

variable "central_dns_zone_resource_group_name" {
  type        = string
  description = "Name of the external private DNS zone that should be used to register private endpoint A record"
}
#
# Cache rule
#
variable "container_registry_cache_rules" {
  type = list(object({
    name              = string
    targetRepo        = string
    sourceRepo        = string
    credentialSetName = optional(string)
  }))
  default     = []
  description = "List of objects containing the `source` and `target` repo of with Images should be imported / cached."
}

variable "container_registry_credential_sets" {
  type = list(object({
    name             = string
    loginServer      = string
    usernameSecretId = string
    passwordSecretId = string
  }))
  default     = []
  description = <<DESCRIPTION
  "List of objects containing the login credentials of a foreign container registry. Used in conjunction with containerRegistryCacheRules. <br/>
  **IMPORTANT!:** `usernameSecretId` and `passwordSecretId` need to be the **versionless_id** of the key vault secret"
DESCRIPTION
}

#
# Encryption
#
variable "acr_encryption_user_identity_id" {
  type        = string
  default     = null
  description = "The Client ID of the user assigned managed identity that has key access on the Key Vault."
}

variable "acr_encryption_key_vault_id" {
  type        = string
  default     = null
  description = "The ID of the Key Vault that contains the encryption key."
}
