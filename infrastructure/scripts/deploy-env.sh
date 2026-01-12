#!/bin/bash

# Deploy ti8m BeachBreak infrastructure for specified environment
# This script orchestrates the deployment with proper dependency management

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
    echo "Usage: $0 <environment> [options]"
    echo ""
    echo "Arguments:"
    echo "  environment: Environment to deploy (dev, prod)"
    echo ""
    echo "Options:"
    echo "  --auto-approve    : Skip confirmation prompts and auto-approve"
    echo "  --plan-only      : Only generate and show the plan, don't apply"
    echo "  --destroy        : Destroy the infrastructure (only allowed for dev)"
    echo "  --target=resource: Target specific resource for deployment"
    echo "  --help          : Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 dev                                    # Plan and deploy dev (with confirmation)"
    echo "  $0 dev --auto-approve                     # Deploy dev without confirmation"
    echo "  $0 dev --plan-only                        # Only show the plan"
    echo "  $0 prod --target=module.aks_cluster       # Deploy only AKS cluster"
    echo "  $0 dev --destroy                          # Destroy dev environment"
    echo ""
    echo "Prerequisites:"
    echo "  - Terraform backend initialized (run init-terraform.sh first)"
    echo "  - Azure CLI logged in with appropriate permissions"
    echo "  - terraform.tfvars configured for the target environment"
}

# Parse command line arguments
ENVIRONMENT=""
AUTO_APPROVE=false
PLAN_ONLY=false
DESTROY=false
TARGET=""

while [[ $# -gt 0 ]]; do
    case $1 in
        dev|prod)
            ENVIRONMENT=$1
            shift
            ;;
        --auto-approve)
            AUTO_APPROVE=true
            shift
            ;;
        --plan-only)
            PLAN_ONLY=true
            shift
            ;;
        --destroy)
            DESTROY=true
            shift
            ;;
        --target=*)
            TARGET="${1#*=}"
            shift
            ;;
        --help)
            show_usage
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

# Validate required arguments
if [ -z "$ENVIRONMENT" ]; then
    print_error "Environment is required"
    show_usage
    exit 1
fi

# Validate environment
if [[ ! "$ENVIRONMENT" =~ ^(dev|prod)$ ]]; then
    print_error "Environment must be 'dev' or 'prod'"
    show_usage
    exit 1
fi

# Validate destroy option (only allowed for dev)
if [ "$DESTROY" = true ] && [ "$ENVIRONMENT" != "dev" ]; then
    print_error "Destroy operation is only allowed for development environment"
    exit 1
fi

# Check prerequisites
print_status "Checking prerequisites..."

# Check if Terraform is installed
if ! command -v terraform &> /dev/null; then
    print_error "Terraform is not installed"
    exit 1
fi

# Check if Azure CLI is installed and logged in
if ! command -v az &> /dev/null; then
    print_error "Azure CLI is not installed"
    exit 1
fi

if ! az account show &> /dev/null; then
    print_error "Not logged in to Azure. Please run 'az login' first"
    exit 1
fi

# Navigate to environment directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
ENV_DIR="$SCRIPT_DIR/../environments/$ENVIRONMENT"

if [ ! -d "$ENV_DIR" ]; then
    print_error "Environment directory does not exist: $ENV_DIR"
    exit 1
fi

cd "$ENV_DIR"
print_status "Working in environment directory: $ENV_DIR"

# Check if terraform.tfvars exists
if [ ! -f "terraform.tfvars" ]; then
    print_error "terraform.tfvars file not found. Please create it first."
    exit 1
fi

# Check if Terraform is initialized
if [ ! -d ".terraform" ]; then
    print_error "Terraform not initialized. Please run init-terraform.sh first."
    exit 1
fi

# Get current subscription info
CURRENT_SUBSCRIPTION=$(az account show --query "name" -o tsv)
print_status "Using Azure subscription: $CURRENT_SUBSCRIPTION"

# Display deployment information
print_status "Deployment Configuration:"
print_status "  Environment: $ENVIRONMENT"
print_status "  Operation: $(if [ "$DESTROY" = true ]; then echo "DESTROY"; else echo "DEPLOY"; fi)"
print_status "  Auto-approve: $AUTO_APPROVE"
print_status "  Plan-only: $PLAN_ONLY"
if [ -n "$TARGET" ]; then
    print_status "  Target: $TARGET"
fi

# Confirm deployment (unless auto-approve or plan-only)
if [ "$AUTO_APPROVE" = false ] && [ "$PLAN_ONLY" = false ]; then
    echo ""
    if [ "$DESTROY" = true ]; then
        print_warning "⚠️  WARNING: You are about to DESTROY the $ENVIRONMENT environment!"
        print_warning "This action will delete all resources and cannot be undone."
    else
        print_status "You are about to deploy to the $ENVIRONMENT environment."
    fi
    echo ""
    read -p "Do you want to continue? (yes/no): " -r
    if [[ ! $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
        print_status "Deployment cancelled by user"
        exit 0
    fi
fi

# Build Terraform command arguments
TF_ARGS=""
if [ -n "$TARGET" ]; then
    TF_ARGS="$TF_ARGS -target=$TARGET"
fi

# Generate execution plan
print_status "Generating Terraform execution plan..."

PLAN_FILE="${ENVIRONMENT}-$(date +%Y%m%d-%H%M%S).tfplan"

if [ "$DESTROY" = true ]; then
    terraform plan -destroy -out="$PLAN_FILE" $TF_ARGS
else
    terraform plan -out="$PLAN_FILE" $TF_ARGS
fi

if [ $? -ne 0 ]; then
    print_error "Terraform plan generation failed"
    exit 1
fi

print_success "Plan generated successfully: $PLAN_FILE"

# If plan-only, show plan and exit
if [ "$PLAN_ONLY" = true ]; then
    print_status "Plan-only mode. Showing plan details:"
    terraform show "$PLAN_FILE"
    print_success "Plan generation completed. Plan saved as: $PLAN_FILE"
    exit 0
fi

# Apply the plan
print_status "Applying Terraform plan..."

if [ "$AUTO_APPROVE" = true ]; then
    terraform apply "$PLAN_FILE"
else
    terraform apply "$PLAN_FILE"
fi

if [ $? -eq 0 ]; then
    print_success "🎉 Deployment completed successfully!"

    # Clean up plan file
    rm -f "$PLAN_FILE"

    if [ "$DESTROY" = false ]; then
        print_status "Infrastructure Summary:"
        terraform output deployment_info

        print_status ""
        print_status "💡 Next Steps:"
        if [ "$ENVIRONMENT" = "dev" ]; then
            print_status "  1. Configure kubectl: az aks get-credentials --resource-group rg-beachbreak-compute-dev-swn-01 --name aks-bb-dev-swn-01"
            print_status "  2. Test AKS access: kubectl get nodes"
            print_status "  3. Build and push container images to ACR"
            print_status "  4. Deploy BeachBreak applications to AKS"
            print_status "  5. Set up monitoring and alerts"
        else
            print_status "  1. Configure production kubectl access"
            print_status "  2. Set up production monitoring and alerting"
            print_status "  3. Configure backup policies"
            print_status "  4. Test disaster recovery procedures"
        fi

        print_status ""
        print_success "Cost Estimation:"
        terraform output cost_estimation
    else
        print_success "Infrastructure destroyed successfully"
    fi
else
    print_error "Deployment failed"
    # Keep plan file for debugging
    print_status "Plan file preserved for debugging: $PLAN_FILE"
    exit 1
fi

print_success "Operation completed!"