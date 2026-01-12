# Resource Groups
resource "azurerm_resource_group" "k8s" {
  name     = "rg-beachbreak-k8s-${var.env}-swn-01"
  location = var.location
  tags     = var.tags
}

resource "azurerm_resource_group" "db" {
  name     = "rg-beachbreak-db-${var.env}-swn-01"
  location = var.location
  tags     = var.tags
}

# Networking Module (VNet, subnets, DNS zones)
module "networking" {
  source = "../../modules/networking"

  env                 = var.env
  location            = var.location
  resource_group_name = azurerm_resource_group.k8s.name
  vnet_address_space  = ["10.10.0.0/16"] # Prod: Different address space

  tags = var.tags
}

# Identities Module (Managed identities for AKS)
module "identities" {
  source = "../../modules/identities"

  env                 = var.env
  location            = var.location
  resource_group_name = azurerm_resource_group.k8s.name
  vnet_id             = module.networking.vnet_id

  tags = var.tags
}

# Monitoring Module (Log Analytics, Application Insights)
module "monitoring" {
  source = "../../modules/monitoring"

  env                         = var.env
  location                    = var.location
  resource_group_name         = azurerm_resource_group.k8s.name
  log_analytics_retention_days = 90 # Prod: Longer retention

  tags = var.tags
}

# Key Vault Module
module "keyvault" {
  source = "../../modules/keyvault"

  env                        = var.env
  location                   = var.location
  resource_group_name        = azurerm_resource_group.k8s.name
  private_endpoint_subnet_id = module.networking.private_endpoint_subnet_id
  private_dns_zone_id        = module.networking.private_dns_zone_ids["keyvault"]
  sku                        = "premium" # Prod: Premium SKU for HSM-backed keys
  purge_protection_enabled   = true      # Prod: Enable purge protection
  diagnostics_workspace_id   = module.monitoring.log_analytics_workspace_id

  tags = var.tags
}

# Storage Account Module
module "storage" {
  source = "../../modules/storage"

  env                        = var.env
  location                   = var.location
  resource_group_name        = azurerm_resource_group.k8s.name
  private_endpoint_subnet_id = module.networking.private_endpoint_subnet_id
  private_dns_zone_id        = module.networking.private_dns_zone_ids["storage_blob"]
  account_tier               = "Standard"
  account_replication_type   = "ZRS" # Prod: Zone Redundant Storage
  containers                 = ["backups", "uploads", "logs"]
  diagnostics_workspace_id   = module.monitoring.log_analytics_workspace_id

  tags = var.tags
}

# PostgreSQL Module
module "postgresql" {
  source = "../../modules/postgresql"

  env                          = var.env
  location                     = var.location
  resource_group_name          = azurerm_resource_group.db.name
  delegated_subnet_id          = module.networking.postgresql_subnet_id
  private_dns_zone_id          = module.networking.private_dns_zone_ids["postgresql"]
  postgres_version             = var.postgresql_version
  sku_name                     = "GP_Standard_D4s_v3" # Prod: 4 vCPU, 16 GB RAM
  storage_mb                   = 262144               # Prod: 256 GB
  backup_retention_days        = 30                   # Prod: 30 days retention
  geo_redundant_backup_enabled = true                 # Prod: Geo-redundant backup
  high_availability_enabled    = true                 # Prod: Zone-redundant HA
  diagnostics_workspace_id     = module.monitoring.log_analytics_workspace_id

  tags = var.tags
}

# AKS Cluster Module
module "aks" {
  source = "../../modules/aks-cluster"

  env                = var.env
  location           = var.location
  resource_group_name = azurerm_resource_group.k8s.name

  # Identities
  cluster_identity_id           = module.identities.cluster_identity_id
  cluster_identity_principal_id = module.identities.cluster_identity_principal_id
  kubelet_identity_id           = module.identities.kubelet_identity_id
  kubelet_identity_principal_id = module.identities.kubelet_identity_principal_id
  kubelet_identity_client_id    = module.identities.cluster_identity.client_id

  # Networking
  vnet_id                   = module.networking.vnet_id
  aks_node_pool_subnet_id   = module.networking.aks_node_pool_subnet_id
  aks_api_server_subnet_id  = module.networking.aks_api_server_subnet_id

  # Configuration
  kubernetes_version = var.kubernetes_version

  # Default node pool (system) - Prod: Larger and zone-redundant
  default_node_pool_vm_size    = "Standard_D4s_v3" # Prod: 4 vCPU, 16 GB RAM
  default_node_pool_node_count = 3                  # Prod: 3 nodes for availability

  # User node pool (auto-scaling) - Prod: Larger capacity
  user_node_pool_vm_size  = "Standard_D8s_v3" # Prod: 8 vCPU, 32 GB RAM
  user_node_pool_min_count = 3
  user_node_pool_max_count = 10

  # Encryption
  etcd_encryption_key_id = module.keyvault.etcd_encryption_key_id

  # Monitoring
  diagnostics_workspace_id = module.monitoring.log_analytics_workspace_id

  tags = var.tags

  depends_on = [module.keyvault]
}

# Container Registry - Prod: Premium SKU
resource "azurerm_container_registry" "main" {
  name                = "acrbeachbreakprodswn01"
  resource_group_name = azurerm_resource_group.k8s.name
  location            = var.location
  sku                 = "Premium" # Prod: Premium SKU for geo-replication
  admin_enabled       = false

  network_rule_set {
    default_action = "Deny"
  }

  # Zone redundancy for premium SKU
  zone_redundancy_enabled = true

  tags = var.tags
}

# Private Endpoint for ACR
resource "azurerm_private_endpoint" "acr" {
  name                = "pe-${azurerm_container_registry.main.name}"
  location            = var.location
  resource_group_name = azurerm_resource_group.k8s.name
  subnet_id           = module.networking.private_endpoint_subnet_id

  private_service_connection {
    name                           = "psc-${azurerm_container_registry.main.name}"
    private_connection_resource_id = azurerm_container_registry.main.id
    is_manual_connection           = false
    subresource_names              = ["registry"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [module.networking.private_dns_zone_ids["acr"]]
  }

  tags = var.tags
}

# Role Assignment: Kubelet identity as AcrPull on Container Registry
resource "azurerm_role_assignment" "kubelet_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = module.identities.kubelet_identity_principal_id
}
