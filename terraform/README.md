# BeachBreak Terraform Infrastructure

This directory contains Infrastructure as Code (IaC) for deploying the BeachBreak application to Azure using Terraform.

## Directory Structure

```
terraform/
├── modules/                    # Reusable Terraform modules
│   ├── networking/            # VNet, Subnets, NSGs, Private DNS
│   ├── aks/                   # AKS cluster with 3 node pools
│   ├── postgresql/            # PostgreSQL Flexible Server
│   ├── acr/                   # Azure Container Registry
│   ├── keyvault/              # Azure Key Vault
│   ├── monitoring/            # Application Insights, Log Analytics
│   └── security/              # Managed Identities, RBAC, Workload Identity
├── environments/              # Environment-specific configurations
│   ├── dev/                   # Development environment
│   │   ├── main.tf           # Dev environment resources
│   │   ├── outputs.tf        # Dev environment outputs
│   │   └── backend-dev.hcl   # Dev backend configuration
│   └── production/            # Production environment
│       ├── main.tf           # Production resources
│       ├── outputs.tf        # Production outputs
│       └── backend-production.hcl
└── shared/                    # Shared Terraform configuration
    ├── providers.tf          # Provider configuration
    ├── backend.tf            # Backend configuration
    └── variables.tf          # Common variables
```

## Modules Overview

### 1. Networking Module (`modules/networking/`)

Creates Azure networking infrastructure:

- **Virtual Network (VNet)**: Isolated network for all resources
- **Subnets**:
  - AKS subnet for Kubernetes nodes
  - PostgreSQL subnet for database (delegated)
  - Private endpoints subnet for secure access
- **Network Security Groups (NSGs)**: Firewall rules for each subnet
- **Private DNS Zone**: For PostgreSQL private endpoints
- **Application Gateway** (optional): Load balancer with WAF

**Key Outputs**:
- `vnet_id`, `vnet_name`
- `aks_subnet_id`, `postgresql_subnet_id`, `private_endpoints_subnet_id`
- `postgresql_private_dns_zone_id`

### 2. AKS Module (`modules/aks/`)

Deploys Azure Kubernetes Service with 3 specialized node pools:

- **System Node Pool**: CoreDNS, metrics server, CSI drivers
  - Dev: 1-3 nodes (D2s_v5)
  - Production: 2-5 nodes (D4s_v5), zone-redundant
- **Application Node Pool**: CommandApi and QueryApi workloads
  - Dev: 1-5 nodes (D4s_v5)
  - Production: 2-10 nodes (D8s_v5), zone-redundant
- **Frontend Node Pool**: Blazor web application
  - Dev: 1-3 nodes (D2s_v5)
  - Production: 2-8 nodes (D4s_v5), zone-redundant

**Features**:
- Azure CNI networking for better pod-to-pod communication
- Azure Network Policy for micro-segmentation
- Container Insights for monitoring
- Key Vault Secrets Provider CSI driver
- Workload Identity support (no service principal keys)
- Azure RBAC integration
- Auto-scaler with intelligent scaling policies
- Maintenance windows (Sat/Sun 2-4 AM)

**Key Outputs**:
- `cluster_id`, `cluster_name`
- `kubelet_identity_object_id` (for ACR pull)
- `oidc_issuer_url` (for Workload Identity)

### 3. PostgreSQL Module (`modules/postgresql/`)

Deploys PostgreSQL Flexible Server for event store and read models:

- **Dev Environment**:
  - SKU: B_Standard_B2s (Burstable, 2 vCores)
  - Storage: 32 GB
  - Backup: 7 days retention
  - HA: Disabled
- **Production Environment**:
  - SKU: GP_Standard_D8s_v3 (General Purpose, 8 vCores)
  - Storage: 256 GB
  - Backup: 35 days retention, geo-redundant
  - HA: Zone-redundant

**Features**:
- Private endpoint with delegated subnet
- Private DNS integration
- Admin password stored in Key Vault
- Connection pooling optimized
- Diagnostic logging to Log Analytics
- SSL/TLS enforcement

**Key Outputs**:
- `server_id`, `server_fqdn`
- `admin_username` (from Key Vault)
- Connection strings stored in Key Vault

### 4. ACR Module (`modules/acr/`)

Deploys Azure Container Registry for Docker images:

- **Dev Environment**:
  - SKU: Premium (for private endpoints capability)
  - Zone redundancy: Disabled
  - Public access: Enabled
  - Defender: Disabled
