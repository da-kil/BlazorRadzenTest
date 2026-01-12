resource "azurerm_container_registry_cache_rule" "this" {
  for_each = {
    for rule in var.container_registry_cache_rules :
    "${rule.name}" => rule
  }
  name                  = each.value.name
  container_registry_id = azurerm_container_registry.this.id
  target_repo           = each.value.targetRepo
  source_repo           = each.value.sourceRepo
  credential_set_id     = each.value.credentialSetName != null ? azurerm_container_registry_credential_set.this[each.value.credentialSetName].id : null
}

resource "azurerm_container_registry_credential_set" "this" {
  for_each = {
    for credential in var.container_registry_credential_sets :
    "${credential.name}" => credential
  }
  name                  = each.value.name
  container_registry_id = azurerm_container_registry.this.id
  login_server          = each.value.loginServer

  identity {
    type = "SystemAssigned"
  }

  authentication_credentials {
    username_secret_id = each.value.usernameSecretId
    password_secret_id = each.value.passwordSecretId
  }
}
