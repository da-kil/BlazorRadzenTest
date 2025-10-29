# Production Environment Main Configuration
terraform {
  required_version = ">= 1.9.0"

  backend "azurerm" {
    # Backend configuration should be provided via backend-production.hcl
  }
}

# Local variables
locals {
  environment = "production"
  project_name = "beachbreak"
  location = "westeurope"

  common_tags = {
    Environment = "Production"
    Project     = "BeachBreak"
    ManagedBy   = "Terraform"
    Owner       = "Platform Team"
    CostCenter  = "Production"
    Criticality = "High"
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.project_name}-${local.environment}"
  location = local.location
  tags     = local.common_tags
}

# Monitoring Module
module "monitoring" {
  source              = "../../modules/monitoring"
  project_name        = local.project_name
  environment         = local.environment
  location            = local.location
  resource_group_name = azurerm_resource_group.main.name
  key_vault_id        = module.keyvault.id

  log_retention_in_days = 90  # Production - longer retention
  enable_alerts         = true

  alert_email_receivers = [
    {
      name          = "OpsTeam"
      email_address = "ops@ti8m.com"  # Replace with actual email
    },
    {
      name          = "OnCall"
      email_address = "oncall@ti8m.com"  # Replace with actual email
    }
  ]

  # Optional: Add webhook for PagerDuty or similar
  # alert_webhook_receivers = [
  #   {
  #     name        = "PagerDuty"
  #     service_uri = "https://events.pagerduty.com/integration/..."
  #   }
  # ]

  aks_cluster_id       = module.aks.cluster_id
  postgresql_server_id = module.postgresql.server_id

  enable_smart_detection = true
  smart_detection_email_recipients = ["ops@ti8m.com"]

  tags = local.common_tags
}

# Networking Module
module "networking" {
  source              = "../../modules/networking"
  project_name        = local.project_name
  environment         = local.environment
  location            = local.location
  resource_group_name = azurerm_resource_group.main.name

  vnet_address_space                     = "10.2.0.0/16"  # Production uses different range
  aks_subnet_address_prefix              = "10.2.1.0/24"
  postgresql_subnet_address_prefix       = "10.2.2.0/24"
  private_endpoints_subnet_address_prefix = "10.2.3.0/24"

  enable_app_gateway = false  # Can enable for production WAF

  tags = local.common_tags
}

# Container Registry Module
module "acr" {
  source              = "../../modules/acr"
  project_name        = local.project_name
  environment         = local.environment
  location            = local.location
  resource_group_name = azurerm_resource_group.main.name

  sku                           = "Premium"
  admin_enabled                 = false
  public_network_access_enabled = false  # Production - private only
  zone_redundancy_enabled       = true   # Production - HA
  enable_private_endpoint       = true
  private_endpoint_subnet_id    = module.networking.private_endpoints_subnet_id
  enable_defender               = true
  retention_policy_in_days      = 30
  trust_policy_enabled          = true
  quarantine_policy_enabled     = true

  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = local.common_tags
}

# Key Vault Module
module "keyvault" {
  source              = "../../modules/keyvault"
  project_name        = local.project_name
  environment         = local.environment
  location            = local.location
  resource_group_name = azurerm_resource_group.main.name

  sku_name                    = "premium"  # Production - HSM backed
  soft_delete_retention_days  = 90
  purge_protection_enabled    = true
  enable_rbac_authorization   = true
  default_network_action      = "Deny"  # Production - restrictive
  enable_private_endpoint     = true
  private_endpoint_subnet_id  = module.networking.private_endpoints_subnet_id

  allowed_subnet_ids = [module.networking.aks_subnet_id]

  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = local.common_tags
}

# AKS Module
module "aks" {
  source              = "../../modules/aks"
  project_name        = local.project_name
  environment         = local.environment
  location            = local.location
  resource_group_name = azurerm_resource_group.main.name

  aks_subnet_id              = module.networking.aks_subnet_id
  vnet_id                    = module.networking.vnet_id
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id
  acr_id                     = module.acr.id

  kubernetes_version  = "1.29"
  availability_zones  = ["1", "2", "3"]  # Production - multi-zone

  # System node pool (production sizing)
  system_node_vm_size   = "Standard_D4s_v5"
  system_node_count     = 3
  system_node_min_count = 3
  system_node_max_count = 5

  # Application node pool (production sizing)
  app_node_vm_size   = "Standard_D8s_v5"
  app_node_count     = 3
  app_node_min_count = 3
  app_node_max_count = 10

  # Frontend node pool (production sizing)
  frontend_node_vm_size   = "Standard_D4s_v5"
  frontend_node_count     = 3
  frontend_node_min_count = 3
  frontend_node_max_count = 8

  dns_service_ip = "10.3.0.10"
  service_cidr   = "10.3.0.0/16"

  tags = local.common_tags

  depends_on = [module.acr]
}

# Security Module
module "security" {
  source              = "../../modules/security"
  project_name        = local.project_name
  environment         = local.environment
  location            = local.location
  resource_group_name = azurerm_resource_group.main.name
  resource_group_id   = azurerm_resource_group.main.id

  key_vault_id             = module.keyvault.id
  enable_workload_identity = true
  aks_oidc_issuer_url      = module.aks.oidc_issuer_url
  kubernetes_namespace     = "beachbreak"

  enable_azure_policy = true
  enable_defender     = true

  tags = local.common_tags

  depends_on = [module.aks, module.keyvault]
}

# PostgreSQL Module
module "postgresql" {
  source              = "../../modules/postgresql"
  project_name        = local.project_name
  environment         = local.environment
  location            = local.location
  resource_group_name = azurerm_resource_group.main.name

  postgresql_subnet_id       = module.networking.postgresql_subnet_id
  private_dns_zone_id        = module.networking.postgresql_private_dns_zone_id
  key_vault_id               = module.keyvault.id
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id

  postgresql_version = "16"
  sku_name           = "GP_Standard_D8s_v3"  # Production sizing
  storage_mb         = 262144  # 256 GB
  storage_tier       = "P40"

  zone                         = "1"
  standby_availability_zone    = "2"
  backup_retention_days        = 35
  geo_redundant_backup_enabled = true
  high_availability_mode       = "ZoneRedundant"  # Production HA

  max_connections = "400"
  shared_buffers  = "2097152"  # 16GB
  work_mem        = "20480"  # 20MB

  tags = local.common_tags

  depends_on = [module.networking, module.keyvault]
}
