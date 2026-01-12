resource "azurerm_monitor_diagnostic_setting" "this" {
  for_each                   = var.diagnostics != null && (length(var.diagnostics.log_metrics) > 0 || length(var.diagnostics.log_categories) > 0) ? { main = var.diagnostics } : {}
  name                       = format("%s-AKV-Audit", azurerm_key_vault.this.name)
  target_resource_id         = azurerm_key_vault.this.id
  log_analytics_workspace_id = try(each.value.log_analytics_workspace_id, null)

  dynamic "enabled_metric" {
    for_each = each.value.log_metrics
    content {
      category = enabled_metric.value
    }
  }

  dynamic "enabled_log" {
    for_each = var.diagnostics.log_category_groups
    content {
      category_group = enabled_log.value
    }
  }

  dynamic "enabled_log" {
    for_each = var.diagnostics.log_categories
    content {
      category = enabled_log.value
    }
  }
}
