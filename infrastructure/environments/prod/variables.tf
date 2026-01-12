# Variable definitions for ti8m BeachBreak development environment
# Imports shared variables and adds environment-specific ones

#=============================================================================
# SHARED VARIABLES (from ../../shared/variables.tf)
#=============================================================================

variable "environment" {
  description = "The deployment environment (dev, test, prod)"
  type        = string
  default     = "dev"
  validation {
    condition     = var.environment == "dev"
    error_message = "This configuration is specifically for the development environment."
  }
}

variable "location" {
  description = "Azure region for resource deployment"
  type        = string
  default     = "switzerlandnorth"
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

#=============================================================================
# AKS CONFIGURATION
#=============================================================================

variable "aks_config" {
  description = "AKS cluster configuration for development"
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
  default = {
    sku_tier           = "Standard"
    kubernetes_version = "1.29"
    default_node_pool = {
      vm_size            = "Standard_D2s_v5"
      node_count         = 2
      min_count         = 1
      max_count         = 3
      availability_zones = ["1"]
    }
    additional_node_pools = {}
  }
}

#=============================================================================
# POSTGRESQL CONFIGURATION
#=============================================================================

variable "postgres_config" {
  description = "PostgreSQL flexible server configuration for development"
  type = object({
    sku_name                       = string
    storage_mb                     = number
    backup_retention_days          = number
    geo_redundant_backup_enabled   = bool
    high_availability_enabled      = bool
  })
  default = {
    sku_name                       = "B_Standard_B1ms"  # Burstable tier for cost optimization
    storage_mb                     = 32768              # 32 GB
    backup_retention_days          = 7                 # Shorter retention for dev
    geo_redundant_backup_enabled   = false             # No geo-redundancy for dev
    high_availability_enabled      = false             # No HA for dev
  }
}

#=============================================================================
# STORAGE CONFIGURATION
#=============================================================================

variable "storage_config" {
  description = "Storage account configuration for development"
  type = object({
    account_tier             = string
    account_replication_type = string
    account_kind            = string
  })
  default = {
    account_tier             = "Standard"
    account_replication_type = "LRS"         # Locally redundant for cost optimization
    account_kind            = "StorageV2"
  }
}

#=============================================================================
# CONTAINER REGISTRY CONFIGURATION
#=============================================================================

variable "container_registry_config" {
  description = "Container registry configuration for development"
  type = object({
    sku                     = string
    geo_replication_enabled = bool
  })
  default = {
    sku                     = "Basic"        # Basic tier for cost optimization
    geo_replication_enabled = false         # No geo-replication for dev
  }
}

#=============================================================================
# KEY VAULT CONFIGURATION
#=============================================================================

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

#=============================================================================
# MONITORING CONFIGURATION
#=============================================================================

variable "monitoring_config" {
  description = "Monitoring and logging configuration for development"
  type = object({
    log_analytics_sku = string
    retention_in_days = number
  })
  default = {
    log_analytics_sku = "PerGB2018"
    retention_in_days = 30              # Shorter retention for dev
  }
}

#=============================================================================
# APPLICATION INSIGHTS CONFIGURATION
#=============================================================================

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

#=============================================================================
# SECURITY CONFIGURATION
#=============================================================================

variable "admin_group_object_ids" {
  description = "Azure AD group object IDs for AKS cluster admin access"
  type        = list(string)
  default     = []
}

variable "allowed_ip_ranges" {
  description = "IP ranges allowed to access resources (for emergency access)"
  type        = list(string)
  default     = []
}

#=============================================================================
# COST MANAGEMENT
#=============================================================================

variable "budget_config" {
  description = "Budget and cost management configuration for development"
  type = object({
    enabled    = bool
    amount     = number
    currency   = string
    time_grain = string
    alert_thresholds = list(number)
  })
  default = {
    enabled = true
    amount  = 300                       # Lower budget for dev environment
    currency = "CHF"
    time_grain = "Monthly"
    alert_thresholds = [80, 100]
  }
}

#=============================================================================
# TAGS
#=============================================================================

variable "additional_tags" {
  description = "Additional tags to apply to all resources"
  type        = map(string)
  default = {
    Environment   = "Development"
    CostOptimized = "true"
    AutoShutdown  = "enabled"
  }
}

#=============================================================================
# DEVELOPMENT-SPECIFIC CONFIGURATIONS
#=============================================================================

variable "enable_development_features" {
  description = "Enable development-specific features"
  type = object({
    auto_shutdown_enabled    = optional(bool, true)
    debug_logging_enabled    = optional(bool, true)
    allow_http_traffic      = optional(bool, false)  # Still enforce HTTPS only
    skip_backup_policies    = optional(bool, true)   # Skip backup policies for dev
  })
  default = {
    auto_shutdown_enabled    = true
    debug_logging_enabled    = true
    allow_http_traffic      = false
    skip_backup_policies    = true
  }
}

variable "developer_access_config" {
  description = "Developer access configuration"
  type = object({
    enable_jumpbox          = optional(bool, false)  # Jumpbox for development access
    enable_public_endpoints = optional(bool, false)  # Keep private endpoints only
    enable_dev_tools       = optional(bool, true)    # Additional development tools
  })
  default = {
    enable_jumpbox          = false
    enable_public_endpoints = false
    enable_dev_tools       = true
  }
}

#=============================================================================
# BACKUP CONFIGURATION
#=============================================================================

variable "backup_config" {
  description = "Backup configuration for development resources"
  type = object({
    enabled           = bool
    retention_days    = number
    frequency         = string
  })
  default = {
    enabled        = false              # Disable backups for dev to save cost
    retention_days = 7                  # Short retention if enabled
    frequency      = "Daily"
  }
}