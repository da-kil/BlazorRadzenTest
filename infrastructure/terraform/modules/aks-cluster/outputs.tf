output "cluster_id" {
  description = "ID of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.id
}

output "cluster_name" {
  description = "Name of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.name
}

output "cluster_fqdn" {
  description = "FQDN of the AKS cluster (private)"
  value       = azurerm_kubernetes_cluster.main.private_fqdn
}

output "cluster_portal_fqdn" {
  description = "Portal FQDN of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.portal_fqdn
}

output "oidc_issuer_url" {
  description = "OIDC issuer URL for workload identity"
  value       = azurerm_kubernetes_cluster.main.oidc_issuer_url
}

output "kube_admin_config" {
  description = "Kube admin configuration (sensitive)"
  value       = azurerm_kubernetes_cluster.main.kube_admin_config
  sensitive   = true
}

output "kube_config" {
  description = "Kube configuration for cluster access (sensitive)"
  value       = azurerm_kubernetes_cluster.main.kube_config
  sensitive   = true
}

output "kubelet_identity_object_id" {
  description = "Object ID of the kubelet identity"
  value       = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
}

output "key_vault_secrets_provider_identity" {
  description = "Identity of the Key Vault secrets provider"
  value = azurerm_kubernetes_cluster.main.key_vault_secrets_provider[0].secret_identity[0]
}
