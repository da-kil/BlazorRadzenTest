#!/bin/bash
# Deploy infrastructure for a specific environment

set -e

ENV=$1
if [ -z "$ENV" ]; then
  echo "Usage: ./deploy-env.sh <dev|prod>"
  echo "Example: ./deploy-env.sh dev"
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
echo "Deploying infrastructure for $ENV environment"
echo "=========================================="
echo ""

echo "Step 1: Creating Terraform plan..."
terraform plan -out=tfplan

if [ $? -ne 0 ]; then
  echo "✗ Planning failed!"
  exit 1
fi

echo ""
echo "=========================================="
echo "Plan created successfully!"
echo "=========================================="
echo ""

read -p "Do you want to apply this plan? (yes/no): " CONFIRM

if [ "$CONFIRM" != "yes" ]; then
  echo "Deployment cancelled."
  exit 0
fi

echo ""
echo "Step 2: Applying Terraform plan..."
terraform apply tfplan

if [ $? -eq 0 ]; then
  echo ""
  echo "=========================================="
  echo "✓ Deployment successful!"
  echo "=========================================="
  echo ""

  echo "Step 3: Displaying outputs..."
  terraform output

  echo ""
  echo "=========================================="
  echo "Deployment Summary"
  echo "=========================================="
  echo "Environment: $ENV"
  echo "AKS Cluster: $(terraform output -raw aks_cluster_name)"
  echo "PostgreSQL: $(terraform output -raw postgresql_server_name)"
  echo "Storage Account: $(terraform output -raw storage_account_name)"
  echo "Key Vault: $(terraform output -raw keyvault_name)"
  echo ""
  echo "Next steps:"
  echo "  1. Configure kubectl: az aks get-credentials --resource-group $(terraform output -raw k8s_resource_group_name) --name $(terraform output -raw aks_cluster_name) --admin"
  echo "  2. Store secrets in Key Vault"
  echo "  3. Deploy applications to AKS"
  echo ""
else
  echo ""
  echo "✗ Deployment failed!"
  exit 1
fi
