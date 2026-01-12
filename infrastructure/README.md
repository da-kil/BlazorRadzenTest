# ti8m BeachBreak Azure Infrastructure

This repository contains the Infrastructure as Code (IaC) for the ti8m BeachBreak application using Terraform and ti8m's proven Azure modules.

## 🏗️ Architecture Overview

The infrastructure implements a secure, scalable Azure platform for the BeachBreak CQRS/Event Sourcing application:

- **Separate Subscriptions**: Development and Production in isolated subscriptions
- **Private Networking**: All services use private endpoints, no public internet access
- **Single-Zone Deployment**: Cost-optimized deployment in Switzerland North
- **RBAC Security**: Azure AD authentication with managed identities
- **Event Sourcing**: PostgreSQL with separate schemas for events and read models

## 📁 Directory Structure

```
infrastructure/
├── modules/                    # Symbolic links to ti8m terraform modules
│   ├── azure_kubernetes_cluster/
│   ├── container_registry/
│   ├── key_vault/
│   ├── postgres_flexible_server/
│   ├── storage_account/
│   └── ...
├── environments/
│   ├── dev/                   # Development environment
│   │   ├── main.tf           # Main orchestration
│   │   ├── networking.tf     # VNet, subnets, NSGs
│   │   ├── security.tf       # Identities, RBAC, Key Vault
│   │   ├── data-layer.tf     # PostgreSQL, Storage Account
│   │   ├── compute-layer.tf  # AKS, Container Registry
│   │   ├── terraform.tfvars  # Development values
│   │   └── backend.hcl       # State configuration
│   └── prod/                 # Production environment (similar structure)
├── shared/
│   ├── locals.tf             # Naming conventions, common config
│   └── variables.tf          # Shared variable definitions
└── scripts/
    ├── init-terraform.sh     # Initialize Terraform backend
    ├── deploy-env.sh         # Deploy environment
    └── validate-terraform.sh # Validate configuration
```

## 🎯 Infrastructure Components

### Resource Groups (per environment)
- `rg-beachbreak-compute-{env}-swn-01` - AKS, Container Registry, Log Analytics
- `rg-beachbreak-data-{env}-swn-01` - PostgreSQL, Storage Account
- `rg-beachbreak-shared-{env}-swn-01` - Key Vault, VNet, DNS zones

### Networking (10.0.0.0/16)
- **AKS Nodes**: 10.0.1.0/24 (256 addresses)
- **AKS API**: 10.0.2.0/27 (32 addresses, delegated)
- **PostgreSQL**: 10.0.3.0/27 (32 addresses, delegated)
- **Private Endpoints**: 10.0.4.0/27 (32 addresses)
- **Future Gateway**: 10.0.5.0/27 (32 addresses, reserved)

### Core Services
- **AKS**: Private cluster with workload identity and etcd encryption
- **PostgreSQL**: Flexible Server with Entra ID authentication
- **Storage Account**: Private endpoints with customer-managed encryption
- **Key Vault**: RBAC-enabled with private endpoints
- **Container Registry**: Private with cache rules for MCR

## 💰 Cost Estimates

### Development Environment (~283 CHF/month)
- AKS (2x D2s_v5): ~150 CHF
- PostgreSQL (B1ms): ~45 CHF
- Storage (LRS): ~25 CHF
- Container Registry (Basic): ~5 CHF
- Other services: ~58 CHF

### Production Environment (~840 CHF/month)
- AKS (3x D4s_v5 + 2x D2s_v5): ~480 CHF
- PostgreSQL (D2s_v3): ~180 CHF
- Storage (ZRS): ~40 CHF
- Container Registry (Premium): ~20 CHF
- Other services: ~120 CHF

## 🚀 Quick Start

### Prerequisites

1. **Azure CLI** installed and logged in
2. **Terraform** >= 1.5 installed
3. **Appropriate Azure permissions**:
   - Subscription Contributor
   - Azure AD permissions for managed identities
   - Access to create storage accounts

### Step 1: Setup Infrastructure

```bash
# Clone and navigate to infrastructure
cd infrastructure

# Initialize Terraform backend for development
./scripts/init-terraform.sh dev

# Validate configuration
./scripts/validate-terraform.sh dev --all-checks

# Deploy development environment
./scripts/deploy-env.sh dev
```

### Step 2: Configure Access

```bash
# Get AKS credentials
az aks get-credentials \
  --resource-group rg-beachbreak-compute-dev-swn-01 \
  --name aks-bb-dev-swn-01

# Test access
kubectl get nodes
```

### Step 3: Container Images

```bash
# Login to Container Registry
az acr login --name acrbbdevswn01

# Build and push images
docker build -t acrbbdevswn01.azurecr.io/beachbreak-commandapi:latest ./CommandApi
docker build -t acrbbdevswn01.azurecr.io/beachbreak-queryapi:latest ./QueryApi
docker build -t acrbbdevswn01.azurecr.io/beachbreak-frontend:latest ./Frontend

docker push acrbbdevswn01.azurecr.io/beachbreak-commandapi:latest
docker push acrbbdevswn01.azurecr.io/beachbreak-queryapi:latest
docker push acrbbdevswn01.azurecr.io/beachbreak-frontend:latest
```

## 🔧 Configuration

### Environment-Specific Values

Edit `environments/{env}/terraform.tfvars` to customize:

```hcl
# Security - Add your Azure AD groups
admin_group_object_ids = [
  "12345678-1234-1234-1234-123456789012"  # Your AKS admin group
]

# Network - Add emergency access IPs
allowed_ip_ranges = [
  "203.0.113.0/24"  # Your office IP range
]

# Sizing - Adjust for your needs
aks_config = {
  default_node_pool = {
    vm_size    = "Standard_D2s_v5"
    node_count = 2
    min_count  = 1
    max_count  = 3
  }
}
```

