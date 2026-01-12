# Operations Runbook

This runbook provides step-by-step procedures for common operational tasks.

## Table of Contents

1. [Initial Deployment](#initial-deployment)
2. [Accessing Resources](#accessing-resources)
3. [Scaling Operations](#scaling-operations)
4. [Backup and Restore](#backup-and-restore)
5. [Monitoring](#monitoring)
6. [Troubleshooting](#troubleshooting)

## Initial Deployment

### Prerequisites

- Azure CLI installed and authenticated
- Terraform >= 1.12.2 installed
- kubectl installed
- Appropriate Azure RBAC permissions

### Deploy Dev Environment

```bash
# 1. Create backend storage (one-time)
az group create --name rg-beachbreak-tfstate-swn --location switzerlandnorth
az storage account create --name sabeachbreaktfstate --resource-group rg-beachbreak-tfstate-swn --location switzerlandnorth --sku Standard_LRS
az storage container create --name tfstate --account-name sabeachbreaktfstate

# 2. Initialize Terraform
cd infrastructure/terraform/scripts
./init-terraform.sh dev

# 3. Deploy
./deploy-env.sh dev
```

**Duration**: ~15-20 minutes

### Deploy Prod Environment

Same steps as dev, but use `prod` instead of `dev`:

```bash
./init-terraform.sh prod
./deploy-env.sh prod
```

**Duration**: ~20-25 minutes (HA components take longer)

## Accessing Resources

### Connect to AKS Cluster

```bash
# Dev
az aks get-credentials \
  --resource-group rg-beachbreak-k8s-dev-swn-01 \
  --name aks-bb-dev-swn-01 \
  --admin

# Prod
az aks get-credentials \
  --resource-group rg-beachbreak-k8s-prod-swn-01 \
  --name aks-bb-prod-swn-01 \
  --admin

# Verify
kubectl cluster-info
kubectl get nodes
```

### Connect to PostgreSQL

```bash
# Get connection details from Terraform
cd infrastructure/terraform/environments/dev  # or prod
PGHOST=$(terraform output -raw postgresql_fqdn)
PGUSER=$(terraform output -raw postgresql_admin_username)
PGPASS=$(terraform output -raw postgresql_admin_password)

# Connect from local machine (requires VPN or Azure Bastion)
psql "host=$PGHOST user=$PGUSER dbname=postgres sslmode=require"

# Connect from AKS pod
kubectl run -it --rm psql-client --image=postgres:16 --restart=Never -- \
  psql "host=$PGHOST user=$PGUSER dbname=postgres sslmode=require"
```

### Access Key Vault Secrets

```bash
# Get Key Vault name
cd infrastructure/terraform/environments/dev  # or prod
KV_NAME=$(terraform output -raw keyvault_name)

# List secrets
az keyvault secret list --vault-name $KV_NAME

# Get secret value
az keyvault secret show --vault-name $KV_NAME --name postgresql-connection-string --query value -o tsv
```

### Access Container Registry

```bash
# Get ACR details
cd infrastructure/terraform/environments/dev  # or prod
ACR_NAME=$(terraform output -raw acr_name)

# Login
az acr login --name $ACR_NAME

# List images
az acr repository list --name $ACR_NAME
```

## Scaling Operations

### Scale AKS User Node Pool

#### Method 1: Update Terraform (Recommended)

Edit `infrastructure/terraform/environments/{env}/main.tf`:

```hcl
# User node pool configuration
user_node_pool_min_count = 3  # Change minimum
user_node_pool_max_count = 8  # Change maximum
```

Apply changes:

```bash
cd infrastructure/terraform/environments/dev  # or prod
terraform plan
terraform apply
```

#### Method 2: Azure CLI (Temporary)

```bash
az aks nodepool update \
  --resource-group rg-beachbreak-k8s-dev-swn-01 \
  --cluster-name aks-bb-dev-swn-01 \
  --name user \
  --min-count 3 \
  --max-count 8
```

⚠️ **Note**: CLI changes will be reverted on next Terraform apply

### Scale Application Pods

```bash
# Scale deployment
kubectl scale deployment myapp --replicas=5 -n beachbreak

# Or update HPA
kubectl autoscale deployment myapp --min=2 --max=10 --cpu-percent=70 -n beachbreak
```

### Scale PostgreSQL (Vertical)

⚠️ **Requires downtime** (~5 minutes)

Edit `infrastructure/terraform/environments/{env}/main.tf`:

```hcl
sku_name = "GP_Standard_D4s_v3"  # Change from D2s_v3 to D4s_v3
```

Apply:

```bash
cd infrastructure/terraform/environments/dev  # or prod
terraform plan
terraform apply
```

**Schedule during maintenance window!**

## Backup and Restore

### PostgreSQL Backups

#### View Available Backups

```bash
az postgres flexible-server backup list \
  --resource-group rg-beachbreak-db-dev-swn-01 \
  --name pg-bbdb-dev-swn-xxxx
```

#### Point-in-Time Restore

```bash
# Restore to new server
az postgres flexible-server restore \
  --resource-group rg-beachbreak-db-dev-swn-01 \
  --name pg-bbdb-dev-swn-restored \
  --source-server pg-bbdb-dev-swn-xxxx \
  --restore-time "2026-01-10T14:30:00Z"
```

**Duration**: ~10-15 minutes

#### Manual Backup (pg_dump)

```bash
# From AKS pod
kubectl run -it --rm pg-backup --image=postgres:16 --restart=Never -- \
  pg_dump "host=$PGHOST user=$PGUSER dbname=beachbreak sslmode=require" > backup.sql

# Upload to storage
az storage blob upload \
  --account-name sabb... \
  --container-name backups \
  --file backup.sql \
  --name "manual-backup-$(date +%Y%m%d).sql" \
  --auth-mode login
```

### Storage Account Backups

**Automatic**: Soft delete (30 days) and versioning enabled

#### Restore Deleted Blob

```bash
# List deleted blobs
az storage blob list \
  --account-name sabb... \
  --container-name backups \
  --include d \
  --auth-mode login

# Undelete
az storage blob undelete \
  --account-name sabb... \
  --container-name backups \
  --name myfile.txt \
  --auth-mode login
```

## Monitoring

### View AKS Metrics

```bash
# Node resource usage
kubectl top nodes

# Pod resource usage
kubectl top pods -A

# View logs
kubectl logs -f deployment/myapp -n beachbreak
```

### Log Analytics Queries

Access via Azure Portal: Log Analytics Workspace > Logs

**AKS Node Health**:
```kql
KubeNodeInventory
| where TimeGenerated > ago(1h)
| summarize arg_max(TimeGenerated, *) by Computer
| project Computer, Status, KubeNodeProperties = parse_json(ClusterName)
```

**PostgreSQL Slow Queries**:
```kql
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.DBFORPOSTGRESQL"
| where Category == "PostgreSQLLogs"
| where Message contains "duration"
| project TimeGenerated, Message
| order by TimeGenerated desc
```

### Alerts

Configure in Azure Monitor:

1. **AKS Node Not Ready**: Alert when node status != Ready for > 5 min
2. **PostgreSQL CPU > 80%**: Alert when avg CPU > 80% for > 10 min
3. **Storage Capacity > 80%**: Alert when storage used > 80%
4. **Key Vault Access Denied**: Alert on 403 errors

## Troubleshooting

### AKS Pods Not Starting

```bash
# Check pod status
kubectl describe pod <pod-name> -n beachbreak

# Check events
kubectl get events -n beachbreak --sort-by='.lastTimestamp'

# Check node capacity
kubectl describe nodes | grep -A 5 "Allocated resources"

# Common issues:
# - Image pull errors: Check ACR access
# - Resource limits: Scale node pool
# - Secrets missing: Check Key Vault CSI driver
```

### PostgreSQL Connection Failures

```bash
# Test DNS resolution
kubectl run -it --rm dns-test --image=busybox --restart=Never -- \
  nslookup pg-bbdb-dev-swn-xxxx.postgres.database.azure.com

# Test network connectivity
kubectl run -it --rm nc-test --image=busybox --restart=Never -- \
  nc -zv pg-bbdb-dev-swn-xxxx.postgres.database.azure.com 5432

# Check server status
az postgres flexible-server show \
  --resource-group rg-beachbreak-db-dev-swn-01 \
  --name pg-bbdb-dev-swn-xxxx \
  --query state

# Common issues:
# - Private DNS not resolving: Check DNS zone link
# - Network security: Check NSGs and subnet delegation
# - Max connections: Increase max_connections parameter
```

### Storage Access Denied

```bash
# Check managed identity
kubectl get azureidentity -A

# Check RBAC assignment
az role assignment list \
  --scope /subscriptions/.../resourceGroups/rg-beachbreak-k8s-dev-swn-01/providers/Microsoft.Storage/storageAccounts/sabb...

# Common issues:
# - Workload identity not configured: Check federated credentials
# - RBAC not assigned: Add "Storage Blob Data Contributor" role
# - Private endpoint issue: Test from AKS pod, not local machine
```

### Terraform State Lock

```bash
# View lock
az storage blob lease show \
  --container-name tfstate \
  --name beachbreak-dev.tfstate \
  --account-name sabeachbreaktfstate

# Break lock (only if no Terraform operation is running!)
az storage blob lease break \
  --container-name tfstate \
  --name beachbreak-dev.tfstate \
  --account-name sabeachbreaktfstate
```

## Maintenance Windows

### Recommended Schedule

- **Dev**: Anytime (no SLA)
- **Prod**: Sunday 02:00-05:00 CET

### Pre-Maintenance Checklist

1. [ ] Announce maintenance in team channel
2. [ ] Create backup of PostgreSQL database
3. [ ] Test rollback procedure in dev
4. [ ] Prepare rollback plan
5. [ ] Monitor dashboards ready

### Post-Maintenance Checklist

1. [ ] Verify all pods running: `kubectl get pods -A`
2. [ ] Test application endpoints
3. [ ] Check database connectivity
4. [ ] Review error logs
5. [ ] Update status page

## Emergency Contacts

- **Platform Team**: platform@beachbreak.ch
- **On-Call**: +41 XX XXX XX XX
- **Azure Support**: Portal > Help + support

## Change Management

All infrastructure changes must:

1. Be reviewed in pull request
2. Pass Terraform plan review
3. Be approved by team lead
4. Be applied during maintenance window (prod)
5. Be documented in change log
