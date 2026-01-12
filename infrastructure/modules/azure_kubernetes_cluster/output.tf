output "resource_name" {
  description = "Name of the Kubernetes Cluster"
  value       = azurerm_kubernetes_cluster.this.name
}
output "resource_id" {
  description = "Resource ID of the Kubernetes Cluster"
  value       = azurerm_kubernetes_cluster.this.id
}
output "artifact_identifier" {
  description = "Identifier of the deployed blueprint"
  value       = local.artifactIdentifier
}
output "cluster_portal_fqdn" {
  description = "Full qualified domain name of cluster used in Azure Portal"
  value       = azurerm_kubernetes_cluster.this.portal_fqdn
}
output "cluster_private_fqdn" {
  description = "Full qualified domain name of cluster used for Private Endpoint"
  value       = azurerm_kubernetes_cluster.this.private_fqdn
}
output "oidc_issuer_url" {
  description = "URL of the OIDC issuer used for Workload Identity"
  value       = azurerm_kubernetes_cluster.this.oidc_issuer_url
}
output "kube_admin_config" {
  description = "Kube Admin config"
  value       = azurerm_kubernetes_cluster.this.kube_admin_config
}
output "secret_identity" {
  description = "Properties of the managed identity used by the secret provider"
  value       = azurerm_kubernetes_cluster.this.key_vault_secrets_provider[0].secret_identity
}
