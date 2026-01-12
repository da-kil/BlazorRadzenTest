#!/bin/bash

# Initialize Terraform backend for ti8m BeachBreak infrastructure
# This script sets up the Azure Storage backend for Terraform state

set -e  # Exit on any error

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to show usage
show_usage() {
    echo "Usage: $0 <environment> [subscription_id]"
    echo ""
    echo "Arguments:"
    echo "  environment    : Environment to initialize (dev, prod)"
    echo "  subscription_id: Azure subscription ID (optional, will use current if not provided)"
    echo ""
    echo "Examples:"
    echo "  $0 dev"
    echo "  $0 prod 12345678-1234-1234-1234-123456789012"
    echo ""
    echo "Prerequisites:"
    echo "  - Azure CLI installed and logged in"
    echo "  - Appropriate permissions to create storage accounts"
    echo "  - Terraform installed (>= 1.5)"
}

# Check arguments
if [ $# -lt 1 ] || [ $# -gt 2 ]; then
    print_error "Invalid number of arguments"
    show_usage
    exit 1
fi

ENVIRONMENT=$1
SUBSCRIPTION_ID=${2:-""}

# Validate environment
if [[ ! "$ENVIRONMENT" =~ ^(dev|prod)$ ]]; then
    print_error "Environment must be 'dev' or 'prod'"
    show_usage
    exit 1
fi

# Check prerequisites
print_status "Checking prerequisites..."

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    print_error "Azure CLI is not installed. Please install it first."
    exit 1
fi

# Check if Terraform is installed
if ! command -v terraform &> /dev/null; then
    print_error "Terraform is not installed. Please install it first."
    exit 1
fi

# Check if logged in to Azure
if ! az account show &> /dev/null; then
    print_error "Not logged in to Azure. Please run 'az login' first."
    exit 1
fi

# Set subscription if provided
if [ -n "$SUBSCRIPTION_ID" ]; then
    print_status "Setting Azure subscription to: $SUBSCRIPTION_ID"
    az account set --subscription "$SUBSCRIPTION_ID"
fi

# Get current subscription info
CURRENT_SUBSCRIPTION=$(az account show --query "id" -o tsv)
CURRENT_SUBSCRIPTION_NAME=$(az account show --query "name" -o tsv)
print_status "Using subscription: $CURRENT_SUBSCRIPTION_NAME ($CURRENT_SUBSCRIPTION)"

# Configuration
RESOURCE_GROUP="rg-beachbreak-tfstate-swn"
STORAGE_ACCOUNT="sabeachbreaktfstateswn"
CONTAINER_NAME="tfstate"
LOCATION="switzerlandnorth"

print_status "Terraform state configuration:"
print_status "  Resource Group: $RESOURCE_GROUP"
print_status "  Storage Account: $STORAGE_ACCOUNT"
print_status "  Container: $CONTAINER_NAME"
print_status "  Location: $LOCATION"

# Create resource group for Terraform state
print_status "Creating resource group for Terraform state..."
if az group show --name "$RESOURCE_GROUP" &> /dev/null; then
    print_warning "Resource group $RESOURCE_GROUP already exists"
else
    az group create \
        --name "$RESOURCE_GROUP" \
        --location "$LOCATION" \
        --tags "Purpose=TerraformState" "Project=ti8m-beachbreak"
    print_success "Created resource group: $RESOURCE_GROUP"
fi

# Create storage account for Terraform state
print_status "Creating storage account for Terraform state..."
if az storage account show --name "$STORAGE_ACCOUNT" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
    print_warning "Storage account $STORAGE_ACCOUNT already exists"
else
    az storage account create \
        --name "$STORAGE_ACCOUNT" \
        --resource-group "$RESOURCE_GROUP" \
        --location "$LOCATION" \
        --sku Standard_LRS \
        --kind StorageV2 \
        --https-only true \
        --min-tls-version TLS1_2 \
        --allow-blob-public-access false \
        --tags "Purpose=TerraformState" "Project=ti8m-beachbreak"
    print_success "Created storage account: $STORAGE_ACCOUNT"
fi

# Create container for Terraform state
print_status "Creating container for Terraform state..."
if az storage container show --name "$CONTAINER_NAME" --account-name "$STORAGE_ACCOUNT" --auth-mode login &> /dev/null; then
    print_warning "Container $CONTAINER_NAME already exists"
else
    az storage container create \
        --name "$CONTAINER_NAME" \
        --account-name "$STORAGE_ACCOUNT" \
        --auth-mode login
    print_success "Created container: $CONTAINER_NAME"
fi

# Navigate to environment directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
ENV_DIR="$SCRIPT_DIR/../environments/$ENVIRONMENT"

if [ ! -d "$ENV_DIR" ]; then
    print_error "Environment directory does not exist: $ENV_DIR"
    exit 1
fi

cd "$ENV_DIR"
print_status "Changed to environment directory: $ENV_DIR"

# Initialize Terraform
print_status "Initializing Terraform for $ENVIRONMENT environment..."

if [ ! -f "backend.hcl" ]; then
    print_error "backend.hcl file not found in $ENV_DIR"
    exit 1
fi

# Run terraform init with backend configuration
terraform init -backend-config="backend.hcl" -reconfigure

if [ $? -eq 0 ]; then
    print_success "Terraform initialized successfully for $ENVIRONMENT environment!"
    print_status "Next steps:"
    print_status "  1. Review and update terraform.tfvars file"
    print_status "  2. Run: terraform plan"
    print_status "  3. Run: terraform apply"
else
    print_error "Terraform initialization failed"
    exit 1
fi

print_success "Terraform backend initialization completed!"
print_status "State will be stored in:"
print_status "  Storage Account: $STORAGE_ACCOUNT"
print_status "  Container: $CONTAINER_NAME"
print_status "  Key: $ENVIRONMENT/terraform.tfstate"