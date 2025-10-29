# Security Module Variables
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

variable "resource_group_id" {
  description = "Resource group ID for policy assignments"
  type        = string
}

variable "key_vault_id" {
  description = "Key Vault ID for RBAC assignments"
  type        = string
}

variable "enable_workload_identity" {
  description = "Enable workload identity federation for AKS"
  type        = bool
  default     = true
}

variable "aks_oidc_issuer_url" {
  description = "AKS OIDC issuer URL for workload identity"
  type        = string
  default     = null
}

variable "kubernetes_namespace" {
  description = "Kubernetes namespace for workload identity"
  type        = string
  default     = "beachbreak"
}

variable "enable_azure_policy" {
  description = "Enable Azure Policy assignments"
  type        = bool
  default     = false
}

variable "allowed_locations" {
  description = "List of allowed Azure regions for resources"
  type        = list(string)
  default     = ["westeurope", "northeurope"]
}

variable "enable_defender" {
  description = "Enable Azure Defender for Cloud"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
