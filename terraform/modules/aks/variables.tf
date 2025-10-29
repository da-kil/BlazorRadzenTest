# AKS Module Variables
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

variable "aks_subnet_id" {
  description = "AKS subnet ID"
  type        = string
}

variable "vnet_id" {
  description = "Virtual Network ID"
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "Log Analytics workspace ID for monitoring"
  type        = string
}

variable "acr_id" {
  description = "Azure Container Registry ID"
  type        = string
  default     = null
}

variable "kubernetes_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.29"
}

variable "availability_zones" {
  description = "Availability zones for node pools"
  type        = list(string)
  default     = ["1", "2", "3"]
}

# System Node Pool
variable "system_node_vm_size" {
  description = "VM size for system node pool"
  type        = string
  default     = "Standard_D4s_v5"
}

variable "system_node_count" {
  description = "Initial node count for system pool"
  type        = number
  default     = 3
}

variable "system_node_min_count" {
  description = "Minimum node count for system pool"
  type        = number
  default     = 2
}

variable "system_node_max_count" {
  description = "Maximum node count for system pool"
  type        = number
  default     = 5
}

# Application Node Pool
variable "app_node_vm_size" {
  description = "VM size for application node pool"
  type        = string
  default     = "Standard_D8s_v5"
}

variable "app_node_count" {
  description = "Initial node count for application pool"
  type        = number
  default     = 3
}

variable "app_node_min_count" {
  description = "Minimum node count for application pool"
  type        = number
  default     = 2
}

variable "app_node_max_count" {
  description = "Maximum node count for application pool"
  type        = number
  default     = 10
}

# Frontend Node Pool
variable "frontend_node_vm_size" {
  description = "VM size for frontend node pool"
  type        = string
  default     = "Standard_D4s_v5"
}

variable "frontend_node_count" {
  description = "Initial node count for frontend pool"
  type        = number
  default     = 2
}

variable "frontend_node_min_count" {
  description = "Minimum node count for frontend pool"
  type        = number
  default     = 2
}

variable "frontend_node_max_count" {
  description = "Maximum node count for frontend pool"
  type        = number
  default     = 8
}

# Network configuration
variable "dns_service_ip" {
  description = "DNS service IP for AKS"
  type        = string
  default     = "10.1.0.10"
}

variable "service_cidr" {
  description = "Service CIDR for AKS"
  type        = string
  default     = "10.1.0.0/16"
}

variable "aks_admin_group_object_ids" {
  description = "Azure AD group object IDs for AKS admin access"
  type        = list(string)
  default     = []
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
