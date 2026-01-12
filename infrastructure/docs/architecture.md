# Architecture Documentation

## Overview

This document describes the Azure infrastructure architecture for the BeachBreak application, including design decisions, network topology, security model, and architectural trade-offs.

## Design Principles

1. **Security First**: Private networking, encryption at rest and in transit, RBAC everywhere
2. **High Availability**: Zone redundancy in production, automatic failover for critical components
3. **Cost Optimization**: Right-sized resources for each environment, auto-scaling where appropriate
4. **Observability**: Comprehensive logging and monitoring built-in
5. **Infrastructure as Code**: All resources managed via Terraform for repeatability and version control

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Azure Subscription                           │
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐│
│  │ Resource Group: rg-beachbreak-k8s-{env}-swn-01                  ││
│  │                                                                  ││
│  │  ┌──────────────────────────────────────────────────────────┐  ││
│  │  │ Virtual Network (10.0.0.0/16)                            │  ││
│  │  │                                                          │  ││
│  │  │  ┌────────────────┐  ┌────────────────┐                │  ││
│  │  │  │ AKS Node Pool  │  │ AKS API Server │                │  ││
│  │  │  │ 10.0.0.0/22    │  │ 10.0.4.0/28    │                │  ││
│  │  │  └────────────────┘  └────────────────┘                │  ││
│  │  │                                                          │  ││
│  │  │  ┌────────────────┐  ┌────────────────┐                │  ││
│  │  │  │ Private        │  │ PostgreSQL     │                │  ││
│  │  │  │ Endpoints      │  │ Subnet         │                │  ││
│  │  │  │ 10.0.5.0/24    │  │ 10.0.6.0/24    │                │  ││
│  │  │  └────────────────┘  └────────────────┘                │  ││
│  │  └──────────────────────────────────────────────────────────┘  ││
│  │                                                                  ││
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐              ││
│  │  │ AKS        │  │ Storage    │  │ Key Vault  │              ││
│  │  │ Cluster    │  │ Account    │  │            │              ││
│  │  └────────────┘  └────────────┘  └────────────┘              ││
│  │                                                                  ││
│  │  ┌────────────┐  ┌────────────┐                                ││
│  │  │ Container  │  │ Log        │                                ││
│  │  │ Registry   │  │ Analytics  │                                ││
│  │  └────────────┘  └────────────┘                                ││
│  └─────────────────────────────────────────────────────────────────┘│
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐│
│  │ Resource Group: rg-beachbreak-db-{env}-swn-01                   ││
│  │                                                                  ││
│  │  ┌────────────────────┐                                         ││
│  │  │ PostgreSQL         │                                         ││
│  │  │ Flexible Server    │                                         ││
│  │  │ (Private)          │                                         ││
│  │  └────────────────────┘                                         ││
│  └─────────────────────────────────────────────────────────────────┘│
└───────────────────────────────────────────────────────────────────────┘
```

## Resource Group Strategy

### Decision: 2 Resource Groups per Environment

**Rationale**:
- Simplifies RBAC: App team manages k8s RG, DBA team manages db RG
- Cost tracking: Clear separation between compute/app costs vs database costs
- Lifecycle management: Can destroy/recreate dev k8s resources without affecting database
- Security: Database isolation for stricter security policies and backup management

**Alternatives Considered**:
1. Single RG: Rejected - too coarse-grained for RBAC and cost tracking
2. 4+ RGs (separate storage, Key Vault): Rejected - unnecessary complexity for this scale

## Network Architecture

### Virtual Network Design

**CIDR Allocation**:
- Dev: `10.0.0.0/16` (65,536 IPs)
- Prod: `10.10.0.0/16` (65,536 IPs)

**Subnets**:

| Subnet | CIDR | Size | Purpose | Delegation |
|--------|------|------|---------|------------|
| AKS Node Pool | 10.0.0.0/22 | 1024 | Worker nodes | None |
| AKS API Server | 10.0.4.0/28 | 16 | API server VNet integration | Microsoft.ContainerService/managedClusters |
| Private Endpoints | 10.0.5.0/24 | 256 | Storage, Key Vault, ACR | None |
| PostgreSQL | 10.0.6.0/24 | 256 | Database server | Microsoft.DBforPostgreSQL/flexibleServers |

### Private DNS Zones

| Zone | Purpose |
|------|---------|
| privatelink.postgres.database.azure.com | PostgreSQL private resolution |
| privatelink.blob.core.windows.net | Storage blob private resolution |
| privatelink.vaultcore.azure.net | Key Vault private resolution |
| privatelink.azurecr.io | Container Registry private resolution |

### Network Security

**Default Deny**: All resources configured with network ACLs set to "Deny" by default

**Private Only**:
- AKS: Private cluster (no public API endpoint)
- PostgreSQL: Accessible only via delegated subnet
- Storage: Private endpoints only
- Key Vault: Private endpoint

**NSGs**:
- Applied to AKS node pool subnet
- Applied to private endpoint subnet

## AKS Cluster Design

### Control Plane

**Private Cluster**: API server accessible only via VNet integration subnet

**API Server VNet Integration**:
- Eliminates need for private link
- Lower latency
- Simpler networking

**Azure AD Integration**:
- Managed Azure AD integration
- Azure RBAC enabled for Kubernetes authorization
- No local Kubernetes accounts

### Node Pools

**System Pool** (default):
- Dev: 2x Standard_D2s_v3 (2 vCPU, 8 GB RAM)
- Prod: 3x Standard_D4s_v3 (4 vCPU, 16 GB RAM)
- Mode: System
- Auto-scaling: Disabled (fixed size for predictability)

**User Pool**:
- Dev: 1-3x Standard_D4s_v3 (4 vCPU, 16 GB RAM)
- Prod: 3-10x Standard_D8s_v3 (8 vCPU, 32 GB RAM)
- Mode: User
- Auto-scaling: Enabled

**Rationale**:
- System pool: Fixed size for stability of system workloads (CoreDNS, kube-proxy, etc.)
- User pool: Auto-scaling for application workloads based on demand

### Network Plugin

**Azure CNI**: Native Azure networking

**Advantages**:
- Pods get IPs from VNet (direct connectivity)
- Better performance than kubenet
- Required for Azure Network Policy

**Network Policy**: Azure Network Policy (native Azure implementation)

### Workload Identity

**OIDC-based**: Using Azure AD Workload Identity

**Advantages over Pod Identity**:
- No node-level MIC (Managed Identity Controller)
- Federated credentials (more secure)
- Kubernetes standard (OIDC)

### Encryption

**etcd Encryption**: Enabled with Key Vault CMK

**Data Plane Encryption**: Azure disk encryption enabled by default

## PostgreSQL Architecture

### Server Configuration

**Version**: PostgreSQL 16 (latest)

**SKU**:
- Dev: GP_Standard_D2s_v3 (2 vCPU, 8 GB RAM)
- Prod: GP_Standard_D4s_v3 (4 vCPU, 16 GB RAM)

**Storage**:
- Dev: 128 GB (P10 tier)
- Prod: 256 GB (P15 tier)
- Auto-grow: Enabled

**High Availability**:
- Dev: Disabled (cost optimization)
- Prod: Zone-redundant HA (automatic failover)

**Backup**:
- Dev: 7 days retention, LRS
- Prod: 30 days retention, geo-redundant

### Authentication

**Dual Mode**:
- Azure AD authentication: For automated processes (preferred)
- Password authentication: For legacy tools

**Network**:
- Private subnet delegation
- No public endpoint
- TLS 1.2 minimum

### Database Design

**Schemas**:
- `events`: Event sourcing events (Marten)
- `readmodels`: Denormalized read models (Marten projections)

## Storage Architecture

### Account Configuration

**Type**: StorageV2 (general-purpose v2)

**Replication**:
- Dev: LRS (Locally Redundant Storage)
- Prod: ZRS (Zone Redundant Storage)

**Access Tier**: Hot (frequently accessed data)

**Containers**:
- `backups`: Database and application backups
- `uploads`: User-uploaded files
- `logs`: Application log archives

### Security

**Authentication**: Azure AD only (shared keys disabled)

**Network**: Private endpoints only

**Versioning**: Enabled (30-day retention)

**Soft Delete**: 30 days (blob and container level)

## Key Vault Architecture

### Configuration

**SKU**:
- Dev: Standard
- Prod: Premium (HSM-backed keys)

**Authorization**: RBAC (no access policies)

**Purge Protection**:
- Dev: Disabled (allows destroy/recreate)
- Prod: Enabled (prevents accidental deletion)

**Soft Delete**: 90 days

### Keys

- `etcd-encryption-key`: RSA 2048 for AKS etcd encryption

### Secrets

- `postgresql-admin-password`: Auto-generated during deployment
- `postgresql-connection-string`: Full connection string for applications
- `storage-connection-string`: Storage account connection
- `aspire-dashboard-token`: API key for Aspire dashboard

### RBAC Assignments

| Principal | Role | Scope |
|-----------|------|-------|
| Terraform identity | Key Vault Administrator | Key Vault |
| AKS cluster identity | Key Vault Crypto User | etcd encryption key |
| Application identity | Key Vault Secrets User | All secrets |

## Container Registry

### Configuration

**SKU**:
- Dev: Standard
- Prod: Premium (geo-replication support, zone redundancy)

**Admin Account**: Disabled (use RBAC)

**Network**: Private endpoint only

### RBAC

- Kubelet identity: AcrPull role

### Cache Rules

**MCR Cache**: Cache Microsoft Container Registry images for faster pulls

## Monitoring Architecture

### Log Analytics Workspace

**Retention**:
- Dev: 30 days
- Prod: 90 days

**Data Sources**:
- AKS: Control plane logs (api-server, controller-manager, scheduler, audit)
- PostgreSQL: Query logs, error logs
- Storage: Access logs
- Key Vault: Audit logs

### Application Insights

**Type**: Workspace-based (connected to Log Analytics)

**Instrumentation**: For .NET Aspire telemetry

**Sampling**: Adaptive sampling in production

## Managed Identities

### Cluster Identity

**Purpose**: AKS cluster operations

**Assignments**:
- Network Contributor on VNet
- Managed Identity Operator on Kubelet identity
- Key Vault Crypto User on etcd encryption key

### Kubelet Identity

**Purpose**: Node-level operations

**Assignments**:
- AcrPull on Container Registry

### Terraform Identity

**Purpose**: Infrastructure deployment

**Assignments**:
- Contributor on subscription
- Key Vault Administrator on Key Vault

## Scaling Strategy

### AKS Horizontal Pod Autoscaler (HPA)

**Metrics**: CPU and memory

**Configuration**:
```yaml
minReplicas: 2
maxReplicas: 10
targetCPUUtilizationPercentage: 70
```

### AKS Cluster Autoscaler

**User Node Pool**:
- Dev: 1-3 nodes
- Prod: 3-10 nodes

**Behavior**:
- Scale up: Aggressive (pods pending > 30s)
- Scale down: Conservative (node idle > 10 min)

### PostgreSQL Scaling

**Vertical Scaling Only**: Requires brief downtime

**Process**:
1. Schedule maintenance window
2. Update SKU in Terraform
3. Apply (triggers automatic resize)
4. Test connection

## Cost Optimization

### Development

- Fixed-size system pool (no autoscaling overhead)
- LRS storage
- Standard Key Vault
- No HA for PostgreSQL
- Smaller VM SKUs

**Estimated**: ~590 CHF/month

### Production

- Zone-redundant everything
- Auto-scaling for efficiency
- Premium Container Registry for caching
- HA PostgreSQL with geo-backup

**Estimated**: ~1790 CHF/month

### Cost Savings Opportunities

1. **Reserved Instances**: 1-year commitment saves ~30%
2. **Azure Hybrid Benefit**: If SQL Server licenses available
3. **Dev/Test Pricing**: For non-production subscriptions
4. **Auto-shutdown**: For dev environment off-hours

## Security Model

### Defense in Depth

**Layer 1: Network**
- Private cluster
- No public endpoints
- NSGs on all subnets

**Layer 2: Identity**
- Azure AD integration
- Managed identities (no credentials in code)
- RBAC everywhere

**Layer 3: Data**
- Encryption at rest (all storage)
- Encryption in transit (TLS 1.2+)
- Key Vault for secrets

**Layer 4: Monitoring**
- Audit logs for all resources
- Alerts on anomalous access
- Log Analytics retention

### Compliance

**Data Residency**: Switzerland North region

**Encryption**: FIPS 140-2 Level 2 (Premium Key Vault)

**Backup**: 30-day retention (prod), geo-redundant

## Disaster Recovery

See [disaster-recovery.md](disaster-recovery.md) for detailed DR procedures.

**RTO** (Recovery Time Objective):
- Dev: 4 hours (rebuild from scratch)
- Prod: 30 minutes (failover to HA replica)

**RPO** (Recovery Point Objective):
- Dev: 24 hours (daily backups)
- Prod: 5 minutes (continuous replication + point-in-time restore)

## Future Enhancements

1. **Multi-Region**: Active-active deployment across Switzerland regions
2. **Azure Front Door**: Global load balancing and CDN
3. **Azure Firewall**: Centralized egress filtering
4. **Service Mesh**: Istio or Linkerd for advanced traffic management
5. **GitOps**: ArgoCD or Flux for application deployment
6. **Policy as Code**: Azure Policy + OPA Gatekeeper for compliance

## References

- [Azure Well-Architected Framework](https://docs.microsoft.com/en-us/azure/architecture/framework/)
- [AKS Best Practices](https://docs.microsoft.com/en-us/azure/aks/best-practices)
- [PostgreSQL Azure Best Practices](https://docs.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-best-practices)
