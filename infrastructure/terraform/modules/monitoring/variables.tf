variable "env" {
  description = "Environment name (dev, test, prod)"
  type        = string
  validation {
    condition     = contains(["dev", "test", "prod"], var.env)
    error_message = "Environment must be dev, test, or prod"
  }
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "switzerlandnorth"
}

variable "resource_group_name" {
  description = "Name of the resource group for monitoring resources"
  type        = string
}

variable "log_analytics_retention_days" {
  description = "Number of days to retain data in Log Analytics workspace"
  type        = number
  default     = 30
}

variable "log_analytics_sku" {
  description = "SKU for Log Analytics workspace"
  type        = string
  default     = "PerGB2018"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
