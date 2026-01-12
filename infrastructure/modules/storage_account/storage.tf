resource "azurerm_storage_account" "this" {
  name                = local.resource_name
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = local.merged_tags

  account_tier                     = var.account_tier
  account_replication_type         = var.account_replication_type
  min_tls_version                  = "TLS1_2" // Nothing lower allowed for new accounts
  https_traffic_only_enabled       = var.https_traffic_only_enabled
  account_kind                     = var.account_kind
  cross_tenant_replication_enabled = false
  access_tier                      = var.access_tier
  public_network_access_enabled    = var.network_access_mode != "OnlyPrivateEndpoints"
  allow_nested_items_to_be_public  = var.allow_nested_items_to_be_public
  shared_access_key_enabled        = var.sas_enabled
  default_to_oauth_authentication  = false

  queue_encryption_key_type = var.storage_encryption_customer_managed_key != null ? "Account" : "Service"
  table_encryption_key_type = var.storage_encryption_customer_managed_key != null ? "Account" : "Service"

  network_rules {
    default_action             = var.network_access_mode == "Public" ? "Allow" : "Deny"
    ip_rules                   = coalesce(var.network_ips_to_whitelist, [])
    virtual_network_subnet_ids = coalesce(var.network_subnet_id_to_whitelist, [])
    bypass                     = coalesce(var.network_bypass, ["None"])
  }

  dynamic "identity" {
    for_each = var.storage_encryption_customer_managed_key != null ? ["this"] : []
    content {
      type = "UserAssigned"
      identity_ids = [
        var.storage_encryption_customer_managed_key.user_assigned_identity_id
      ]
    }
  }

  lifecycle {
    ignore_changes = [
      customer_managed_key,
    ]
  }
}
