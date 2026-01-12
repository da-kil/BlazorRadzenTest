resource "azurerm_container_registry" "this" {
  name                          = local.resourceName
  resource_group_name           = var.resource_group_name
  location                      = var.location
  sku                           = "Premium"
  admin_enabled                 = false
  public_network_access_enabled = var.public_network_access_enabled
  data_endpoint_enabled         = true
  anonymous_pull_enabled        = var.anonymous_pull_enabled
  export_policy_enabled         = var.export_policy_enabled
  zone_redundancy_enabled       = var.zone_redundancy_enabled
  trust_policy_enabled          = var.trust_policy_enabled
  quarantine_policy_enabled     = var.quarantine_policy_enabled

  network_rule_bypass_option = "AzureServices"

  dynamic "georeplications" {
    for_each = var.geo_replications
    content {
      location                = georeplications.value.location
      zone_redundancy_enabled = georeplications.value.zoneRedundancyEnabled
      tags                    = local.merged_tags
    }
  }

  dynamic "encryption" {
    for_each = var.acr_encryption_key_vault_id != null ? toset([var.acr_encryption_key_vault_id]) : toset([])
    content {
      key_vault_key_id   = var.acr_encryption_key_vault_id
      identity_client_id = var.acr_encryption_user_identity_id
    }
  }

  network_rule_set {
    default_action = "Deny"

    ip_rule = [
      for ip in var.network_ips_to_whitelist : {
        action : "Allow"
        ip_mask : ip
      }
    ]
  }

  # Used for potential cache rule
  identity {
    type = "SystemAssigned"
  }

  dynamic "identity" {
    for_each = var.user_assigned_identity_id != null ? toset([var.user_assigned_identity_id]) : toset([])
    content {
      type         = "UserAssigned"
      identity_ids = [identity.value]
    }
  }

  tags = local.merged_tags
}
