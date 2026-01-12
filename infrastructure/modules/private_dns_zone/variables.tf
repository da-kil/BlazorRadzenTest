#
# GENERAL
#
variable "env" {
  type        = string
  description = "The deployment environment name (dev / test / prod)"
  validation {
    condition     = contains(["dev", "test", "prod"], var.env)
    error_message = "Allowed values for sku are \"dev\", \"test\", or \"prod\"."
  }
}

variable "name" {
  description = "Name of the DNS Zone (aka the domain itself)"
  type        = string
}

variable "resource_group_name" {
  type        = string
  description = "Name of the Resource Group the DNS Zone should be deployed in"
}

variable "tags" {
  type        = map(string)
  default     = {}
  description = "Map of TAGs that will be added to the deployed resources"
}

#
# PRIVATE DNS ZONE
#

variable "linked_vnet_ids" {
  type = map(string)
}
