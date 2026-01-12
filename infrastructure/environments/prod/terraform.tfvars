# Production environment configuration values for ti8m BeachBreak

#=============================================================================
# BASIC CONFIGURATION
#=============================================================================

environment      = "prod"
location        = "switzerlandnorth"
deployment_index = "01"

#=============================================================================
# AKS CONFIGURATION - Production Scale
#=============================================================================

aks_config = {
  sku_tier           = "Standard"
  kubernetes_version = "1.29"

  # Production node pool configuration
  default_node_pool = {
    vm_size            = "Standard_D4s_v5"  # 4 vCPUs, 16 GB RAM - production scale
    node_count         = 3                  # Higher baseline for production
    min_count         = 2                  # Minimum 2 nodes for availability
    max_count         = 10                 # Higher maximum for production loads
    availability_zones = ["1"]             # Single zone for cost optimization
  }

  # Additional workload node pools for production
  additional_node_pools = {
    workload = {
      vm_size            = "Standard_D2s_v5"  # 2 vCPUs, 8 GB RAM for workloads
      node_count         = 2                  # Start with 2 workload nodes
      min_count         = 1                  # Can scale down to 1
      max_count         = 5                  # Scale up to 5 for workloads
      availability_zones = ["1"]             # Single zone strategy
    }
  }
}

#=============================================================================
# POSTGRESQL CONFIGURATION - General Purpose
#=============================================================================

postgres_config = {
  sku_name                       = "GP_Standard_D2s_v3"  # General Purpose tier
  storage_mb                     = 65536                 # 64 GB storage
  backup_retention_days          = 30                    # 30 days retention
  geo_redundant_backup_enabled   = true                  # Geo-redundant backups
  high_availability_enabled      = false                 # Single zone strategy
}

#=============================================================================
# STORAGE CONFIGURATION - Zone Redundant
#=============================================================================

storage_config = {
  account_tier             = "Standard"
  account_replication_type = "ZRS"         # Zone Redundant Storage
  account_kind            = "StorageV2"
}

#=============================================================================
# CONTAINER REGISTRY CONFIGURATION - Premium Tier
#=============================================================================

container_registry_config = {
  sku                     = "Premium"      # Premium tier for production
  geo_replication_enabled = false         # Single zone strategy
}

#=============================================================================
# KEY VAULT CONFIGURATION - Premium with HSM
#=============================================================================

key_vault_config = {
  sku                        = "premium"   # Premium SKU for HSM support
  purge_protection_enabled   = true       # Critical for production
  soft_delete_retention_days = 30
}

#=============================================================================
# MONITORING CONFIGURATION - Extended Retention
#=============================================================================

monitoring_config = {
  log_analytics_sku = "PerGB2018"
  retention_in_days = 90                   # 90 days retention for production
}

application_insights_config = {
  application_type  = "web"
  retention_in_days = 365                  # 1 year retention for production
}

#=============================================================================
# SECURITY CONFIGURATION
#=============================================================================

# Azure AD admin groups for AKS access (add your production group object IDs)
admin_group_object_ids = [
  # Add your production Azure AD group object IDs here
  # Example: "12345678-1234-1234-1234-123456789012"
]

# Allowed IP ranges for emergency access (production office IPs)
allowed_ip_ranges = [
  # Add your production office IP ranges here
  # Example: "203.0.113.0/24"
]

#=============================================================================
# COST MANAGEMENT - Production Budget
#=============================================================================

budget_config = {
  enabled          = true
  amount          = 1000                   # 1000 CHF monthly budget for prod
  currency        = "CHF"
  time_grain      = "Monthly"
  alert_thresholds = [70, 85, 100]        # More granular alerts for prod
}

#=============================================================================
# PRODUCTION-SPECIFIC FEATURES
#=============================================================================

enable_development_features = {
  auto_shutdown_enabled    = false        # No auto-shutdown for production
  debug_logging_enabled    = false        # Reduced debug logging for performance
  allow_http_traffic      = false        # Enforce HTTPS only
  skip_backup_policies    = false        # Enable all backup policies
}

developer_access_config = {
  enable_jumpbox          = false        # No jumpbox for production
  enable_public_endpoints = false        # Keep private endpoints only
  enable_dev_tools       = false        # No dev tools in production
}

#=============================================================================
# BACKUP CONFIGURATION - Full Production Backup
#=============================================================================

backup_config = {
  enabled        = true                  # Enable backups for production
  retention_days = 30                    # 30 days retention
  frequency      = "Daily"               # Daily backups
}

#=============================================================================
# ADDITIONAL TAGS
#=============================================================================

additional_tags = {
  Environment     = "Production"
  CriticalSystem  = "true"
  BackupRequired  = "true"
  Monitoring      = "24x7"
  Owner          = "Production-Team"
  Purpose        = "BeachBreak-Application-Production"
  DataClass      = "Confidential"
}