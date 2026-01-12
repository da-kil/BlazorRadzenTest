#!/bin/bash
# Initialize Terraform for a specific environment

set -e

ENV=$1
if [ -z "$ENV" ]; then
  echo "Usage: ./init-terraform.sh <dev|prod>"
  echo "Example: ./init-terraform.sh dev"
  exit 1
fi

if [ "$ENV" != "dev" ] && [ "$ENV" != "prod" ]; then
  echo "Error: Environment must be 'dev' or 'prod'"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_DIR="$SCRIPT_DIR/../environments/$ENV"

if [ ! -d "$ENV_DIR" ]; then
  echo "Error: Environment directory $ENV_DIR does not exist"
  exit 1
fi

cd "$ENV_DIR"

echo "=========================================="
echo "Initializing Terraform for $ENV environment"
echo "=========================================="
echo ""

echo "Step 1: Initializing Terraform backend..."
terraform init -upgrade

echo ""
echo "Step 2: Validating configuration..."
terraform validate

if [ $? -eq 0 ]; then
  echo "✓ Validation successful!"
else
  echo "✗ Validation failed!"
  exit 1
fi

echo ""
echo "Step 3: Formatting code..."
terraform fmt -recursive

echo ""
echo "=========================================="
echo "Initialization complete for $ENV environment!"
echo "=========================================="
echo ""
echo "Next steps:"
echo "  1. Review the configuration files"
echo "  2. Run: ./deploy-env.sh $ENV"
echo ""
