# Azure Container Registry Module
resource "azurerm_container_registry" "main" {
  name                = "acr${var.project_name}${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = var.sku
  admin_enabled       = var.admin_enabled

  # Zone redundancy for Premium SKU
  zone_redundancy_enabled = var.sku == "Premium" ? var.zone_redundancy_enabled : false

  # Network access
  public_network_access_enabled = var.public_network_access_enabled

  # Data encryption
  dynamic "encryption" {
    for_each = var.enable_encryption ? [1] : []
    content {
      enabled = true
    }
  }

  # Retention policy for untagged manifests
  retention_policy_in_days = var.retention_policy_in_days

  # Trust policy
  trust_policy_enabled = var.trust_policy_enabled

  # Quarantine policy (requires Premium SKU)
  quarantine_policy_enabled = var.sku == "Premium" ? var.quarantine_policy_enabled : false

  tags = var.tags
}

# Private endpoint for ACR (if enabled)
resource "azurerm_private_endpoint" "acr" {
  count               = var.enable_private_endpoint ? 1 : 0
  name                = "pe-${azurerm_container_registry.main.name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "psc-${azurerm_container_registry.main.name}"
    private_connection_resource_id = azurerm_container_registry.main.id
    subresource_names              = ["registry"]
    is_manual_connection           = false
  }

  tags = var.tags
}

# Diagnostic settings
resource "azurerm_monitor_diagnostic_setting" "acr" {
  name                       = "acr-diagnostics"
  target_resource_id         = azurerm_container_registry.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "ContainerRegistryRepositoryEvents"
  }

  enabled_log {
    category = "ContainerRegistryLoginEvents"
  }

  metric {
    category = "AllMetrics"
    enabled  = true
  }
}

# Azure Defender for container registries (optional)
resource "azurerm_security_center_subscription_pricing" "acr" {
  count         = var.enable_defender ? 1 : 0
  tier          = "Standard"
  resource_type = "ContainerRegistry"
}
