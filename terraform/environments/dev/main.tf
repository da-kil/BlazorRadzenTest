# Development Environment Main Configuration
terraform {
  required_version = ">= 1.9.0"

  backend "azurerm" {
    # Backend configuration should be provided via backend-dev.hcl
  }
}

# Local variables
locals {
  environment = "dev"
  project_name = "beachbreak"
  location = "westeurope"

  common_tags = {
    Environment = "Development"
    Project     = "BeachBreak"
    ManagedBy   = "Terraform"
    Owner       = "Platform Team"
    CostCenter  = "Engineering"
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.project_name}-${local.environment}"
  location = local.location
  tags     = local.common_tags
}

# Monitoring Module (created first as it's needed by other modules)
module "monitoring" {
  source              = "../../modules/monitoring"
  project_name        = local.project_name
  environment         = local.environment
  location            = local.location
  resource_group_name = azurerm_resource_group.main.name
  key_vault_id        = module.keyvault.id

  log_retention_in_days = 30
  enable_alerts         = true

  alert_email_receivers = [
    {
      name          = "DevTeam"
      email_address = "devteam@ti8m.com"  # Replace with actual email
    }
  ]

  aks_cluster_id       = module.aks.cluster_id
  postgresql_server_id = module.postgresql.server_id

  tags = local.common_tags
}

# Networking Module
module "networking" {
  source              = "../../modules/networking"
  project_name        = local.project_name
  environment         = local.environment
  location            = local.location
  resource_group_name = azurerm_resource_group.main.name

  vnet_address_space                     = "10.0.0.0/16"
  aks_subnet_address_prefix              = "10.0.1.0/24"
  postgresql_subnet_address_prefix       = "10.0.2.0/24"
  private_endpoints_subnet_address_prefix = "10.0.3.0/24"

  enable_app_gateway = false

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
  public_network_access_enabled = true
  zone_redundancy_enabled       = false  # Dev environment
  enable_private_endpoint       = false  # Dev environment
  enable_defender               = false  # Dev environment

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

  sku_name                    = "standard"
  soft_delete_retention_days  = 7  # Dev environment
  purge_protection_enabled    = false  # Dev environment
  enable_rbac_authorization   = true
  default_network_action      = "Allow"  # Dev environment - more permissive
  enable_private_endpoint     = false  # Dev environment

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
  availability_zones  = ["1"]  # Dev - single zone for cost savings

  # System node pool (smaller for dev)
  system_node_vm_size   = "Standard_D2s_v5"
  system_node_count     = 1
  system_node_min_count = 1
  system_node_max_count = 3

  # Application node pool
  app_node_vm_size   = "Standard_D4s_v5"
  app_node_count     = 1
  app_node_min_count = 1
  app_node_max_count = 5

  # Frontend node pool
  frontend_node_vm_size   = "Standard_D2s_v5"
  frontend_node_count     = 1
  frontend_node_min_count = 1
  frontend_node_max_count = 3

  dns_service_ip = "10.1.0.10"
  service_cidr   = "10.1.0.0/16"

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

  enable_azure_policy = false  # Dev environment
  enable_defender     = false  # Dev environment

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
  sku_name           = "B_Standard_B2s"  # Dev - smaller instance
  storage_mb         = 32768  # 32 GB for dev
  storage_tier       = "P10"

  zone                         = "1"
  backup_retention_days        = 7
  geo_redundant_backup_enabled = false
  high_availability_mode       = ""  # Dev - no HA

  max_connections = "100"  # Dev - fewer connections needed
  shared_buffers  = "131072"  # 1GB
  work_mem        = "4096"  # 4MB

  tags = local.common_tags

  depends_on = [module.networking, module.keyvault]
}
