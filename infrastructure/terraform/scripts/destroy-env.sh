#!/bin/bash
# Destroy infrastructure for a specific environment (dev only!)

set -e

ENV=$1
if [ -z "$ENV" ]; then
  echo "Usage: ./destroy-env.sh <dev>"
  echo "Example: ./destroy-env.sh dev"
  exit 1
fi

if [ "$ENV" == "prod" ]; then
  echo ""
  echo "=========================================="
  echo "ERROR: Cannot destroy production environment!"
  echo "=========================================="
  echo ""
  echo "This script is intentionally restricted to prevent"
  echo "accidental destruction of production resources."
  echo ""
  echo "If you need to destroy production resources:"
  echo "  1. Ensure you have proper approval"
  echo "  2. Manually run: cd environments/prod && terraform destroy"
  echo "  3. Type the confirmation phrase when prompted"
  echo ""
  exit 1
fi

if [ "$ENV" != "dev" ]; then
  echo "Error: This script only supports 'dev' environment"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_DIR="$SCRIPT_DIR/../environments/$ENV"

if [ ! -d "$ENV_DIR" ]; then
  echo "Error: Environment directory $ENV_DIR does not exist"
  exit 1
fi

cd "$ENV_DIR"

echo ""
echo "=========================================="
echo "WARNING: DESTRUCTIVE OPERATION"
echo "=========================================="
echo ""
echo "This will PERMANENTLY DESTROY all resources in the $ENV environment:"
echo "  - AKS Cluster and all workloads"
echo "  - PostgreSQL database and all data"
echo "  - Storage accounts and all data"
echo "  - Key Vault and all secrets"
echo "  - Virtual Network and all network resources"
echo "  - Container Registry and all images"
echo ""
echo "This action CANNOT be undone!"
echo ""

read -p "Type 'destroy-$ENV' to confirm destruction: " CONFIRM

if [ "$CONFIRM" != "destroy-$ENV" ]; then
  echo ""
  echo "Destruction cancelled. Confirmation phrase did not match."
  exit 0
fi

echo ""
read -p "Are you absolutely sure? Type 'yes' to proceed: " FINAL_CONFIRM

if [ "$FINAL_CONFIRM" != "yes" ]; then
  echo ""
  echo "Destruction cancelled."
  exit 0
fi

echo ""
echo "=========================================="
echo "Destroying $ENV environment..."
echo "=========================================="
echo ""

terraform destroy -auto-approve

if [ $? -eq 0 ]; then
  echo ""
  echo "=========================================="
  echo "✓ Environment destroyed successfully"
  echo "=========================================="
  echo ""
else
  echo ""
  echo "✗ Destruction failed!"
  echo "Some resources may still exist. Please check the Azure portal."
  exit 1
fi
