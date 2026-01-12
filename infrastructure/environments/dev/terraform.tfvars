# Development environment configuration values for ti8m BeachBreak

#=============================================================================
# BASIC CONFIGURATION
#=============================================================================

environment      = "dev"
location        = "switzerlandnorth"
deployment_index = "01"

#=============================================================================
# AKS CONFIGURATION - Cost Optimized for Development
#=============================================================================

aks_config = {
  sku_tier           = "Standard"
  kubernetes_version = "1.29"

  # Single node pool configuration for development
  default_node_pool = {
    vm_size            = "Standard_D2s_v5"  # 2 vCPUs, 8 GB RAM - cost optimized
    node_count         = 2                  # Minimal nodes for development
    min_count         = 1                  # Can scale down to 1 node
    max_count         = 3                  # Maximum 3 nodes to control costs
    availability_zones = ["1"]             # Single zone for cost optimization
  }

  # No additional node pools for development
  additional_node_pools = {}
}

#=============================================================================
# POSTGRESQL CONFIGURATION - Burstable Tier
#=============================================================================

postgres_config = {
  sku_name                       = "B_Standard_B1ms"  # Burstable tier - cost effective
  storage_mb                     = 32768              # 32 GB storage
  backup_retention_days          = 7                 # 7 days retention for dev
  geo_redundant_backup_enabled   = false             # No geo-redundancy needed
  high_availability_enabled      = false             # No HA for development
}

#=============================================================================
# STORAGE CONFIGURATION - Locally Redundant
#=============================================================================

storage_config = {
  account_tier             = "Standard"
  account_replication_type = "LRS"         # Locally Redundant Storage
  account_kind            = "StorageV2"
}

#=============================================================================
# CONTAINER REGISTRY CONFIGURATION - Basic Tier
#=============================================================================

container_registry_config = {
  sku                     = "Basic"        # Basic tier for development
  geo_replication_enabled = false         # No geo-replication needed
}

#=============================================================================
# KEY VAULT CONFIGURATION
#=============================================================================

key_vault_config = {
  sku                        = "standard"
  purge_protection_enabled   = true       # Keep enabled for safety
  soft_delete_retention_days = 30
}

#=============================================================================
# MONITORING CONFIGURATION - Reduced Retention
#=============================================================================

monitoring_config = {
  log_analytics_sku = "PerGB2018"
  retention_in_days = 30                   # 30 days retention for development
}

application_insights_config = {
  application_type  = "web"
  retention_in_days = 90                   # 90 days for application insights
}

#=============================================================================
# SECURITY CONFIGURATION
#=============================================================================

# Azure AD admin groups for AKS access (add your group object IDs here)
admin_group_object_ids = [
  # Add your Azure AD group object IDs here
  # Example: "12345678-1234-1234-1234-123456789012"
]

# Allowed IP ranges for emergency access (add your office/home IPs here)
allowed_ip_ranges = [
  # Add your IP ranges here for emergency access
  # Example: "203.0.113.0/24"
]

#=============================================================================
# COST MANAGEMENT - Development Budget
#=============================================================================

budget_config = {
  enabled          = true
  amount          = 300                    # 300 CHF monthly budget for dev
  currency        = "CHF"
  time_grain      = "Monthly"
  alert_thresholds = [80, 100]            # Alert at 80% and 100%
}

#=============================================================================
# DEVELOPMENT-SPECIFIC FEATURES
#=============================================================================

enable_development_features = {
  auto_shutdown_enabled    = true         # Enable auto-shutdown for cost savings
  debug_logging_enabled    = true         # Enable debug logging
  allow_http_traffic      = false        # Still enforce HTTPS only
  skip_backup_policies    = true         # Skip backup policies to save costs
}

developer_access_config = {
  enable_jumpbox          = false        # No jumpbox needed for dev
  enable_public_endpoints = false        # Keep private endpoints only
  enable_dev_tools       = true         # Enable additional dev tools
}

#=============================================================================
# BACKUP CONFIGURATION - Disabled for Cost Savings
#=============================================================================

backup_config = {
  enabled        = false                 # Disable backups for development
  retention_days = 7                     # Short retention if enabled
  frequency      = "Daily"
}

#=============================================================================
# ADDITIONAL TAGS
#=============================================================================

additional_tags = {
  Environment     = "Development"
  CostOptimized   = "true"
  AutoShutdown    = "enabled"
  Owner          = "Development-Team"
  Purpose        = "BeachBreak-Application-Development"
}