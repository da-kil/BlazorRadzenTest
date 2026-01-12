# Shared variable definitions for ti8m BeachBreak infrastructure
# These variables are used across all environments (dev, prod)

variable "environment" {
  description = "The deployment environment (dev, test, prod)"
  type        = string
  validation {
    condition     = contains(["dev", "test", "prod"], var.environment)
    error_message = "Environment must be one of: dev, test, prod."
  }
}

variable "location" {
  description = "Azure region for resource deployment"
  type        = string
  default     = "switzerlandnorth"
  validation {
    condition = contains([
      "switzerlandnorth",
      "switzerlandwest",
      "northeurope",
      "westeurope"
    ], var.location)
    error_message = "Location must be a supported European region."
  }
}

variable "deployment_index" {
  description = "Deployment instance index (01, 02, etc.)"
  type        = string
  default     = "01"
  validation {
    condition     = can(regex("^[0-9]{2}$", var.deployment_index))
    error_message = "Deployment index must be a two-digit number (e.g., '01', '02')."
  }
}

# AKS Configuration
variable "aks_config" {
  description = "AKS cluster configuration"
  type = object({
    sku_tier           = string
    kubernetes_version = string
    default_node_pool = object({
      vm_size            = string
      node_count         = number
      min_count         = number
      max_count         = number
      availability_zones = list(string)
    })
    additional_node_pools = optional(map(object({
      vm_size            = string
      node_count         = number
      min_count         = number
      max_count         = number
      availability_zones = optional(list(string), ["1"])
    })), {})
  })
}

# PostgreSQL Configuration
variable "postgres_config" {
  description = "PostgreSQL flexible server configuration"
  type = object({
    sku_name                       = string
    storage_mb                     = number
    backup_retention_days          = number
    geo_redundant_backup_enabled   = bool
    high_availability_enabled      = bool
  })
}

# Storage Account Configuration
variable "storage_config" {
  description = "Storage account configuration"
  type = object({
    account_tier             = string
    account_replication_type = string
    account_kind            = string
  })
}

# Container Registry Configuration
variable "container_registry_config" {
  description = "Container registry configuration"
  type = object({
    sku                     = string
    geo_replication_enabled = bool
  })
}

# Key Vault Configuration
variable "key_vault_config" {
  description = "Key Vault configuration"
  type = object({
    sku                     = string
    purge_protection_enabled = optional(bool, true)
    soft_delete_retention_days = optional(number, 30)
  })
  default = {
    sku = "standard"
    purge_protection_enabled = true
    soft_delete_retention_days = 30
  }
}

# Monitoring Configuration
variable "monitoring_config" {
  description = "Monitoring and logging configuration"
  type = object({
    log_analytics_sku = string
    retention_in_days = number
  })
  default = {
    log_analytics_sku = "PerGB2018"
    retention_in_days = 30
  }
}

# Network Configuration
variable "allowed_ip_ranges" {
  description = "IP ranges allowed to access resources (for emergency access)"
  type        = list(string)
  default     = []
}

# Security Configuration
variable "admin_group_object_ids" {
  description = "Azure AD group object IDs for AKS cluster admin access"
  type        = list(string)
  default     = []
}

# Application Configuration
variable "application_insights_config" {
  description = "Application Insights configuration"
  type = object({
    application_type = string
    retention_in_days = number
  })
  default = {
    application_type = "web"
    retention_in_days = 90
  }
}

# Backup Configuration
variable "backup_config" {
  description = "Backup configuration for resources"
  type = object({
    enabled           = bool
    retention_days    = number
    frequency         = string
  })
  default = {
    enabled        = true
    retention_days = 30
    frequency      = "Daily"
  }
}

# Cost Management
variable "budget_config" {
  description = "Budget and cost management configuration"
  type = object({
    enabled    = bool
    amount     = number
    currency   = string
    time_grain = string
    alert_thresholds = list(number)
  })
  default = {
    enabled = true
    amount  = 500
    currency = "CHF"
    time_grain = "Monthly"
    alert_thresholds = [80, 100]
  }
}

# Tags
variable "additional_tags" {
  description = "Additional tags to apply to all resources"
  type        = map(string)
  default     = {}
}