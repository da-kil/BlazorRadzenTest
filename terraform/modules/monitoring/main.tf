# Monitoring Module - Application Insights, Log Analytics
resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${var.project_name}-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.log_analytics_sku
  retention_in_days   = var.log_retention_in_days

  tags = var.tags
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = "appi-${var.project_name}-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"

  tags = var.tags
}

# Store Application Insights connection string in Key Vault
resource "azurerm_key_vault_secret" "app_insights_connection_string" {
  name         = "ApplicationInsights-ConnectionString"
  value        = azurerm_application_insights.main.connection_string
  key_vault_id = var.key_vault_id

  tags = var.tags
}

resource "azurerm_key_vault_secret" "app_insights_instrumentation_key" {
  name         = "ApplicationInsights-InstrumentationKey"
  value        = azurerm_application_insights.main.instrumentation_key
  key_vault_id = var.key_vault_id

  tags = var.tags
}

# Action Group for alerts
resource "azurerm_monitor_action_group" "main" {
  name                = "ag-${var.project_name}-${var.environment}"
  resource_group_name = var.resource_group_name
  short_name          = substr("${var.project_name}${var.environment}", 0, 12)

  dynamic "email_receiver" {
    for_each = var.alert_email_receivers
    content {
      name                    = email_receiver.value.name
      email_address           = email_receiver.value.email_address
      use_common_alert_schema = true
    }
  }

  dynamic "webhook_receiver" {
    for_each = var.alert_webhook_receivers
    content {
      name                    = webhook_receiver.value.name
      service_uri             = webhook_receiver.value.service_uri
      use_common_alert_schema = true
    }
  }

  tags = var.tags
}

# Alert Rules - High CPU on AKS
resource "azurerm_monitor_metric_alert" "aks_cpu" {
  count               = var.enable_alerts ? 1 : 0
  name                = "alert-aks-high-cpu-${var.environment}"
  resource_group_name = var.resource_group_name
  scopes              = [var.aks_cluster_id]
  description         = "Alert when AKS node CPU exceeds threshold"
  severity            = 2
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.ContainerService/managedClusters"
    metric_name      = "node_cpu_usage_percentage"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 80
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.tags
}

# Alert Rules - High Memory on AKS
resource "azurerm_monitor_metric_alert" "aks_memory" {
  count               = var.enable_alerts ? 1 : 0
  name                = "alert-aks-high-memory-${var.environment}"
  resource_group_name = var.resource_group_name
  scopes              = [var.aks_cluster_id]
  description         = "Alert when AKS node memory exceeds threshold"
  severity            = 2
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.ContainerService/managedClusters"
    metric_name      = "node_memory_working_set_percentage"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 80
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.tags
}

# Alert Rules - PostgreSQL High DTU
resource "azurerm_monitor_metric_alert" "postgresql_dtu" {
  count               = var.enable_alerts && var.postgresql_server_id != null ? 1 : 0
  name                = "alert-postgresql-high-dtu-${var.environment}"
  resource_group_name = var.resource_group_name
  scopes              = [var.postgresql_server_id]
  description         = "Alert when PostgreSQL CPU exceeds threshold"
  severity            = 2
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.DBforPostgreSQL/flexibleServers"
    metric_name      = "cpu_percent"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 80
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.tags
}

# Alert Rules - PostgreSQL Storage
resource "azurerm_monitor_metric_alert" "postgresql_storage" {
  count               = var.enable_alerts && var.postgresql_server_id != null ? 1 : 0
  name                = "alert-postgresql-storage-${var.environment}"
  resource_group_name = var.resource_group_name
  scopes              = [var.postgresql_server_id]
  description         = "Alert when PostgreSQL storage exceeds threshold"
  severity            = 1
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.DBforPostgreSQL/flexibleServers"
    metric_name      = "storage_percent"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 85
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.tags
}

# Application Insights Smart Detection - Failure Anomalies
resource "azurerm_application_insights_smart_detection_rule" "failure_anomalies" {
  name                    = "Failure Anomalies"
  application_insights_id = azurerm_application_insights.main.id
  enabled                 = var.enable_smart_detection

  send_emails_to_subscription_owners = false
  additional_email_recipients        = var.smart_detection_email_recipients
}

# Workbook for monitoring (optional)
resource "azurerm_application_insights_workbook" "main" {
  count               = var.create_workbook ? 1 : 0
  name                = "workbook-${var.project_name}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  display_name        = "BeachBreak ${var.environment} Monitoring"
  source_id           = azurerm_application_insights.main.id

  data_json = jsonencode({
    version = "Notebook/1.0"
    items = [
      {
        type = 1
        content = {
          json = "## BeachBreak Application Monitoring Dashboard"
        }
      }
    ]
  })

  tags = var.tags
}
