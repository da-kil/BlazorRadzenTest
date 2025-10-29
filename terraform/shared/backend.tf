# Remote State Backend Configuration
# This file defines where Terraform state is stored
# For production use, configure Azure Storage Account backend

terraform {
  backend "azurerm" {
    # These values should be provided via backend config file or command line
    # Example: terraform init -backend-config="backend-dev.hcl"

    # resource_group_name  = "rg-terraform-state"
    # storage_account_name = "sttfstatebeachbreak"
    # container_name       = "tfstate"
    # key                  = "beachbreak.terraform.tfstate"
  }
}

# Example backend config file (backend-dev.hcl):
# resource_group_name  = "rg-terraform-state-dev"
# storage_account_name = "sttfstatebeachbreakdev"
# container_name       = "tfstate"
# key                  = "dev/beachbreak.terraform.tfstate"
