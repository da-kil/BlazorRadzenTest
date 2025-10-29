# Backend configuration for Dev environment
# Use this file with: terraform init -backend-config="backend-dev.hcl"

resource_group_name  = "rg-terraform-state-dev"
storage_account_name = "sttfstatebeachbreakdev"
container_name       = "tfstate"
key                  = "dev/beachbreak.terraform.tfstate"

# Note: Create these resources manually before running terraform init:
# 1. Resource group: rg-terraform-state-dev
# 2. Storage account: sttfstatebeachbreakdev (must be globally unique)
# 3. Container: tfstate
# 4. Enable versioning on the storage account

# Azure CLI commands to create backend storage:
# az group create --name rg-terraform-state-dev --location westeurope
# az storage account create --name sttfstatebeachbreakdev --resource-group rg-terraform-state-dev --location westeurope --sku Standard_LRS --encryption-services blob
# az storage container create --name tfstate --account-name sttfstatebeachbreakdev
# az storage account blob-service-properties update --account-name sttfstatebeachbreakdev --enable-versioning true
