resource "azurerm_storage_container" "this" {
  for_each = {
    for container in var.storage_containers :
    container.name => container
  }
  name                  = each.value.name
  storage_account_id    = azurerm_storage_account.this.id
  container_access_type = each.value.access_type
  metadata              = each.value.metadata
}

resource "azurerm_storage_queue" "this" {
  for_each = {
    for queue in var.storage_queues :
    queue.name => queue
  }
  name               = each.value.name
  metadata           = each.value.metadata
  storage_account_id = azurerm_storage_account.this.id
}

resource "azurerm_storage_share" "this" {
  for_each = {
    for file_share in var.storage_file_shares :
    file_share.name => file_share
  }
  name               = each.value.name
  storage_account_id = azurerm_storage_account.this.id
  quota              = each.value.quota_in_gb
  metadata           = each.value.metadata
  enabled_protocol   = each.value.protocol

  dynamic "acl" {
    for_each = each.value.acl
    content {
      id = acl.value.id

      access_policy {
        permissions = acl.value.access_policy.permissions
        start       = acl.value.access_policy.start
        expiry      = acl.value.access_policy.end
      }
    }
  }
}

resource "azurerm_storage_table" "this" {
  for_each = {
    for table in var.storage_tables :
    table.name => table
  }

  name                 = each.value.name
  storage_account_name = azurerm_storage_account.this.name

  dynamic "acl" {
    for_each = each.value.acl
    content {
      id = acl.value.id
      access_policy {
        permissions = acl.value.access_policy.permissions
        start       = acl.value.access_policy.start
        expiry      = acl.value.access_policy.end
      }
    }
  }
}
