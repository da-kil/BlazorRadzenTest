# Networking Module Variables
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

variable "vnet_address_space" {
  description = "Address space for the virtual network"
  type        = string
  default     = "10.0.0.0/16"
}

variable "aks_subnet_address_prefix" {
  description = "Address prefix for AKS subnet"
  type        = string
  default     = "10.0.1.0/24"
}

variable "postgresql_subnet_address_prefix" {
  description = "Address prefix for PostgreSQL subnet"
  type        = string
  default     = "10.0.2.0/24"
}

variable "private_endpoints_subnet_address_prefix" {
  description = "Address prefix for private endpoints subnet"
  type        = string
  default     = "10.0.3.0/24"
}

variable "app_gateway_subnet_address_prefix" {
  description = "Address prefix for Application Gateway subnet"
  type        = string
  default     = "10.0.4.0/24"
}

variable "enable_app_gateway" {
  description = "Enable Application Gateway subnet"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
