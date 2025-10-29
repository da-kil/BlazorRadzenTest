# Monitoring Module Variables
variable "project_name" {
  description = "Project name for resource naming"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "key_vault_id" {
  description = "Key Vault ID for storing secrets"
  type        = string
}

variable "log_analytics_sku" {
  description = "Log Analytics SKU"
  type        = string
  default     = "PerGB2018"
}

variable "log_retention_in_days" {
  description = "Log retention period in days"
  type        = number
  default     = 30
}

variable "enable_alerts" {
  description = "Enable metric alerts"
  type        = bool
  default     = true
}

variable "aks_cluster_id" {
  description = "AKS cluster ID for monitoring"
  type        = string
  default     = null
}

variable "postgresql_server_id" {
  description = "PostgreSQL server ID for monitoring"
  type        = string
  default     = null
}

variable "alert_email_receivers" {
  description = "List of email receivers for alerts"
  type = list(object({
    name          = string
    email_address = string
  }))
  default = []
}

variable "alert_webhook_receivers" {
  description = "List of webhook receivers for alerts"
  type = list(object({
    name        = string
    service_uri = string
  }))
  default = []
}

variable "enable_smart_detection" {
  description = "Enable Application Insights smart detection"
  type        = bool
  default     = true
}

variable "smart_detection_email_recipients" {
  description = "Email recipients for smart detection alerts"
  type        = list(string)
  default     = []
}

variable "create_workbook" {
  description = "Create Application Insights workbook"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
