# Azure provider version
terraform {
  required_version = ">= 1.12.2"

  required_providers {
    azurerm = {
      source                = "hashicorp/azurerm"
      version               = ">= 4.45.0"
      configuration_aliases = [azurerm.connectivity]
    }
    random = {
      source  = "hashicorp/random"
      version = ">= 3.7.2"
    }
    azapi = {
      source  = "Azure/azapi"
      version = ">= 2.6.1"
    }
    http = {
      source  = "hashicorp/http"
      version = ">=3.5.0"
    }
  }
}

# resource "azurerm_resource_provider_registration" "encryptionAtHost" {
#   name = "Microsoft.Compute"
#   feature {
#     name       = "EncryptionAtHost"
#     registered = true
#   }
# }
