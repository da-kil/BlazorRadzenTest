# Backend configuration for ti8m BeachBreak production environment
# This file configures Terraform to store state in Azure Storage Account

# Storage account configuration for Terraform state
# This storage account should be created manually before running terraform init
# Use a separate storage account for production environment security

# Resource group for Terraform state (separate from dev)
resource_group_name  = "rg-beachbreak-tfstate-prod-swn"

# Storage account for Terraform state (must be globally unique)
storage_account_name = "sabeachbreaktfstateprodswn"

# Container for Terraform state files
container_name       = "tfstate"

# State file name for production environment
key                 = "prod/terraform.tfstate"

# Additional security settings
use_azuread_auth    = true

# Notes for setup:
# 1. Create the resource group manually:
#    az group create --name rg-beachbreak-tfstate-prod-swn --location switzerlandnorth
#
# 2. Create the storage account manually:
#    az storage account create \
#      --name sabeachbreaktfstateprodswn \
#      --resource-group rg-beachbreak-tfstate-prod-swn \
#      --location switzerlandnorth \
#      --sku Standard_ZRS \
#      --kind StorageV2 \
#      --https-only true \
#      --min-tls-version TLS1_2 \
#      --allow-blob-public-access false
#
# 3. Create the container:
#    az storage container create \
#      --name tfstate \
#      --account-name sabeachbreaktfstateprodswn \
#      --auth-mode login
#
# 4. Initialize Terraform:
#    terraform init -backend-config="backend.hcl"
#
# PRODUCTION NOTES:
# - Use Zone-Redundant Storage (ZRS) for production state
# - Ensure proper RBAC permissions are configured
# - Consider enabling soft delete and versioning
# - Implement proper backup and disaster recovery procedures