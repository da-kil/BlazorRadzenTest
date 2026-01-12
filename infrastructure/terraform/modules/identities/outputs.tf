output "cluster_identity" {
  description = "AKS cluster identity object with clientId, objectId (principalId), and resourceId"
  value = {
    client_id   = azurerm_user_assigned_identity.aks_cluster.client_id
    principal_id = azurerm_user_assigned_identity.aks_cluster.principal_id
    id          = azurerm_user_assigned_identity.aks_cluster.id
  }
}

output "kubelet_identity" {
  description = "AKS kubelet identity object with clientId, objectId (principalId), and resourceId"
  value = {
    client_id   = azurerm_user_assigned_identity.aks_kubelet.client_id
    principal_id = azurerm_user_assigned_identity.aks_kubelet.principal_id
    id          = azurerm_user_assigned_identity.aks_kubelet.id
  }
}

output "cluster_identity_id" {
  description = "Resource ID of the AKS cluster identity"
  value       = azurerm_user_assigned_identity.aks_cluster.id
}

output "cluster_identity_principal_id" {
  description = "Principal ID of the AKS cluster identity"
  value       = azurerm_user_assigned_identity.aks_cluster.principal_id
}

output "kubelet_identity_id" {
  description = "Resource ID of the AKS kubelet identity"
  value       = azurerm_user_assigned_identity.aks_kubelet.id
}

output "kubelet_identity_principal_id" {
  description = "Principal ID of the AKS kubelet identity"
  value       = azurerm_user_assigned_identity.aks_kubelet.principal_id
}
