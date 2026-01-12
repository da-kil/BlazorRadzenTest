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

variable "deployment_index" {
  type        = string
  default     = "01"
  description = "Index counter of this instance, will be used on resource naming"
}

variable "component" {
  type        = string
  description = "Name of the application component, will be used on resource naming"
}

variable "location" {
  type        = string
  default     = "switzerlandnorth"
  description = "Deployment location"
}

variable "tags" {
  type        = map(string)
  default     = {}
  description = "Map of TAGs that will be added to the deployed resources"
}