- **Production Environment**:
  - SKU: Premium
  - Zone redundancy: Enabled
  - Public access: Disabled (private endpoints only)
  - Defender: Enabled (vulnerability scanning)

**Features**:
- Integrated with AKS via AcrPull role assignment
- Image retention policies
- Webhook integration for CI/CD
- Admin account disabled (use managed identities)
- Diagnostic logging

**Key Outputs**:
- `id`, `login_server`
- `admin_username`, `admin_password` (if admin enabled)

### 5. Key Vault Module (`modules/keyvault/`)

Deploys Azure Key Vault for secrets management:

- **Dev Environment**:
  - SKU: Standard
  - Soft delete: 7 days
  - Purge protection: Disabled
  - Network access: Allow from AKS subnet
- **Production Environment**:
  - SKU: Premium (HSM-backed keys)
  - Soft delete: 90 days
  - Purge protection: Enabled
  - Network access: Private endpoints only

**Features**:
- RBAC-based access (not access policies)
- Stores PostgreSQL admin password
- Stores connection strings
- Integration with AKS via CSI driver
- Audit logging to Log Analytics

**Key Outputs**:
- `id`, `vault_uri`
- `name`

### 6. Monitoring Module (`modules/monitoring/`)

Deploys observability stack:

- **Log Analytics Workspace**: Centralized logging
  - Dev: 30 days retention
  - Production: 90 days retention
- **Application Insights**: Distributed tracing and APM
- **Alert Rules**:
  - AKS node CPU > 80%
  - AKS node memory > 85%
  - PostgreSQL CPU > 80%
  - PostgreSQL storage > 85%
  - Failed pod count > 3

**Features**:
- Integrated with AKS Container Insights
- Application performance monitoring
- Custom metrics and logs
- Email alert notifications
- Workbook dashboards

**Key Outputs**:
- `log_analytics_workspace_id`
- `app_insights_id`, `app_insights_instrumentation_key`

### 7. Security Module (`modules/security/`)

Deploys security and identity infrastructure:

- **Managed Identities**:
  - CommandApi identity (for Key Vault access)
  - QueryApi identity (for Key Vault access)
  - Frontend identity (for Key Vault access)
- **Workload Identity**:
  - Federated credentials for Kubernetes service accounts
  - No service principal keys required
- **RBAC Assignments**:
  - Key Vault Secrets User role
  - Key Vault Secrets Officer role (for automation)
- **Azure Policy** (production only):
  - Pod security standards
  - Required labels
  - Resource limits enforcement

**Key Outputs**:
- `commandapi_identity_id`, `commandapi_identity_client_id`
- `queryapi_identity_id`, `queryapi_identity_client_id`
- `frontend_identity_id`, `frontend_identity_client_id`

## Environment Configurations

### Development Environment

Located in `environments/dev/`:

- **Purpose**: Development and testing
- **Cost**: ~€490/month
- **Characteristics**:
  - Single availability zone
  - Smaller VM sizes
  - No high availability for PostgreSQL
  - Shorter backup retention
  - Public network access enabled
  - No Microsoft Defender

**Resource Naming**: `{type}-{project}-dev`
- Examples: `aks-beachbreak-dev`, `kv-beachbreak-dev`

### Production Environment

Located in `environments/production/`:

- **Purpose**: Production workloads
- **Cost**: ~€2,010/month
- **Characteristics**:
  - Three availability zones (zone-redundant)
  - Larger VM sizes
  - High availability for all components
  - Long backup retention (35 days)
  - Private endpoints only
  - Microsoft Defender enabled
  - Azure Policy enforcement

**Resource Naming**: `{type}-{project}-production`
- Examples: `aks-beachbreak-production`, `kv-beachbreak-production`

## Prerequisites

### Tools Required

```bash
# Terraform >= 1.9.0
terraform version

# Azure CLI
az --version

# kubectl (for AKS management)
kubectl version --client

# jq (for JSON parsing)
jq --version
```

### Azure Setup

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Verify
az account show
```

## Deployment Steps

### 1. Create Terraform Backend Storage

Terraform state must be stored remotely for collaboration:

```bash
# For Development
RESOURCE_GROUP="rg-terraform-state-dev"
STORAGE_ACCOUNT="sttfstatebeachbreakdev"
CONTAINER_NAME="tfstate"
LOCATION="westeurope"

az group create --name $RESOURCE_GROUP --location $LOCATION
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS \
  --encryption-services blob
az storage container create \
  --name $CONTAINER_NAME \
  --account-name $STORAGE_ACCOUNT

