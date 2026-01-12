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
  description = "Name of the resource group for identity resources"
  type        = string
}

variable "vnet_id" {
  description = "ID of the virtual network for Network Contributor role"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
