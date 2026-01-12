# Disaster Recovery Plan

## Overview

This document outlines disaster recovery procedures for the BeachBreak infrastructure, including recovery objectives, backup strategies, and step-by-step recovery procedures.

## Recovery Objectives

### Development Environment

- **RTO** (Recovery Time Objective): 4 hours
- **RPO** (Recovery Point Objective): 24 hours
- **Strategy**: Rebuild from Terraform + restore latest daily backup

### Production Environment

- **RTO**: 30 minutes
- **RPO**: 5 minutes
- **Strategy**: Automatic failover (HA) + point-in-time restore

## Disaster Scenarios

1. **AKS Cluster Failure**: Node pool outage or control plane issue
2. **PostgreSQL Database Failure**: Database server crash or corruption
3. **Region Outage**: Entire Azure region unavailable
4. **Data Corruption**: Application bug corrupts database
5. **Security Incident**: Compromised credentials or malicious activity

## Backup Strategy

### PostgreSQL Backups

#### Automatic Backups

**Development**:
- **Frequency**: Daily
- **Retention**: 7 days
- **Type**: Full backup
- **Replication**: Locally redundant (LRS)

**Production**:
- **Frequency**: Continuous (transaction logs)
- **Retention**: 30 days
- **Type**: Full + incremental
- **Replication**: Geo-redundant (GRS)
- **High Availability**: Zone-redundant standby replica

#### Point-in-Time Restore

Restore to any point within retention period:

```bash
az postgres flexible-server restore \
  --resource-group rg-beachbreak-db-prod-swn-01 \
  --name pg-bbdb-prod-swn-restored \
  --source-server pg-bbdb-prod-swn-xxxx \
  --restore-time "2026-01-10T14:30:00Z"
```

### Storage Account Backups

#### Blob Versioning

- **Enabled**: Yes
- **Retention**: 30 days
- **Scope**: All containers

#### Soft Delete

- **Enabled**: Yes
- **Retention**: 30 days
- **Scope**: Blobs and containers

### Infrastructure as Code

- **Source**: Git repository
- **State**: Azure Storage (Terraform state)
- **Backup**: Automated daily snapshots of state storage account

## Recovery Procedures

### Scenario 1: AKS Cluster Failure

#### Symptoms

- Pods not scheduling
- Control plane unresponsive
- Nodes showing "NotReady"

#### Detection

```bash
kubectl get nodes
kubectl get pods --all-namespaces
```

#### Recovery Steps

**Option A: Automatic Recovery** (Production with HA)

1. Wait for Azure to automatically recover (typical: 5-10 minutes)
2. Monitor node status:
   ```bash
   watch kubectl get nodes
   ```
3. Verify pods are rescheduling:
   ```bash
   kubectl get pods -A -w
   ```

**Option B: Manual Recovery** (Development or severe failure)

1. **Assess damage**:
   ```bash
   az aks show --resource-group rg-beachbreak-k8s-dev-swn-01 --name aks-bb-dev-swn-01 --query provisioningState
   ```

2. **If cluster is recoverable**, restart node pools:
   ```bash
   az aks nodepool stop --resource-group rg-beachbreak-k8s-dev-swn-01 --cluster-name aks-bb-dev-swn-01 --name user
   az aks nodepool start --resource-group rg-beachbreak-k8s-dev-swn-01 --cluster-name aks-bb-dev-swn-01 --name user
   ```

3. **If cluster is unrecoverable**, rebuild from Terraform:
   ```bash
   # Backup current state
   cd infrastructure/terraform/environments/dev
   terraform state pull > state-backup.json

   # Recreate cluster resource
   terraform destroy -target=module.aks
   terraform apply -target=module.aks

   # Redeploy applications (see application runbook)
   ```

**Duration**: 15-30 minutes

### Scenario 2: PostgreSQL Database Failure

#### Symptoms

- Connection timeouts
- Database read-only mode
- Data corruption errors

#### Detection

```bash
az postgres flexible-server show \
  --resource-group rg-beachbreak-db-prod-swn-01 \
  --name pg-bbdb-prod-swn-xxxx \
  --query state
```

#### Recovery Steps

**Option A: High Availability Failover** (Production)

1. **Automatic**: Azure triggers failover to standby replica
2. **Verify connection string still works** (DNS updates automatically)
3. **Monitor failover status**:
   ```bash
   az postgres flexible-server show \
     --resource-group rg-beachbreak-db-prod-swn-01 \
     --name pg-bbdb-prod-swn-xxxx \
     --query highAvailability
   ```