# Get storage account key (save this securely)
az storage account keys list \
  --resource-group $RESOURCE_GROUP \
  --account-name $STORAGE_ACCOUNT \
  --query '[0].value' -o tsv
```

### 2. Initialize Terraform

```bash
cd terraform/environments/dev

# Initialize with backend configuration
terraform init -backend-config="backend-dev.hcl"

# For production
cd ../production
terraform init -backend-config="backend-production.hcl"
```

### 3. Plan Infrastructure Changes

```bash
# Review what will be created
terraform plan -out=tfplan

# Save the plan to a file for review
terraform show -json tfplan | jq > plan.json
```

### 4. Apply Infrastructure

```bash
# Apply with auto-approve (use with caution)
terraform apply -auto-approve

# Or apply interactively
terraform apply tfplan
```

### 5. Configure kubectl

```bash
# Get AKS credentials
az aks get-credentials \
  --resource-group rg-beachbreak-dev \
  --name aks-beachbreak-dev

# Verify connection
kubectl get nodes
kubectl get namespaces
```

### 6. Retrieve Outputs

```bash
# View all outputs
terraform output

# Get specific output
terraform output -raw commandapi_identity_client_id

# Export for use in scripts
TENANT_ID=$(terraform output -raw tenant_id)
KEYVAULT_NAME=$(terraform output -raw key_vault_name)
```

## Common Operations

### Update Infrastructure

```bash
# Pull latest changes
git pull origin main

# Review changes
terraform plan

# Apply changes
terraform apply
```

### Scale Node Pools

```bash
# Via Terraform (update variables)
cd terraform/environments/dev
# Edit main.tf to change node counts
terraform apply

# Or via Azure CLI (temporary)
az aks nodepool scale \
  --resource-group rg-beachbreak-dev \
  --cluster-name aks-beachbreak-dev \
  --name app \
  --node-count 3
```

### View State

```bash
# List all resources in state
terraform state list

# Show specific resource
terraform state show module.aks.azurerm_kubernetes_cluster.main

# Pull current state
terraform state pull > state.json
```

### Destroy Infrastructure

```bash
# DANGER: Destroys all resources
terraform destroy

# Destroy specific resource
terraform destroy -target=module.aks.azurerm_kubernetes_cluster_node_pool.application
```

## Troubleshooting

### Backend Initialization Fails

**Error**: `Failed to get existing workspaces`

**Solution**:
```bash
# Verify storage account exists
az storage account show --name sttfstatebeachbreakdev --resource-group rg-terraform-state-dev

# Verify container exists
az storage container show --name tfstate --account-name sttfstatebeachbreakdev

# Check backend-dev.hcl configuration
cat backend-dev.hcl
```

### AKS Cluster Creation Fails

**Error**: `Insufficient quota for requested VM SKU`

**Solution**:
```bash
# Check quota
az vm list-usage --location westeurope --output table

# Request quota increase
az support tickets create \
  --ticket-name "Increase VM quota" \
  --issue-type "quota" \
  --quota-type "compute-vm-cores"
```

### PostgreSQL Connection Issues

**Error**: `Could not connect to server`

**Solution**:
```bash
# Verify network rules
az postgres flexible-server firewall-rule list \
  --resource-group rg-beachbreak-dev \
  --name psql-beachbreak-dev

# Check private DNS zone linking
az network private-dns link vnet list \
  --resource-group rg-beachbreak-dev \
  --zone-name privatelink.postgres.database.azure.com
```

### Key Vault Access Denied

**Error**: `User does not have permission to perform action`

**Solution**:
```bash
# Check RBAC assignments
az role assignment list --assignee YOUR_USER_ID --scope /subscriptions/YOUR_SUB_ID

# Assign yourself Key Vault Secrets Officer role (for dev)
az role assignment create \
  --role "Key Vault Secrets Officer" \
  --assignee YOUR_USER_ID \
  --scope /subscriptions/YOUR_SUB_ID/resourceGroups/rg-beachbreak-dev/providers/Microsoft.KeyVault/vaults/kv-beachbreak-dev
