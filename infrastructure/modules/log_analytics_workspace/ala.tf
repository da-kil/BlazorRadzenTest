resource "azurerm_log_analytics_workspace" "this" {
  name                         = local.resource_name
  resource_group_name          = var.resource_group_name
  location                     = var.location
  sku                          = var.sku
  retention_in_days            = var.retention_days
  daily_quota_gb               = var.daily_quota_gb
  internet_ingestion_enabled   = var.allow_public_ingestion_access
  internet_query_enabled       = var.allow_public_query_access
  local_authentication_enabled = var.local_auth_enabled
  tags                         = local.merged_tags
}
