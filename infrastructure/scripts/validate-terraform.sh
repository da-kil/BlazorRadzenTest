#!/bin/bash

# Validate Terraform configuration for ti8m BeachBreak infrastructure
# This script performs comprehensive validation checks

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
    echo "Usage: $0 [environment] [options]"
    echo ""
    echo "Arguments:"
    echo "  environment: Environment to validate (dev, prod, or 'all')"
    echo "               If not specified, validates all environments"
    echo ""
    echo "Options:"
    echo "  --format-check : Check Terraform formatting"
    echo "  --security     : Run security validation checks"
    echo "  --cost-check   : Validate cost estimation"
    echo "  --all-checks   : Run all validation checks"
    echo "  --help         : Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                    # Validate all environments"
    echo "  $0 dev               # Validate dev environment only"
    echo "  $0 all --all-checks  # Full validation of all environments"
    echo "  $0 dev --format-check --security"
}

# Parse command line arguments
ENVIRONMENT="all"
FORMAT_CHECK=false
SECURITY_CHECK=false
COST_CHECK=false
ALL_CHECKS=false

while [[ $# -gt 0 ]]; do
    case $1 in
        dev|prod|all)
            ENVIRONMENT=$1
            shift
            ;;
        --format-check)
            FORMAT_CHECK=true
            shift
            ;;
        --security)
            SECURITY_CHECK=true
            shift
            ;;
        --cost-check)
            COST_CHECK=true
            shift
            ;;
        --all-checks)
            ALL_CHECKS=true
            FORMAT_CHECK=true
            SECURITY_CHECK=true
            COST_CHECK=true
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

# Check prerequisites
print_status "Checking prerequisites..."

if ! command -v terraform &> /dev/null; then
    print_error "Terraform is not installed"
    exit 1
fi

