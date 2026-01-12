#
# GENERAL
#
variable "env" {
  type        = string
  description = "The deployment environment name, will be used for resource naming"
}

variable "deployment_index" {
  type        = string
  default     = "01"
  description = "The deployment index, will be used for resource naming"
}

variable "component" {
  type        = string
  description = "The app component name, will be used for resource naming"
}

variable "location" {
  type        = string
  default     = "switzerlandnorth"
  description = "Deployment location, will be used for resource naming"
}

variable "resource_group_name" {
  type        = string
  description = "The target resource group for the deployment"
}

variable "resource_name_infix" {
  type        = string
  description = "Random resource name infix."

  validation {
    condition     = length(var.resource_name_infix) <= 3
    error_message = "Random resource name infix can not be longer than 3 characters"
  }
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

variable "tags" {
  type    = map(string)
  default = {}
}

#
# SETTINGS
#
variable "sku" {
  description = "Specifies the Sku of the Log Analytics Workspace. Possible values are Free, PerNode, Premium, Standard, Standalone, Unlimited, and PerGB2018 (new Sku as of 2018-04-03). Defaults to PerGB2018."
  default     = "PerGB2018"
}

variable "retention_days" {
  description = "The workspace data retention in days. Possible values are either 7 (Free Tier only) or range between 30 and 730. Defaults to 30"
  default     = 30
}

variable "daily_quota_gb" {
  description = "The workspace daily quota for ingestion in GB. Defaults to -1 (unlimited). When sku is set to Free this field can be set to a maximum of 0.5 (GB)"
  default     = null
}

variable "allow_public_query_access" {
  description = "The network access type for accessing Log Analytics query. - Enabled or Disabled"
  type        = bool
  default     = false
}

variable "allow_public_ingestion_access" {
  description = "The network access type for accessing Log Analytics ingestion. - Enabled or Disabled"
  type        = bool
  default     = true
}

variable "local_auth_enabled" {
  description = "Whether LAW local authentication should be enabled."
  type        = bool
  default     = false
}