**Duration**: 60-120 seconds

**Option B: Point-in-Time Restore** (Data corruption or dev)

1. **Identify corruption time**:
   ```sql
   -- Connect to database and identify last known good state
   SELECT max(created_at) FROM events.streams;
   ```

2. **Restore to new server**:
   ```bash
   # Restore to 5 minutes before corruption
   RESTORE_TIME="2026-01-10T14:25:00Z"

   az postgres flexible-server restore \
     --resource-group rg-beachbreak-db-prod-swn-01 \
     --name pg-bbdb-prod-swn-recovered \
     --source-server pg-bbdb-prod-swn-xxxx \
     --restore-time $RESTORE_TIME
   ```

3. **Verify restored data**:
   ```bash
   # Connect to recovered server
   PGHOST="pg-bbdb-prod-swn-recovered.postgres.database.azure.com"
   psql "host=$PGHOST user=psqladmin dbname=beachbreak sslmode=require"

   # Verify data integrity
   SELECT count(*) FROM events.streams;
   SELECT max(created_at) FROM events.streams;
   ```

4. **Update application connection string**:
   ```bash
   # Update Key Vault secret
   KV_NAME=$(cd infrastructure/terraform/environments/prod && terraform output -raw keyvault_name)

   az keyvault secret set \
     --vault-name $KV_NAME \
     --name postgresql-connection-string \
     --value "Host=$PGHOST;Database=beachbreak;Username=psqladmin;Password=...;SSL Mode=Require"

   # Restart application pods to pick up new connection string
   kubectl rollout restart deployment/commandapi -n beachbreak
   kubectl rollout restart deployment/queryapi -n beachbreak
   ```

**Duration**: 15-20 minutes

### Scenario 3: Region Outage

#### Symptoms

- All resources in Switzerland North unavailable
- Azure portal shows region incident

#### Current State

⚠️ **No automatic failover** - Single-region deployment

#### Recovery Steps

1. **Wait for Azure region recovery** (typical: 1-4 hours for major incidents)

2. **If extended outage (>4 hours)**, deploy to secondary region:

   ```bash
   # Option 1: Deploy to Switzerland West
   # Edit terraform variables
   cd infrastructure/terraform/environments/prod
   sed -i 's/switzerlandnorth/switzerlandwest/g' variables.tf

   # Apply (creates new resources in new region)
   terraform apply

   # Restore database from geo-redundant backup
   az postgres flexible-server geo-restore \
     --resource-group rg-beachbreak-db-prod-sww-01 \
     --name pg-bbdb-prod-sww-01 \
     --source-server /subscriptions/.../resourceGroups/rg-beachbreak-db-prod-swn-01/providers/Microsoft.DBforPostgreSQL/flexibleServers/pg-bbdb-prod-swn-xxxx \
     --location switzerlandwest

   # Update DNS to point to new region (if using custom domain)
   ```

**Duration**: 2-4 hours (plus region recovery time)

**Future Enhancement**: Active-active multi-region deployment

### Scenario 4: Data Corruption

#### Symptoms

- Application reports data inconsistencies
- Invalid data in database
- Event sourcing stream corruption

#### Detection

```sql
-- Check for orphaned read models
SELECT * FROM readmodels.questionnaire_assignments
WHERE questionnaire_id NOT IN (SELECT id FROM events.streams);

-- Check for event sequence gaps
SELECT stream_id, version
FROM events.events
ORDER BY stream_id, version;
```

#### Recovery Steps

1. **Stop application writes**:
   ```bash
   kubectl scale deployment/commandapi --replicas=0 -n beachbreak
   ```

2. **Identify corruption point**:
   - Review application logs
   - Check database audit logs
   - Identify last known good timestamp

3. **Restore database** (see Scenario 2, Option B)

4. **Replay events** (if using event sourcing):
   ```bash
   # Connect to restored database
   psql ...

   # Rebuild read models from events
   SELECT rebuild_all_projections();  -- Custom function
   ```

5. **Verify data integrity**:
   ```bash
   # Run integrity checks
   psql ... -c "SELECT verify_data_integrity();"
   ```

6. **Resume application**:
   ```bash
   kubectl scale deployment/commandapi --replicas=3 -n beachbreak
   ```

**Duration**: 30-60 minutes

### Scenario 5: Security Incident

#### Symptoms

- Unauthorized access alerts
- Suspicious activity in audit logs
- Compromised credentials

