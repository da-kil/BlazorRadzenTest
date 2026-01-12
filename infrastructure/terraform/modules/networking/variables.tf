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
  description = "Name of the resource group for networking resources"
  type        = string
}

variable "vnet_address_space" {
  description = "Address space for the virtual network"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "aks_node_pool_subnet_cidr" {
  description = "CIDR block for AKS node pool subnet"
  type        = string
  default     = "10.0.0.0/22"
}

variable "aks_api_server_subnet_cidr" {
  description = "CIDR block for AKS API server subnet (VNet integration)"
  type        = string
  default     = "10.0.4.0/28"
}

variable "private_endpoint_subnet_cidr" {
  description = "CIDR block for private endpoint subnet"
  type        = string
  default     = "10.0.5.0/24"
}

variable "postgresql_subnet_cidr" {
  description = "CIDR block for PostgreSQL delegated subnet"
  type        = string
  default     = "10.0.6.0/24"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
