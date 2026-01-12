# Backend configuration for ti8m BeachBreak development environment
# This file configures Terraform to store state in Azure Storage Account

# Storage account configuration for Terraform state
# This storage account should be created manually before running terraform init

# Resource group for Terraform state (shared across environments)
resource_group_name  = "rg-beachbreak-tfstate-swn"

# Storage account for Terraform state (must be globally unique)
storage_account_name = "sabeachbreaktfstateswn"

# Container for Terraform state files
container_name       = "tfstate"

# State file name for development environment
key                 = "dev/terraform.tfstate"

# Additional security settings
use_azuread_auth    = true

# Notes for setup:
# 1. Create the resource group manually:
#    az group create --name rg-beachbreak-tfstate-swn --location switzerlandnorth
#
# 2. Create the storage account manually:
#    az storage account create \
#      --name sabeachbreaktfstateswn \
#      --resource-group rg-beachbreak-tfstate-swn \
#      --location switzerlandnorth \
#      --sku Standard_LRS \
#      --kind StorageV2 \
#      --https-only true \
#      --min-tls-version TLS1_2
#
# 3. Create the container:
#    az storage container create \
#      --name tfstate \
#      --account-name sabeachbreaktfstateswn \
#      --auth-mode login
#
# 4. Initialize Terraform:
#    terraform init -backend-config="backend.hcl"