## 🛠️ Management Commands

### Deployment Commands

```bash
# Plan only (no changes)
./scripts/deploy-env.sh dev --plan-only

# Deploy with auto-approval
./scripts/deploy-env.sh dev --auto-approve

# Deploy specific component
./scripts/deploy-env.sh dev --target=module.aks_cluster

# Destroy development environment
./scripts/deploy-env.sh dev --destroy
```

### Validation Commands

```bash
# Validate all environments
./scripts/validate-terraform.sh all

# Full validation with all checks
./scripts/validate-terraform.sh dev --all-checks

# Format check only
./scripts/validate-terraform.sh dev --format-check
```

## 🔒 Security Features

### Network Security
- **Private Endpoints**: All services isolated from public internet
- **Network Security Groups**: Restrictive rules for each subnet
- **Private DNS Zones**: Automatic registration for private endpoints

### Identity & Access Management
- **Managed Identities**: No stored credentials or secrets
- **RBAC**: Role-based access control throughout
- **Workload Identity**: Kubernetes pods use Azure AD identities
- **Key Vault Integration**: Centralized secrets management

### Data Protection
- **Encryption at Rest**: Customer-managed keys where supported
- **Encryption in Transit**: TLS 1.2+ enforced everywhere
- **Backup**: Automated backups with configurable retention
- **Audit Logging**: Comprehensive logging to Log Analytics

## 🏗️ Architecture Decisions

### Single-Zone Strategy
- **Cost Optimization**: ~50% savings vs zone-redundant deployment
- **Acceptable Risk**: Suitable for non-critical workloads
- **Recovery**: Database geo-redundant backups provide disaster recovery

### Private Networking
- **Security First**: No public endpoints, private connectivity only
- **Compliance**: Meets enterprise security requirements
- **Access**: Emergency access via allowed IP ranges only

### Resource Group Separation
- **Logical Isolation**: Separate compute, data, and shared resources
- **RBAC Granularity**: Different permissions per resource type
- **Cost Management**: Clear cost allocation and budgeting

## 🧪 Testing & Validation

### Infrastructure Testing

```bash
# Test networking
kubectl run test-pod --image=busybox --command -- sleep 3600
kubectl exec test-pod -- nslookup postgres-server-name.postgres.database.azure.com

# Test database connectivity
kubectl run postgres-test --image=postgres:16 --rm -it --restart=Never -- \
  psql "host=psql-bb-dev-swn-01.postgres.database.azure.com user=<client-id> dbname=events sslmode=require"

# Test storage access
kubectl run storage-test --image=mcr.microsoft.com/azure-cli --rm -it --restart=Never -- \
  az storage blob list --account-name stbbdevswn01 --container-name application-logs --auth-mode login
```

### Application Deployment Test

```bash
# Apply Kubernetes manifests (example)
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/command-api.yaml
kubectl apply -f k8s/query-api.yaml
kubectl apply -f k8s/frontend.yaml

# Check pod status
kubectl get pods -n beachbreak
kubectl logs -n beachbreak deployment/command-api
```

## 🚨 Troubleshooting

### Common Issues

**1. AKS Private Cluster Access**
```bash
# Ensure you're connected to the right subscription
az account show

# Get fresh credentials
az aks get-credentials --resource-group rg-beachbreak-compute-dev-swn-01 --name aks-bb-dev-swn-01 --overwrite-existing

# Test with public FQDN first
kubectl get nodes --server=https://aks-bb-dev-swn-01-xyz.hcp.switzerlandnorth.azmk8s.io
```

**2. PostgreSQL Connection Issues**
```bash
# Check managed identity client ID
az identity show --resource-group rg-beachbreak-compute-dev-swn-01 --name umi-commandapi-dev-swn-01 --query clientId -o tsv

# Test connection with Azure CLI
az postgres flexible-server execute --name psql-bb-dev-swn-01 --database-name events --querytext "SELECT version();"
```

**3. Container Registry Access**
```bash
# Check AKS identity has AcrPull role
az role assignment list --assignee <kubelet-identity-principal-id> --scope <acr-resource-id>

# Test manual docker login
az acr login --name acrbbdevswn01
docker pull acrbbdevswn01.azurecr.io/beachbreak-commandapi:latest
```

**4. Terraform State Issues**
```bash
# Check backend configuration
terraform init -backend-config=backend.hcl -reconfigure

# Validate state access
terraform state list

# Force unlock if needed (use carefully!)
terraform force-unlock <lock-id>
```

## 📚 Additional Resources

### ti8m Terraform Modules
- [Module Documentation](./modules/README.md)
- [ti8m Standards](https://docs.ti8m.ch/terraform/)
- [Azure Naming Conventions](https://docs.ti8m.ch/azure/naming/)

### Azure Documentation
- [AKS Private Clusters](https://docs.microsoft.com/en-us/azure/aks/private-clusters)
- [PostgreSQL Flexible Server](https://docs.microsoft.com/en-us/azure/postgresql/flexible-server/)
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/)

### BeachBreak Application
- [Application Architecture](../docs/architecture.md)
- [CQRS/Event Sourcing](../docs/cqrs.md)
- [Deployment Guide](../docs/deployment.md)

## 🤝 Support

For infrastructure issues:
1. Check this README and troubleshooting section
2. Validate configuration: `./scripts/validate-terraform.sh --all-checks`
3. Review Terraform plan: `./scripts/deploy-env.sh <env> --plan-only`
4. Check Azure portal for resource status
5. Contact the platform team with specific error messages

---

**Last Updated**: January 2026
**Version**: 1.0
**Maintained By**: ti8m Platform Team