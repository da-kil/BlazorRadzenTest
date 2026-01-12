variable "env" {
  description = "Environment name"
  type        = string
  default     = "prod"
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "switzerlandnorth"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    Environment = "Production"
    Project     = "BeachBreak"
    ManagedBy   = "Terraform"
    CostCenter  = "Production"
  }
}

variable "kubernetes_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.31"
}

variable "postgresql_version" {
  description = "PostgreSQL version"
  type        = string
  default     = "16"
}
