# Security Module - Managed Identities and RBAC
# User-assigned managed identity for application workloads
resource "azurerm_user_assigned_identity" "commandapi" {
  name                = "id-${var.project_name}-commandapi-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location

  tags = var.tags
}

resource "azurerm_user_assigned_identity" "queryapi" {
  name                = "id-${var.project_name}-queryapi-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location

  tags = var.tags
}

resource "azurerm_user_assigned_identity" "frontend" {
  name                = "id-${var.project_name}-frontend-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location

  tags = var.tags
}

# Federated credentials for workload identity (AKS integration)
resource "azurerm_federated_identity_credential" "commandapi" {
  count               = var.enable_workload_identity ? 1 : 0
  name                = "federated-${var.project_name}-commandapi"
  resource_group_name = var.resource_group_name
  parent_id           = azurerm_user_assigned_identity.commandapi.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = var.aks_oidc_issuer_url
  subject             = "system:serviceaccount:${var.kubernetes_namespace}:commandapi"
}

resource "azurerm_federated_identity_credential" "queryapi" {
  count               = var.enable_workload_identity ? 1 : 0
  name                = "federated-${var.project_name}-queryapi"
  resource_group_name = var.resource_group_name
  parent_id           = azurerm_user_assigned_identity.queryapi.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = var.aks_oidc_issuer_url
  subject             = "system:serviceaccount:${var.kubernetes_namespace}:queryapi"
}

resource "azurerm_federated_identity_credential" "frontend" {
  count               = var.enable_workload_identity ? 1 : 0
  name                = "federated-${var.project_name}-frontend"
  resource_group_name = var.resource_group_name
  parent_id           = azurerm_user_assigned_identity.frontend.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = var.aks_oidc_issuer_url
  subject             = "system:serviceaccount:${var.kubernetes_namespace}:frontend"
}

# RBAC - Key Vault access for managed identities
resource "azurerm_role_assignment" "commandapi_keyvault" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.commandapi.principal_id
}

resource "azurerm_role_assignment" "queryapi_keyvault" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.queryapi.principal_id
}

resource "azurerm_role_assignment" "frontend_keyvault" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.frontend.principal_id
}

# RBAC - ACR pull access for AKS (already handled in AKS module, but can add app-specific if needed)

# Network Security - Azure Policy assignments (optional)
resource "azurerm_resource_group_policy_assignment" "allowed_locations" {
  count                = var.enable_azure_policy ? 1 : 0
  name                 = "policy-allowed-locations-${var.environment}"
  resource_group_id    = var.resource_group_id
  policy_definition_id = "/providers/Microsoft.Authorization/policyDefinitions/e56962a6-4747-49cd-b67b-bf8b01975c4c"

  parameters = jsonencode({
    listOfAllowedLocations = {
      value = var.allowed_locations
    }
  })
}

resource "azurerm_resource_group_policy_assignment" "require_tags" {
  count                = var.enable_azure_policy ? 1 : 0
  name                 = "policy-require-tags-${var.environment}"
  resource_group_id    = var.resource_group_id
  policy_definition_id = "/providers/Microsoft.Authorization/policyDefinitions/96670d01-0a4d-4649-9c89-2d3abc0a5025"

  parameters = jsonencode({
    tagName = {
      value = "Environment"
    }
  })
}

# Azure Defender for Cloud (Security Center)
resource "azurerm_security_center_subscription_pricing" "defender_for_containers" {
  count         = var.enable_defender ? 1 : 0
  tier          = "Standard"
  resource_type = "Containers"
}

resource "azurerm_security_center_subscription_pricing" "defender_for_keyvault" {
  count         = var.enable_defender ? 1 : 0
  tier          = "Standard"
  resource_type = "KeyVaults"
}

resource "azurerm_security_center_subscription_pricing" "defender_for_databases" {
  count         = var.enable_defender ? 1 : 0
  tier          = "Standard"
  resource_type = "OpenSourceRelationalDatabases"
}
