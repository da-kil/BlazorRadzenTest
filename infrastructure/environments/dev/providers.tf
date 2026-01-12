# Provider configurations for ti8m BeachBreak development environment

terraform {
  required_version = ">= 1.5"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.85"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.47"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.4"
    }
  }

  # Backend configuration - loaded from backend.hcl during init
  backend "azurerm" {
    # Configuration loaded from backend.hcl file during terraform init
    # terraform init -backend-config="backend.hcl"
  }
}

# Configure Azure Resource Manager Provider
provider "azurerm" {
  # Subscription ID will be set via environment variable or Azure CLI
  # subscription_id = "00000000-0000-0000-0000-000000000000"

  features {
    # Key Vault configuration
    key_vault {
      purge_soft_delete_on_destroy    = false  # Don't purge on destroy
      recover_soft_deleted_key_vaults = true   # Recover soft-deleted vaults
    }

    # Resource Group configuration
    resource_group {
      prevent_deletion_if_contains_resources = false  # Allow deletion for dev
    }

    # Virtual Machine configuration
    virtual_machine {
      delete_os_disk_on_deletion     = true   # Clean up OS disks
      graceful_shutdown             = false   # Fast shutdown for dev
      skip_shutdown_and_force_delete = false
    }

    # Storage Account configuration
    storage_account {
      # For development, we can be more permissive
    }

    # Log Analytics configuration
    log_analytics_workspace {
      permanently_delete_on_destroy = true  # Allow permanent deletion for dev
    }

    # Application Insights configuration
    application_insights {
      disable_generated_rule = false  # Keep default alert rules
    }

    # Cognitive Services configuration (if used)
    cognitive_account {
      purge_soft_delete_on_destroy = true  # Purge on destroy for dev
    }
  }
}

# Configure Azure Active Directory Provider
provider "azuread" {
  # Tenant ID will be discovered automatically or set via environment variable
  # tenant_id = "00000000-0000-0000-0000-000000000000"
}

# Configure Random Provider
provider "random" {
  # No specific configuration required
}

# Data sources for current context
data "azurerm_client_config" "current" {}
data "azuread_client_config" "current" {}