# Function to validate single environment
validate_environment() {
    local env=$1
    local env_dir="$SCRIPT_DIR/../environments/$env"

    print_status "🔍 Validating $env environment..."

    if [ ! -d "$env_dir" ]; then
        print_error "Environment directory does not exist: $env_dir"
        return 1
    fi

    cd "$env_dir"

    # Check required files
    print_status "Checking required files..."
    local required_files=("main.tf" "variables.tf" "terraform.tfvars" "providers.tf" "backend.hcl")
    local missing_files=()

    for file in "${required_files[@]}"; do
        if [ ! -f "$file" ]; then
            missing_files+=("$file")
        fi
    done

    if [ ${#missing_files[@]} -ne 0 ]; then
        print_error "Missing required files in $env: ${missing_files[*]}"
        return 1
    fi

    print_success "All required files present"

    # Terraform format check
    if [ "$FORMAT_CHECK" = true ]; then
        print_status "Checking Terraform formatting..."
        if terraform fmt -check -recursive .; then
            print_success "Terraform formatting is correct"
        else
            print_warning "Terraform files need formatting. Run 'terraform fmt -recursive .'"
        fi
    fi

    # Terraform validation
    print_status "Running terraform validate..."
    if [ -d ".terraform" ]; then
        if terraform validate; then
            print_success "Terraform configuration is valid"
        else
            print_error "Terraform validation failed"
            return 1
        fi
    else
        print_warning "Terraform not initialized. Skipping terraform validate."
        print_status "Run 'init-terraform.sh $env' to initialize"
    fi

    # Security checks
    if [ "$SECURITY_CHECK" = true ]; then
        print_status "Running security checks..."

        # Check for hardcoded secrets
        print_status "Checking for potential hardcoded secrets..."
        if grep -r -i -E "(password|secret|key)\s*=\s*[\"'][^\"']*[\"']" . --exclude-dir=.terraform 2>/dev/null; then
            print_warning "Potential hardcoded secrets found. Please review."
        else
            print_success "No obvious hardcoded secrets found"
        fi

        # Check for public access configurations
        print_status "Checking for public access configurations..."
        if grep -r -i -E "(public_network_access_enabled\s*=\s*true|allow_blob_public_access\s*=\s*true)" . --exclude-dir=.terraform 2>/dev/null; then
            print_warning "Public access enabled configurations found. Please review for security."
        else
            print_success "No public access configurations found"
        fi

        # Check for HTTPS enforcement
        print_status "Checking HTTPS enforcement..."
        if grep -r -i -E "https_traffic_only_enabled\s*=\s*false" . --exclude-dir=.terraform 2>/dev/null; then
            print_error "HTTPS traffic only is disabled. This is a security risk."
            return 1
        else
            print_success "HTTPS enforcement configured correctly"
        fi
    fi

    # Cost validation
    if [ "$COST_CHECK" = true ]; then
        print_status "Validating cost configuration..."

        # Check for cost-effective SKUs in dev
        if [ "$env" = "dev" ]; then
            if grep -q "B_Standard_B" terraform.tfvars; then
                print_success "Cost-effective PostgreSQL SKU configured for dev"
            else
                print_warning "Consider using burstable PostgreSQL SKUs for dev environment"
            fi

            if grep -q "Basic" terraform.tfvars; then
                print_success "Basic Container Registry SKU configured for dev"
            else
                print_warning "Consider using Basic Container Registry SKU for dev environment"
            fi
        fi

        # Check for production-appropriate SKUs in prod
        if [ "$env" = "prod" ]; then
            if grep -q "GP_Standard_D" terraform.tfvars; then
                print_success "Production PostgreSQL SKU configured"
            else
                print_warning "Consider General Purpose PostgreSQL SKUs for production"
            fi

            if grep -q "Premium" terraform.tfvars; then
                print_success "Premium Container Registry SKU configured for prod"
            else
                print_warning "Consider Premium Container Registry SKU for production"
            fi
        fi
    fi

    print_success "✅ $env environment validation completed"
    return 0
}

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

# Main validation logic
print_status "🚀 Starting Terraform validation..."
print_status "Environment: $ENVIRONMENT"
print_status "Format check: $FORMAT_CHECK"
print_status "Security check: $SECURITY_CHECK"
print_status "Cost check: $COST_CHECK"
echo ""

VALIDATION_ERRORS=0

if [ "$ENVIRONMENT" = "all" ]; then
    # Validate all environments
    for env in dev prod; do
        if ! validate_environment "$env"; then
            VALIDATION_ERRORS=$((VALIDATION_ERRORS + 1))
        fi
        echo ""
    done
else
    # Validate specific environment
    if ! validate_environment "$ENVIRONMENT"; then
        VALIDATION_ERRORS=$((VALIDATION_ERRORS + 1))
    fi
fi

# Validate shared configuration
print_status "🔍 Validating shared configuration..."
SHARED_DIR="$SCRIPT_DIR/../shared"

if [ -d "$SHARED_DIR" ]; then
    cd "$SHARED_DIR"

    # Check shared files exist
    if [ -f "locals.tf" ] && [ -f "variables.tf" ]; then
        print_success "Shared configuration files found"
    else
        print_error "Missing shared configuration files"
        VALIDATION_ERRORS=$((VALIDATION_ERRORS + 1))
    fi

    # Terraform format check for shared files
    if [ "$FORMAT_CHECK" = true ]; then
        print_status "Checking shared file formatting..."
        if terraform fmt -check .; then
            print_success "Shared files formatting is correct"
        else
            print_warning "Shared files need formatting"
        fi
    fi
else
    print_error "Shared configuration directory not found"
    VALIDATION_ERRORS=$((VALIDATION_ERRORS + 1))
fi

# Validate module links
print_status "🔍 Validating module links..."
MODULES_DIR="$SCRIPT_DIR/../modules"

if [ -d "$MODULES_DIR" ]; then
    cd "$MODULES_DIR"

    # Check required modules exist
    required_modules=("azure_kubernetes_cluster" "container_registry" "key_vault" "postgres_flexible_server" "storage_account" "log_analytics_workspace")
    missing_modules=()

    for module in "${required_modules[@]}"; do
        if [ ! -d "$module" ]; then
            missing_modules+=("$module")
        fi
    done

    if [ ${#missing_modules[@]} -ne 0 ]; then
        print_error "Missing required modules: ${missing_modules[*]}"
        VALIDATION_ERRORS=$((VALIDATION_ERRORS + 1))
    else
        print_success "All required modules found"
    fi
else
    print_error "Modules directory not found"
    VALIDATION_ERRORS=$((VALIDATION_ERRORS + 1))
fi

# Final summary
echo ""
print_status "📊 Validation Summary:"
if [ $VALIDATION_ERRORS -eq 0 ]; then
    print_success "🎉 All validations passed successfully!"
    print_status "Infrastructure configuration is ready for deployment"
else
    print_error "❌ Validation completed with $VALIDATION_ERRORS error(s)"
    print_status "Please fix the errors before deploying"
    exit 1
fi

print_success "Validation completed!"