```

## Security Best Practices

1. **State File Security**:
   - Store state in Azure Storage with encryption
   - Enable soft delete and versioning
   - Restrict access with RBAC
   - Never commit state files to Git

2. **Secrets Management**:
   - Store all secrets in Key Vault
   - Use Workload Identity (no service principal keys)
   - Enable Key Vault soft delete and purge protection
   - Rotate secrets regularly

3. **Network Security**:
   - Use private endpoints in production
   - Enable NSG flow logs
   - Implement network policies in AKS
   - Restrict Key Vault network access

4. **Identity & Access**:
   - Use managed identities for all workloads
   - Implement least privilege RBAC
   - Enable Azure AD RBAC for AKS
   - Disable local accounts in production

5. **Monitoring & Auditing**:
   - Enable diagnostic logs for all resources
   - Configure alert rules for critical metrics
   - Review audit logs regularly
   - Set up log retention policies

## Cost Optimization

1. **Dev/Test Pricing**:
   - Use Azure Dev/Test subscription for 20% discount
   - Enable auto-shutdown for dev environments

2. **Reserved Instances**:
   - Purchase 1-year or 3-year reservations for production
   - Save up to 72% on VM costs

3. **Spot Instances**:
   - Use spot VMs for non-critical workloads
   - Save up to 90% on compute costs

4. **Right-Sizing**:
   - Monitor actual usage with Azure Advisor
   - Scale down over-provisioned resources
   - Use burstable VMs (B-series) for dev

5. **Automation**:
   - Auto-scale node pools based on metrics
   - Schedule shutdown for dev environments (evenings/weekends)
   - Use cluster autoscaler aggressively

## Variables Reference

### Common Variables

| Variable | Type | Description | Dev Default | Prod Default |
|----------|------|-------------|-------------|--------------|
| `project_name` | string | Project name prefix | beachbreak | beachbreak |
| `environment` | string | Environment name | dev | production |
| `location` | string | Azure region | westeurope | westeurope |
| `kubernetes_version` | string | AKS K8s version | 1.29 | 1.29 |
| `availability_zones` | list(string) | AZ zones | ["1"] | ["1","2","3"] |

### AKS Variables

| Variable | Type | Description | Dev Default | Prod Default |
|----------|------|-------------|-------------|--------------|
| `system_node_vm_size` | string | System pool VM size | D2s_v5 | D4s_v5 |
| `system_node_count` | number | System pool node count | 1 | 2 |
| `app_node_vm_size` | string | App pool VM size | D4s_v5 | D8s_v5 |
| `app_node_max_count` | number | App pool max nodes | 5 | 10 |
| `frontend_node_vm_size` | string | Frontend pool VM size | D2s_v5 | D4s_v5 |

### PostgreSQL Variables

| Variable | Type | Description | Dev Default | Prod Default |
|----------|------|-------------|-------------|--------------|
| `postgresql_version` | string | PostgreSQL version | 16 | 16 |
| `sku_name` | string | PostgreSQL SKU | B_Standard_B2s | GP_Standard_D8s_v3 |
| `storage_mb` | number | Storage in MB | 32768 | 262144 |
| `backup_retention_days` | number | Backup retention | 7 | 35 |
| `high_availability_mode` | string | HA mode | "" | ZoneRedundant |

## Outputs Reference

### AKS Outputs

- `aks_cluster_id`: AKS cluster resource ID
- `aks_cluster_name`: AKS cluster name
- `aks_fqdn`: AKS cluster FQDN
- `kubelet_identity_object_id`: Kubelet identity for ACR
- `oidc_issuer_url`: OIDC issuer for Workload Identity

### PostgreSQL Outputs

- `postgresql_server_id`: PostgreSQL server resource ID
- `postgresql_fqdn`: PostgreSQL connection hostname
- `postgresql_admin_username`: Admin username (from Key Vault)

### Security Outputs

- `commandapi_identity_client_id`: CommandApi managed identity client ID
- `queryapi_identity_client_id`: QueryApi managed identity client ID
- `frontend_identity_client_id`: Frontend managed identity client ID

### Key Vault Outputs

- `key_vault_id`: Key Vault resource ID
- `key_vault_name`: Key Vault name
- `key_vault_uri`: Key Vault URI

### Monitoring Outputs

- `log_analytics_workspace_id`: Log Analytics workspace ID
- `app_insights_instrumentation_key`: Application Insights key
- `app_insights_connection_string`: Application Insights connection string

## CI/CD Integration

See [azure-pipelines/infrastructure-pipeline.yml](../../azure-pipelines/infrastructure-pipeline.yml) for automated deployment pipeline.

## Additional Resources

- [Azure Terraform Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [AKS Best Practices](https://learn.microsoft.com/en-us/azure/aks/best-practices)
- [PostgreSQL Flexible Server](https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/)

## Support

- **Platform Team**: platform@ti8m.com
- **Terraform Issues**: Create issue in repository
- **Azure Support**: [Azure Portal](https://portal.azure.com)

---

**Last Updated**: 2025-01-28
