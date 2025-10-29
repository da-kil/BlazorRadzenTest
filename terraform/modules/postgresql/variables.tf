# PostgreSQL Module Variables
variable "project_name" {
  description = "Project name for resource naming"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "postgresql_subnet_id" {
  description = "PostgreSQL subnet ID"
  type        = string
}

variable "private_dns_zone_id" {
  description = "Private DNS zone ID for PostgreSQL"
  type        = string
}

variable "key_vault_id" {
  description = "Key Vault ID for storing connection strings"
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "Log Analytics workspace ID for diagnostics"
  type        = string
}

variable "postgresql_version" {
  description = "PostgreSQL version"
  type        = string
  default     = "16"
}

variable "administrator_login" {
  description = "Administrator login name"
  type        = string
  default     = "psqladmin"
}

variable "sku_name" {
  description = "PostgreSQL SKU name (e.g., B_Standard_B2s, GP_Standard_D4s_v3)"
  type        = string
  default     = "GP_Standard_D4s_v3"
}

variable "storage_mb" {
  description = "Storage size in MB"
  type        = number
  default     = 131072  # 128 GB
}

variable "storage_tier" {
  description = "Storage tier (P4, P6, P10, P15, P20, P30, P40, P50)"
  type        = string
  default     = "P30"
}

variable "zone" {
  description = "Availability zone for primary server"
  type        = string
  default     = "1"
}

variable "backup_retention_days" {
  description = "Backup retention in days"
  type        = number
  default     = 35
}

variable "geo_redundant_backup_enabled" {
  description = "Enable geo-redundant backups"
  type        = bool
  default     = false
}

variable "high_availability_mode" {
  description = "High availability mode (ZoneRedundant or SameZone)"
  type        = string
  default     = "SameZone"
  validation {
    condition     = contains(["ZoneRedundant", "SameZone", ""], var.high_availability_mode)
    error_message = "High availability mode must be ZoneRedundant, SameZone, or empty string (disabled)."
  }
}

variable "standby_availability_zone" {
  description = "Availability zone for standby server (required for ZoneRedundant HA)"
  type        = string
  default     = "2"
}

variable "maintenance_window_day" {
  description = "Maintenance window day (0-6, Sunday-Saturday)"
  type        = number
  default     = 0  # Sunday
}

variable "maintenance_window_hour" {
  description = "Maintenance window start hour (0-23)"
  type        = number
  default     = 2
}

variable "maintenance_window_minute" {
  description = "Maintenance window start minute (0-59)"
  type        = number
  default     = 0
}

# Performance tuning parameters
variable "max_connections" {
  description = "Maximum number of connections"
  type        = string
  default     = "200"
}

variable "shared_buffers" {
  description = "Shared buffers (in 8KB blocks)"
  type        = string
  default     = "524288"  # 4GB
}

variable "work_mem" {
  description = "Work memory per operation (in KB)"
  type        = string
  default     = "10240"  # 10MB
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
