#
# GENERAL
#
variable "env" {
  type        = string
  description = "The deployment environment name"
}

variable "deployment_index" {
  type = string
}

variable "component" {
  type = string

  validation {
    condition     = length(var.component) <= 5
    error_message = "Maximum length for component is 5 characters to avoid reaching naming limit for PostgreSQL"
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

variable "postgres_version" {
  description = "The version of this PostgreSQL Flexible Server"
  type        = string
  default     = 16
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

variable "sku" {
  description = "Specifies the SKU for this PostgresSQL"
  type        = string
  default     = "GP_Standard_D2s_v3"
}

variable "public_network_access_enabled" {
  description = "Whether public access is enabled. Setting this to `true` requires a security exemption"
  type        = bool
  default     = false
}

variable "deployment_zone" {
  description = "Azure Zone the PostgresDB should be deployed into. `IMPORTANT!:` can **ONLY** be set duing creation time."
  type        = string
  default     = 1
}

variable "identity_type" {
  description = "Specifies the identity type of the App Service. Possible values are: `SystemAssigned` (where Azure will generate a Service Principal for you), `UserAssigned` where you can specify the Service Principal IDs in the identity_ids field, and `SystemAssigned, UserAssigned` which assigns both a system managed identity as well as the specified user assigned identities."
  type        = string
  default     = "SystemAssigned"
  validation {
    condition = (
      var.identity_type != "None" && var.identity_type != null
    )
    error_message = "IdentityType variable can't be set to `None` or null."
  }
}

variable "identity_ids" {
  description = "Specifies a list of user managed identity ids to be assigned. Required if type is UserAssigned"
  type        = list(string)
  default     = null
}

variable "maintenance_window" {
  description = "The timeframe azure is allowed to do maintenance on the Postgres Server or hardware. This should be configured for prod env"
  type = object({
    dayOfWeek    = string
    startHour    = string
    start_minute = string
  })
  default = null
}

variable "password_auth_enabled" {
  description = "whether local accounts are enabled. Setting this to `true` requires a security exemption"
  type        = bool
  default     = false
}

variable "administrator_login" {
  description = "The local admin username. Configuring this only works in conjunction with `passwordAuthEnabled=true`"
  type        = string
  default     = null
}

variable "administrator_password" {
  description = "The local admin password. Configuring this only works in conjunction with `passwordAuthEnabled=true`"
  type        = string
  default     = null
}

#
# Storage
#
variable "auto_grow_enabled" {
  description = "Is the storage auto grow for PostgreSQL Flexible Server enabled"
  type        = bool
  default     = false
}

variable "storage_tier" {
  description = "The name of storage performance tier."
  type        = string
  default     = "P4"
}

variable "storage_in_mb" {
  description = "The max storage allowed for the PostgreSQL Flexible Server"
  type        = string
  default     = "32768"
}

variable "backup_retention_days" {
  description = "The backup retention days for the PostgreSQL Flexible Server"
  type        = number
  default     = 7
}

variable "geo_redundant_backup_enabled" {
  description = "Is Geo-Redundant backup enabled on the PostgreSQL Flexible Server. Backup will be stored in the switzerland west region"
  type        = bool
  default     = false
}

variable "high_availability" {
  description = "If provided, PostgreSQL Flexible Server will be deploy with a active replication to another Azure zone"
  type = object({
    mode                    = optional(string, "ZoneRedundant")
    standbyAvailabilityZone = optional(number, 2)
  })
  default = {}
}

#
# Networking
#
variable "delegated_sub_net_id" {
  description = "Resource ID of the Subnet that should be used for private conneciton"
  type        = string
}

variable "central_dns_zone_resource_group_name" {
  type        = string
  description = "Name of the external private DNS zone that should be used to register private endpoint A record"
}

#
# Encryption
#
variable "storage_encryption_key_vault_id" {
  type    = string
  default = null
}
