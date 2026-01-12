# Role Assignment for the Cluster to allow Kubelet Identity Management
resource "azurerm_role_assignment" "cluster_to_kubelet" {
  scope                = var.kubelet_identity.resourceId
  principal_id         = var.cluster_identity.objectId
  role_definition_name = "Managed Identity Operator"
  principal_type       = "ServicePrincipal"
}

# Role Assignment for the Cluster to allow Private DNS Zone Management
resource "azurerm_role_assignment" "cluster_to_dns" {
  count                = null != var.cluster_dns_zone_id ? 1 : 0
  scope                = var.cluster_dns_zone_id
  principal_id         = var.cluster_identity.objectId
  role_definition_name = "Private DNS Zone Contributor"
  principal_type       = "ServicePrincipal"
}

# Role Assignment for the Cluster to allow Network Contributor
# Should be replaced with custom role in the future
resource "azurerm_role_assignment" "cluster_to_vnt" {
  scope                = var.cluster_virtual_network_id
  principal_id         = var.cluster_identity.objectId
  role_definition_name = "Network Contributor"
  principal_type       = "ServicePrincipal"
}

# Role Assignment for the Kubelet to allow for Image Pull from Private ACR
resource "azurerm_role_assignment" "kubelet_to_acr" {
  scope                = var.bootstrap_container_registry_id
  principal_id         = var.kubelet_identity.objectId
  role_definition_name = "acrpull"
  principal_type       = "ServicePrincipal"
}

# Role Assignment for the Cluster to allow for Key Vault Access for etcd encryption
resource "azurerm_role_assignment" "cluster_to_akv_data" {
  scope                = var.etcd_encryption_key_vault_id
  principal_id         = var.cluster_identity.objectId
  role_definition_name = "Key Vault Crypto User"
  principal_type       = "ServicePrincipal"
}

# Role Assignment for the Cluster to allow for Key Vault Contributor for etcd encryption
resource "azurerm_role_assignment" "cluster_to_akv_control" {
  scope                = var.etcd_encryption_key_vault_id
  principal_id         = var.cluster_identity.objectId
  role_definition_name = "Key Vault Contributor"
  principal_type       = "ServicePrincipal"
}
