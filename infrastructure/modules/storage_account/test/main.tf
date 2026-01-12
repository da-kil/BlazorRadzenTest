terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.57.0"
    }
  }

  backend "local" {
    path = "./terraform.tfstate"
  }
  required_version = ">= 1.13.3, < 2.0.0"
}

provider "azurerm" {
  features {}
  subscription_id                 = var.azure_subscription_id
  storage_use_azuread             = true
  resource_provider_registrations = "none"
  resource_providers_to_register = [
    "Microsoft.KeyVault",
  ]
}

#
## Base Resources
#

module "test_base" {
  source           = "../../../terraform_azurerm_helper/test_base"
  module_shortname = "storage"
}

resource "azurerm_private_dns_zone" "this" {
  for_each            = toset(["blob", "file", "queue", "table"])
  name                = "privatelink.${each.key}.core.windows.net"
  resource_group_name = module.test_base.base_resource_group_name
}

data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "this" {
  name                       = "tftst-stor-test-01-swn"
  location                   = "switzerlandnorth"
  resource_group_name        = module.test_base.base_resource_group_name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  rbac_authorization_enabled = true

  purge_protection_enabled = true // Needed to use customer managed key in storage account
}

resource "azurerm_user_assigned_identity" "storage" {
  name                = "uai-tftest-storage-test-01-swn"
  location            = "switzerlandnorth"
  resource_group_name = module.test_base.base_resource_group_name
}

resource "azurerm_role_assignment" "this" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Crypto Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "azurerm_role_assignment" "storage" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Crypto Service Encryption User"
  principal_id         = azurerm_user_assigned_identity.storage.principal_id
}

resource "azurerm_key_vault_key" "this" {
  // In Prod, do not forget to set a key rotation policy
  name         = "key-tftest-storage-test-01-swn"
  key_vault_id = azurerm_key_vault.this.id
  key_type     = "RSA"
  key_size     = 2048
  key_opts = [
    "wrapKey",
    "unwrapKey",
  ]

  depends_on = [azurerm_role_assignment.this]
}

resource "azurerm_log_analytics_workspace" "this" {
  name                = "tftst-law-test-01-swn"
  resource_group_name = module.test_base.base_resource_group_name
  location            = "switzerlandnorth"
}

#
## Module config
#

module "storage" {
  source = "../"

  env                                  = "test"
  component                            = "tftest"
  resource_name_infix                  = "sta"
  resource_group_name                  = module.test_base.base_resource_group_name
  storage_types                        = ["blob", "table", "queue", "file"]
  private_endpoint_subnet_id           = module.test_base.base_sub_net_id
  central_dns_zone_resource_group_name = module.test_base.base_resource_group_name

  storage_encryption_customer_managed_key = {
    key_vault_uri             = azurerm_key_vault.this.vault_uri
    key_name                  = azurerm_key_vault_key.this.name
    user_assigned_identity_id = azurerm_user_assigned_identity.storage.id
  }

  // Local network access required to test storage_tables
  sas_enabled         = true
  network_access_mode = "OnlyAllowed"
  network_ips_to_whitelist = [
    "212.51.156.236", // ti&m Office
  ]

  storage_containers = [
    {
      name = "test"
    },
  ]
  storage_file_shares = [
    {
      name        = "test"
      quota_in_gb = 10
      protocol    = "SMB"
    },
  ]
  storage_queues = [
    {
      name = "test"
    },
  ]
  storage_tables = [
    {
      name = "test"
    },
  ]

  providers = {
    azurerm              = azurerm
    azurerm.connectivity = azurerm
  }

  diagnostics = {
    storage_account = {
      log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id
    }
    blobs = {
      log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id
    }
    files = {
      log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id
    }
    queues = {
      log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id
    }
    tables = {
      log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id
    }
  }

  // Only needed for test, since the dns zones are not directly dependant
  depends_on = [
    azurerm_private_dns_zone.this,
  ]
}
