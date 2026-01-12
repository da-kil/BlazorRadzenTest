# Local variables
locals {
  location_short = {
    switzerlandnorth = "swn"
    switzerlandwest  = "sww"
    northeurope      = "eun"
    westeurope       = "euw"
    swedencentral    = "swc"
  }
  loc = lookup(local.location_short, var.location, "swn")

  common_tags = merge(var.tags, {
    Environment = var.env
    ManagedBy   = "Terraform"
  })
}

# Data source for current client (Terraform executor)
data "azurerm_client_config" "current" {}

# AKS Cluster Identity
resource "azurerm_user_assigned_identity" "aks_cluster" {
  name                = "id-aks-cluster-${var.env}-${local.loc}-01"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# AKS Kubelet Identity
resource "azurerm_user_assigned_identity" "aks_kubelet" {
  name                = "id-aks-kubelet-${var.env}-${local.loc}-01"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Role Assignment: AKS Cluster Identity as Network Contributor on VNet
resource "azurerm_role_assignment" "aks_cluster_network_contributor" {
  count                = var.vnet_id != "" ? 1 : 0
  scope                = var.vnet_id
  role_definition_name = "Network Contributor"
  principal_id         = azurerm_user_assigned_identity.aks_cluster.principal_id
}

# Role Assignment: AKS Cluster Identity as Managed Identity Operator on Kubelet Identity
resource "azurerm_role_assignment" "aks_cluster_identity_operator" {
  scope                = azurerm_user_assigned_identity.aks_kubelet.id
  role_definition_name = "Managed Identity Operator"
  principal_id         = azurerm_user_assigned_identity.aks_cluster.principal_id
}
