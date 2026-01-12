# Azure provider version
terraform {
  required_version = ">= 1.13.3, < 2.0.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.57.0, <5.0.0"
    }
  }
}