#### Immediate Actions

1. **Isolate affected resources**:
   ```bash
   # Block network access
   az keyvault update --name $KV_NAME --default-action Deny
   az storage account update --name $STORAGE_NAME --default-action Deny

   # Revoke compromised credentials
   az ad sp credential reset --id $APP_ID
   ```

2. **Rotate all secrets**:
   ```bash
   # Generate new PostgreSQL password
   az postgres flexible-server update \
     --resource-group rg-beachbreak-db-prod-swn-01 \
     --name pg-bbdb-prod-swn-xxxx \
     --admin-password "$(openssl rand -base64 32)"

   # Update Key Vault secrets
   # Rotate service principal credentials
   # Update application configuration
   ```

3. **Review audit logs**:
   ```bash
   # Key Vault access logs
   az monitor activity-log list --resource-group rg-beachbreak-k8s-prod-swn-01 --start-time 2026-01-10T00:00:00Z

   # PostgreSQL audit logs (via Log Analytics)
   # Storage access logs
   ```

4. **Restore from backup if data compromised**:
   - Follow Scenario 2 procedures
   - Restore to timestamp before incident

**Duration**: 1-2 hours

## Testing and Validation

### DR Drill Schedule

- **Development**: Monthly (full rebuild)
- **Production**: Quarterly (failover test)

### DR Drill Checklist

#### PostgreSQL Failover Test

1. [ ] Schedule maintenance window
2. [ ] Notify stakeholders
3. [ ] Take manual backup
4. [ ] Trigger failover:
   ```bash
   az postgres flexible-server restart \
     --resource-group rg-beachbreak-db-prod-swn-01 \
     --name pg-bbdb-prod-swn-xxxx \
     --failover Forced
   ```
5. [ ] Verify automatic failover (target: <2 min)
6. [ ] Test application connectivity
7. [ ] Document results
8. [ ] Update procedures if needed

#### Full Environment Rebuild (Dev)

1. [ ] Backup current state
2. [ ] Destroy environment:
   ```bash
   cd infrastructure/terraform/scripts
   ./destroy-env.sh dev
   ```
3. [ ] Rebuild from Terraform:
   ```bash
   ./init-terraform.sh dev
   ./deploy-env.sh dev
   ```
4. [ ] Restore database backup
5. [ ] Deploy applications
6. [ ] Run smoke tests
7. [ ] Measure total recovery time
8. [ ] Document lessons learned

## Recovery Time Tracking

| Scenario | Target RTO | Last Test | Actual Time | Status |
|----------|-----------|-----------|-------------|--------|
| AKS Cluster Failure | 30 min | 2026-01-05 | 22 min | ✓ Pass |
| PostgreSQL HA Failover | 2 min | 2026-01-05 | 78 sec | ✓ Pass |
| PostgreSQL Point-in-Time Restore | 20 min | 2025-12-15 | 18 min | ✓ Pass |
| Full Dev Rebuild | 4 hours | 2025-12-01 | 3.5 hours | ✓ Pass |
| Region Failover | 4 hours | Not tested | - | ⚠ Pending |

## Escalation Path

### Level 1: On-Call Engineer

- Assess incident severity
- Execute standard recovery procedures
- Document actions taken

### Level 2: Platform Team Lead

- Escalate if RTO at risk
- Approve non-standard procedures
- Coordinate with stakeholders

### Level 3: Azure Support

- Open severity A ticket
- Request Azure engineering assistance
- Track resolution progress

### Level 4: Management

- Notify for customer-impacting incidents
- Approve major changes (e.g., region failover)
- Handle external communications

## Post-Incident Review

After any disaster recovery event:

1. **Document timeline**:
   - Detection time
   - Response time
   - Resolution time
   - Root cause

2. **Calculate metrics**:
   - Actual RTO vs target
   - Actual RPO vs target
   - Data loss (if any)

3. **Identify improvements**:
   - Process changes
   - Automation opportunities
   - Documentation updates

4. **Update procedures**:
   - Reflect lessons learned
   - Update runbooks
   - Schedule training

## Emergency Contacts

- **Platform Team On-Call**: +41 XX XXX XX XX
- **Azure Support (Premium)**: +41 XX XXX XX XX
- **Database Administrator**: dba@beachbreak.ch
- **Security Team**: security@beachbreak.ch

## Related Documents

- [Architecture Documentation](architecture.md)
- [Operations Runbook](runbook.md)
- [Security Incident Response Plan](security-incident-response.md) (if exists)
