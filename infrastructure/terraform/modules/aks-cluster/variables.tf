variable "env" {
  description = "Environment name (dev, test, prod)"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "switzerlandnorth"
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "cluster_identity_id" {
  description = "User-assigned identity ID for AKS cluster"
  type        = string
}

variable "cluster_identity_principal_id" {
  description = "Principal ID of cluster identity"
  type        = string
}

variable "kubelet_identity_id" {
  description = "User-assigned identity ID for kubelet"
  type        = string
}

variable "kubelet_identity_principal_id" {
  description = "Principal ID of kubelet identity"
  type        = string
}

variable "kubelet_identity_client_id" {
  description = "Client ID of kubelet identity"
  type        = string
}

variable "vnet_id" {
  description = "Virtual network ID"
  type        = string
}

variable "aks_node_pool_subnet_id" {
  description = "Subnet ID for AKS node pool"
  type        = string
}

variable "aks_api_server_subnet_id" {
  description = "Subnet ID for AKS API server (VNet integration)"
  type        = string
}

variable "kubernetes_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.31"
}

variable "default_node_pool_vm_size" {
  description = "VM size for default node pool"
  type        = string
  default     = "Standard_D2s_v3"
}

variable "default_node_pool_node_count" {
  description = "Number of nodes in default pool"
  type        = number
  default     = 2
}

variable "user_node_pool_vm_size" {
  description = "VM size for user node pool"
  type        = string
  default     = "Standard_D4s_v3"
}

variable "user_node_pool_min_count" {
  description = "Minimum nodes in user pool"
  type        = number
  default     = 1
}

variable "user_node_pool_max_count" {
  description = "Maximum nodes in user pool"
  type        = number
  default     = 3
}

variable "etcd_encryption_key_id" {
  description = "Key Vault key ID for etcd encryption"
  type        = string
  default     = ""
}

variable "diagnostics_workspace_id" {
  description = "Log Analytics workspace ID for diagnostics"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
