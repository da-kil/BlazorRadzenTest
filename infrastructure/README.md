# BeachBreak Infrastructure

This directory contains the Terraform infrastructure-as-code for deploying the BeachBreak application to Azure.

## Architecture Overview

The infrastructure consists of:

- **AKS (Azure Kubernetes Service)** - Container orchestration platform
- **PostgreSQL Flexible Server** - Event sourcing database (Marten)
- **Storage Account** - Blob storage for backups and files
- **Key Vault** - Secrets and encryption key management
- **Virtual Network** - Private networking with subnets
- **Container Registry** - Private container image storage
- **Log Analytics + Application Insights** - Monitoring and diagnostics

### Resource Group Structure

**2 Resource Groups per environment:**

1. `rg-beachbreak-k8s-{env}-swn-01` - Kubernetes and application resources
   - AKS cluster
   - Storage Account
   - Key Vault
   - Container Registry
   - Log Analytics Workspace

2. `rg-beachbreak-db-{env}-swn-01` - Database resources
   - PostgreSQL Flexible Server

## Prerequisites

### Required Tools

- **Terraform** >= 1.12.2
- **Azure CLI** >= 2.50.0
- **kubectl** >= 1.31.0
- **Git**

### Azure Requirements

- Azure subscription (dev and/or prod)
- Subscription Contributor + User Access Administrator roles
- Sufficient quotas:
  - 32+ vCPU per environment
  - 500 GB+ storage per environment
  - 10+ Private Endpoints per environment

## Quick Start

### 1. Setup Terraform Backend

The first time, create the backend storage account for Terraform state:

```bash
az group create --name rg-beachbreak-tfstate-swn --location switzerlandnorth

az storage account create \
  --name sabeachbreaktfstate \
  --resource-group rg-beachbreak-tfstate-swn \
  --location switzerlandnorth \
  --sku Standard_LRS \
  --kind StorageV2

az storage container create \
  --name tfstate \
  --account-name sabeachbreaktfstate
```

### 2. Initialize Terraform

```bash
cd terraform/scripts
./init-terraform.sh dev   # or prod
```

### 3. Deploy Infrastructure

```bash
./deploy-env.sh dev   # or prod
```

### 4. Connect to AKS

```bash
az aks get-credentials \
  --resource-group rg-beachbreak-k8s-dev-swn-01 \
  --name aks-bb-dev-swn-01 \
  --admin

kubectl cluster-info
kubectl get nodes
```

### 5. Store Secrets in Key Vault

```bash
# Get Key Vault name from Terraform output
KV_NAME=$(cd terraform/environments/dev && terraform output -raw keyvault_name)

# Store PostgreSQL connection string
az keyvault secret set \
  --vault-name $KV_NAME \
  --name "postgresql-connection-string" \
  --value "Host=...;Database=beachbreak;..."
```

## Directory Structure

```
infrastructure/
├── terraform/
│   ├── environments/
│   │   ├── dev/              # Dev environment configuration
│   │   │   ├── main.tf
│   │   │   ├── providers.tf
│   │   │   ├── variables.tf
│   │   │   └── outputs.tf
│   │   └── prod/             # Prod environment configuration
│   │       ├── main.tf
│   │       ├── providers.tf
│   │       ├── variables.tf
│   │       └── outputs.tf
│   ├── modules/
│   │   ├── networking/       # VNet, subnets, DNS zones
│   │   ├── identities/       # Managed identities, RBAC
│   │   ├── monitoring/       # Log Analytics, App Insights
│   │   ├── aks-cluster/      # AKS cluster configuration
│   │   ├── postgresql/       # PostgreSQL database
│   │   ├── storage/          # Storage Account
│   │   └── keyvault/         # Key Vault
│   └── scripts/
│       ├── init-terraform.sh # Initialize environment
│       ├── deploy-env.sh     # Deploy infrastructure
│       └── destroy-env.sh    # Destroy dev environment
└── docs/
    ├── architecture.md       # Architecture decisions
    ├── runbook.md            # Operations guide
    └── disaster-recovery.md  # DR procedures
```

