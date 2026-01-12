terraform {
  required_version = ">= 1.12.2"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.57.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }

  backend "azurerm" {
    resource_group_name  = "rg-beachbreak-tfstate-swn"
    storage_account_name = "sabeachbreaktfstate"
    container_name       = "tfstate"
    key                  = "beachbreak-prod.tfstate"
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = false
      recover_soft_deleted_key_vaults = true
    }
    resource_group {
      prevent_deletion_if_contains_resources = true # Prod: Prevent accidental deletion
    }
  }

  # Enable storage account Azure AD authentication
  storage_use_azuread = true
}
