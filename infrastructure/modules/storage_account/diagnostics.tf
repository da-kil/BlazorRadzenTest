resource "azurerm_monitor_diagnostic_setting" "sa" {
  for_each                   = var.diagnostics != null && var.diagnostics.storage_account != null && length(var.diagnostics.storage_account.log_metrics) > 0 ? { main = var.diagnostics } : {}
  name                       = format("%s-sa-Audit", azurerm_storage_account.this.name)
  target_resource_id         = azurerm_storage_account.this.id
  log_analytics_workspace_id = var.diagnostics.storage_account.log_analytics_workspace_id

  dynamic "enabled_metric" {
    for_each = each.value.storage_account.log_metrics
    content {
      category = enabled_metric.value
    }
  }
}

resource "azurerm_monitor_diagnostic_setting" "blobs" {
  for_each                   = var.diagnostics != null && var.diagnostics.blobs != null && (length(var.diagnostics.blobs.log_metrics) > 0 || length(var.diagnostics.blobs.log_categories) > 0 || length(var.diagnostics.blobs.log_category_groups) > 0) ? { main = var.diagnostics } : {}
  name                       = format("%s-sa-blobs-Audit", azurerm_storage_account.this.name)
  target_resource_id         = "${azurerm_storage_account.this.id}/blobServices/default/"
  log_analytics_workspace_id = try(each.value.blobs.log_analytics_workspace_id, null)

  dynamic "enabled_metric" {
    for_each = each.value.blobs.log_metrics
    content {
      category = enabled_metric.value
    }
  }

  dynamic "enabled_log" {
    for_each = each.value.blobs.log_category_groups
    content {
      category_group = enabled_log.value
    }
  }

  dynamic "enabled_log" {
    for_each = each.value.blobs.log_categories
    content {
      category = enabled_log.value
    }
  }
}

resource "azurerm_monitor_diagnostic_setting" "files" {
  for_each                   = var.diagnostics != null && var.diagnostics.files != null && (length(var.diagnostics.files.log_metrics) > 0 || length(var.diagnostics.files.log_categories) > 0 || length(var.diagnostics.files.log_category_groups) > 0) ? { main = var.diagnostics } : {}
  name                       = format("%s-sa-files-Audit", azurerm_storage_account.this.name)
  target_resource_id         = "${azurerm_storage_account.this.id}/fileServices/default/"
  log_analytics_workspace_id = try(each.value.files.log_analytics_workspace_id, null)

  dynamic "enabled_metric" {
    for_each = each.value.files.log_metrics
    content {
      category = enabled_metric.value
    }
  }

  dynamic "enabled_log" {
    for_each = each.value.files.log_category_groups
    content {
      category_group = enabled_log.value
    }
  }

  dynamic "enabled_log" {
    for_each = each.value.files.log_categories
    content {
      category = enabled_log.value
    }
  }
}

resource "azurerm_monitor_diagnostic_setting" "tables" {
  for_each                   = var.diagnostics != null && var.diagnostics.tables != null && (length(var.diagnostics.tables.log_metrics) > 0 || length(var.diagnostics.tables.log_categories) > 0 || length(var.diagnostics.tables.log_category_groups) > 0) ? { main = var.diagnostics } : {}
  name                       = format("%s-sa-tables-Audit", azurerm_storage_account.this.name)
  target_resource_id         = "${azurerm_storage_account.this.id}/tableServices/default/"
  log_analytics_workspace_id = each.value.tables.log_analytics_workspace_id

  dynamic "enabled_metric" {
    for_each = each.value.tables.log_metrics
    content {
      category = enabled_metric.value
    }
  }

  dynamic "enabled_log" {
    for_each = each.value.tables.log_category_groups
    content {
      category_group = enabled_log.value
    }
  }

  dynamic "enabled_log" {
    for_each = each.value.tables.log_categories
    content {
      category = enabled_log.value
    }
  }
}

resource "azurerm_monitor_diagnostic_setting" "queues" {
  for_each                   = var.diagnostics != null && var.diagnostics.queues != null && (length(var.diagnostics.queues.log_metrics) > 0 || length(var.diagnostics.queues.log_categories) > 0 || length(var.diagnostics.queues.log_category_groups) > 0) ? { main = var.diagnostics } : {}
  name                       = format("%s-sa-queues-Audit", azurerm_storage_account.this.name)
  target_resource_id         = "${azurerm_storage_account.this.id}/queueServices/default/"
  log_analytics_workspace_id = each.value.queues.log_analytics_workspace_id

  dynamic "enabled_metric" {
    for_each = each.value.queues.log_metrics
    content {
      category = enabled_metric.value
    }
  }

  dynamic "enabled_log" {
    for_each = each.value.queues.log_category_groups
    content {
      category_group = enabled_log.value
    }
  }

  dynamic "enabled_log" {
    for_each = each.value.queues.log_categories
    content {
      category = enabled_log.value
    }
  }
}