## Environments

### Development (dev)

**Purpose**: Development and testing

**Configuration**:
- AKS: 2 system nodes (D2s_v3) + 1-3 user nodes (D4s_v3)
- PostgreSQL: D2s_v3, 128 GB, LRS, 7-day backup, no HA
- Storage: LRS
- Key Vault: Standard SKU
- Cost: ~590 CHF/month

### Production (prod)

**Purpose**: Production workloads

**Configuration**:
- AKS: 3 system nodes (D4s_v3) + 3-10 user nodes (D8s_v3)
- PostgreSQL: D4s_v3, 256 GB, ZRS, 30-day backup, zone-redundant HA
- Storage: ZRS
- Key Vault: Premium SKU (HSM)
- Cost: ~1790 CHF/month

## Common Operations

### View Terraform Outputs

```bash
cd terraform/environments/dev  # or prod
terraform output
```

### Update Infrastructure

```bash
cd terraform/environments/dev  # or prod
terraform plan
terraform apply
```

### Scale AKS Nodes

Edit `terraform/environments/{env}/main.tf`:

```hcl
# Update node pool configuration
user_node_pool_min_count = 3  # Change from 1
user_node_pool_max_count = 5  # Change from 3
```

Then apply:

```bash
cd terraform/environments/dev
terraform apply
```

### Destroy Dev Environment

```bash
cd terraform/scripts
./destroy-env.sh dev
```

⚠️ **Warning**: This permanently destroys ALL resources and data!

## Security

### Private Network Isolation

- AKS: Private cluster (no public API endpoint)
- PostgreSQL: Private access via delegated subnet
- Storage: Private endpoints only
- Key Vault: Private endpoint + RBAC

### Encryption

- AKS etcd: Encrypted with Key Vault CMK
- PostgreSQL: TLS 1.2+ enforced
- Storage: HTTPS only
- All data encrypted at rest

### RBAC

- AKS: Azure AD integration with Azure RBAC
- Key Vault: RBAC authorization (no access policies)
- Managed Identities: For AKS cluster and kubelet
- Workload Identity: OIDC-based for pod identities

## Monitoring

### Log Analytics

All resources send diagnostic logs to Log Analytics:
- AKS control plane logs
- PostgreSQL query logs
- Storage access logs
- Key Vault audit logs

### Application Insights

Connected to Log Analytics for distributed tracing and APM.

### Alerts

Configure alerts in Azure Monitor for:
- AKS node health
- PostgreSQL CPU/memory
- Storage capacity
- Key Vault access anomalies

## Troubleshooting

### Terraform State Lock

If state is locked:

```bash
# View lock info
az storage blob lease show \
  --container-name tfstate \
  --name beachbreak-dev.tfstate \
  --account-name sabeachbreaktfstate

# Break lock (use with caution!)
az storage blob lease break \
  --container-name tfstate \
  --name beachbreak-dev.tfstate \
  --account-name sabeachbreaktfstate
```

### AKS Connection Issues

```bash
# Re-download credentials
az aks get-credentials \
  --resource-group rg-beachbreak-k8s-dev-swn-01 \
  --name aks-bb-dev-swn-01 \
  --admin \
  --overwrite-existing

# Test connectivity
kubectl cluster-info
```

### PostgreSQL Connection Issues

```bash
# Test from AKS pod
kubectl run -it --rm psql-test --image=postgres:16 --restart=Never -- \
  psql "host=pg-bbdb-dev-swn-xxxx.postgres.database.azure.com user=psqladmin dbname=postgres sslmode=require"
```

## Support

For detailed documentation:
- [Architecture](docs/architecture.md)
- [Runbook](docs/runbook.md)
- [Disaster Recovery](docs/disaster-recovery.md)

For issues or questions, contact the Platform Team.
