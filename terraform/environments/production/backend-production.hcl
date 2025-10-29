# Backend configuration for Production environment
# Use this file with: terraform init -backend-config="backend-production.hcl"

resource_group_name  = "rg-terraform-state-prod"
storage_account_name = "sttfstatebeachbreakprod"
container_name       = "tfstate"
key                  = "production/beachbreak.terraform.tfstate"

# Note: Create these resources manually before running terraform init:
# 1. Resource group: rg-terraform-state-prod
# 2. Storage account: sttfstatebeachbreakprod (must be globally unique)
# 3. Container: tfstate
# 4. Enable versioning and soft delete on the storage account
# 5. Enable blob encryption and secure transfer

# Azure CLI commands to create backend storage:
# az group create --name rg-terraform-state-prod --location westeurope
# az storage account create --name sttfstatebeachbreakprod --resource-group rg-terraform-state-prod --location westeurope --sku Standard_GRS --encryption-services blob --https-only true
# az storage container create --name tfstate --account-name sttfstatebeachbreakprod
# az storage account blob-service-properties update --account-name sttfstatebeachbreakprod --enable-versioning true --enable-delete-retention true --delete-retention-days 30
