#
# GENERAL
#
variable "env" {
  type        = string
  description = "The deployment environment name (dev / test / prod)"
}

variable "deployment_index" {
  type        = string
  default     = "01"
  description = "Index counter of this instance, will be used on resource naming"
}

variable "component" {
  type        = string
  description = "Name of the application component, will be used on resource naming"
}

variable "resource_name_infix" {
  type        = string
  description = "Infix for the resource name to make it unique"

  validation {
    condition     = length(var.resource_name_infix) <= 3
    error_message = "Random resource name infix can not be longer than 3 characters"
  }
}

variable "location" {
  type    = string
  default = "switzerlandnorth"
}

variable "resource_group_name" {
  type = string
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

variable "enable_rbac_authorization" {
  type    = bool
  default = true
}

variable "sku" {
  type    = string
  default = "standard"
}

#
# Network rules
#
variable "private_endpoint_subnet_id" {
  type     = string
  nullable = true
  default  = null
}

variable "central_dns_zone_resource_group_name" {
  type        = string
  description = "Name of the external private DNS zone that should be used to register private endpoint A record"
}

variable "allowed_ips" {
  type        = set(string)
  description = "Ips that are allowed to access this KV"
  default     = null
}

variable "secret_users" {
  type    = map(string)
  default = {}
}
