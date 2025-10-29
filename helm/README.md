# BeachBreak Helm Charts

This directory contains Helm charts for deploying the BeachBreak application components to Kubernetes (AKS).

## Overview

The BeachBreak application follows CQRS (Command Query Responsibility Segregation) architecture with three main components:

1. **CommandApi**: Handles write operations and commands (event sourcing)
2. **QueryApi**: Handles read operations and queries (read models)
3. **Frontend**: Blazor Server + WebAssembly frontend

Each component has its own Helm chart with production-ready configurations.

## Directory Structure

```
helm/
└── charts/
    ├── beachbreak-commandapi/       # Command API chart
    │   ├── Chart.yaml              # Chart metadata
    │   ├── values.yaml             # Default values
    │   ├── values-dev.yaml         # Dev environment overrides
    │   ├── values-production.yaml  # Production environment overrides
    │   └── templates/              # Kubernetes manifests
    │       ├── deployment.yaml     # Deployment resource
    │       ├── service.yaml        # Service resource
    │       ├── serviceaccount.yaml # Service account
    │       ├── ingress.yaml        # Ingress rules
    │       ├── hpa.yaml           # Horizontal Pod Autoscaler
    │       ├── pdb.yaml           # Pod Disruption Budget
    │       ├── secretproviderclass.yaml  # Key Vault CSI
    │       ├── configmap.yaml     # Configuration
    │       ├── networkpolicy.yaml # Network policies
    │       └── _helpers.tpl       # Template helpers
    ├── beachbreak-queryapi/        # Query API chart
    │   └── (same structure as commandapi)
    └── beachbreak-frontend/        # Frontend chart
        └── (same structure as commandapi)
```

## Chart Components

### Common Resources (All Charts)

Each chart includes the following Kubernetes resources:

1. **Deployment**: Application pods with:
   - Multi-replica configuration (2+ replicas)
   - Health probes (liveness, readiness, startup)
   - Resource limits and requests
   - Security context (non-root, read-only filesystem)
   - Pod anti-affinity for high availability
   - Node affinity for dedicated node pools

2. **Service**: ClusterIP service for internal communication

3. **ServiceAccount**: With Workload Identity annotations

4. **Ingress**: NGINX ingress with:
   - TLS termination (Let's Encrypt)
   - Rate limiting
   - SSL redirect
   - Custom routing rules

5. **HorizontalPodAutoscaler (HPA)**: Auto-scaling based on:
   - CPU utilization (70% target)
   - Memory utilization (80% target)

6. **PodDisruptionBudget (PDB)**: Ensures minimum availability during updates

7. **SecretProviderClass**: Azure Key Vault CSI driver integration

8. **ConfigMap**: Application configuration

9. **NetworkPolicy**: Micro-segmentation rules

### Chart-Specific Details

#### beachbreak-commandapi

**Purpose**: Handles write operations, commands, and event sourcing

**Key Configuration**:
- Node selector: `nodepool-type: application`
- Resources: 250m-1000m CPU, 512Mi-1Gi memory
- Replicas: 2-10 (auto-scaled)
- Health endpoints: `/health/live`, `/health/ready`, `/health/startup`
- Database: PostgreSQL event store (connection from Key Vault)
- Port: 8080
- Ingress: `commandapi.beachbreak-{env}.ti8m.com`

**Secrets from Key Vault**:
- `PostgreSQL-EventStore-ConnectionString` → `ConnectionStrings__EventStore`
- `ApplicationInsights-ConnectionString` → `ApplicationInsights__ConnectionString`

**Network Policy**:
- **Ingress**: Allow from Frontend pods
- **Egress**: Allow HTTPS (443) and PostgreSQL (5432)

#### beachbreak-queryapi

**Purpose**: Handles read operations and query models

**Key Configuration**:
- Node selector: `nodepool-type: application`
- Resources: 250m-1000m CPU, 512Mi-1Gi memory
- Replicas: 2-10 (auto-scaled)
- Health endpoints: `/health/live`, `/health/ready`, `/health/startup`
- Database: PostgreSQL read models (connection from Key Vault)
- Port: 8080
- Ingress: `queryapi.beachbreak-{env}.ti8m.com`

**Secrets from Key Vault**:
- `PostgreSQL-ReadModels-ConnectionString` → `ConnectionStrings__ReadModels`
- `ApplicationInsights-ConnectionString` → `ApplicationInsights__ConnectionString`

**Network Policy**:
- **Ingress**: Allow from Frontend pods
- **Egress**: Allow HTTPS (443) and PostgreSQL (5432)

#### beachbreak-frontend

**Purpose**: Blazor Server + WebAssembly frontend

**Key Configuration**:
- Node selector: `nodepool-type: frontend`
- Resources: 200m-500m CPU, 256Mi-512Mi memory
- Replicas: 2-8 (auto-scaled)
- Health endpoints: `/health/live`, `/health/ready`, `/health/startup`
- Port: 8080
- Ingress: `beachbreak-{env}.ti8m.com`

**Secrets from Key Vault**:
- `ApplicationInsights-ConnectionString` → `ApplicationInsights__ConnectionString`
- API endpoint URLs (CommandApi, QueryApi)

**Network Policy**:
- **Ingress**: Allow from NGINX Ingress
- **Egress**: Allow HTTPS (443) to CommandApi and QueryApi

## Prerequisites

### Required Tools

```bash
# Helm >= 3.0
helm version

# kubectl
kubectl version --client

# Azure CLI
az --version
```

### Required Kubernetes Resources

Before deploying the charts, ensure these are installed:

1. **NGINX Ingress Controller**:
   ```bash
   helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
   helm install ingress-nginx ingress-nginx/ingress-nginx \
     --namespace ingress-nginx \
     --create-namespace \
     --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz
   ```

2. **cert-manager** (for Let's Encrypt TLS):
   ```bash
   helm repo add jetstack https://charts.jetstack.io
   helm install cert-manager jetstack/cert-manager \
     --namespace cert-manager \
     --create-namespace \
     --set installCRDs=true

   # Create ClusterIssuer
   kubectl apply -f - <<EOF
   apiVersion: cert-manager.io/v1
   kind: ClusterIssuer
   metadata:
     name: letsencrypt-prod
   spec:
     acme:
       server: https://acme-v02.api.letsencrypt.org/directory
       email: platform@ti8m.com
       privateKeySecretRef:
         name: letsencrypt-prod
       solvers:
       - http01:
           ingress:
             class: nginx
   EOF
   ```

3. **Azure Key Vault CSI Driver** (installed by AKS add-on):
   ```bash
   az aks enable-addons \
     --resource-group rg-beachbreak-dev \
     --name aks-beachbreak-dev \
     --addons azure-keyvault-secrets-provider
   ```

4. **Metrics Server** (for HPA, usually pre-installed):
   ```bash
   kubectl get deployment metrics-server -n kube-system
   ```

## Installation

### 1. Create Namespace

```bash
kubectl create namespace beachbreak
kubectl label namespace beachbreak name=beachbreak
```

### 2. Get Required Values from Terraform

```bash
# Navigate to terraform environment
cd ../terraform/environments/dev

# Get outputs
TENANT_ID=$(terraform output -raw tenant_id)
KEYVAULT_NAME=$(terraform output -raw key_vault_name)
COMMANDAPI_IDENTITY=$(terraform output -raw commandapi_identity_client_id)
QUERYAPI_IDENTITY=$(terraform output -raw queryapi_identity_client_id)
FRONTEND_IDENTITY=$(terraform output -raw frontend_identity_client_id)
ACR_LOGIN_SERVER=$(terraform output -raw acr_login_server)
```

### 3. Install CommandApi

```bash
cd ../../../helm/charts/beachbreak-commandapi

helm upgrade --install commandapi . \
  --namespace beachbreak \
  --create-namespace \
  --values values-dev.yaml \
  --set image.repository=$ACR_LOGIN_SERVER/beachbreak/commandapi \
  --set image.tag=latest \
  --set serviceAccount.annotations."azure\.workload\.identity/client-id"=$COMMANDAPI_IDENTITY \
  --set keyVault.name=$KEYVAULT_NAME \
  --set keyVault.tenantId=$TENANT_ID \
  --wait --timeout 5m
```

### 4. Install QueryApi

```bash
cd ../beachbreak-queryapi

helm upgrade --install queryapi . \
  --namespace beachbreak \
  --values values-dev.yaml \
  --set image.repository=$ACR_LOGIN_SERVER/beachbreak/queryapi \
  --set image.tag=latest \
  --set serviceAccount.annotations."azure\.workload\.identity/client-id"=$QUERYAPI_IDENTITY \
  --set keyVault.name=$KEYVAULT_NAME \
  --set keyVault.tenantId=$TENANT_ID \
  --wait --timeout 5m
```

### 5. Install Frontend

```bash
cd ../beachbreak-frontend

helm upgrade --install frontend . \
  --namespace beachbreak \
  --values values-dev.yaml \
  --set image.repository=$ACR_LOGIN_SERVER/beachbreak/frontend \
  --set image.tag=latest \
  --set serviceAccount.annotations."azure\.workload\.identity/client-id"=$FRONTEND_IDENTITY \
  --set keyVault.name=$KEYVAULT_NAME \
  --set keyVault.tenantId=$TENANT_ID \
  --wait --timeout 5m
```

### 6. Verify Deployment

```bash
# Check pods
kubectl get pods -n beachbreak

# Check services
kubectl get svc -n beachbreak

# Check ingress
kubectl get ingress -n beachbreak

# Check HPA
kubectl get hpa -n beachbreak

# View logs
kubectl logs -n beachbreak -l app=commandapi --tail=50 -f
```

## Configuration

### values.yaml Structure

Each chart has three values files:

1. **values.yaml**: Base configuration (used by all environments)
2. **values-dev.yaml**: Development overrides
3. **values-production.yaml**: Production overrides

### Common Configuration Options

```yaml
# Replica configuration
replicaCount: 2

# Image configuration
image:
  repository: acrbeachbreakdev.azurecr.io/beachbreak/commandapi
  pullPolicy: IfNotPresent
  tag: "latest"

# Service account with Workload Identity
serviceAccount:
  create: true
  annotations:
    azure.workload.identity/client-id: "CLIENT_ID_HERE"
  name: "commandapi"

# Resource limits
resources:
  limits:
    cpu: 1000m
    memory: 1Gi
  requests:
    cpu: 250m
    memory: 512Mi

# Auto-scaling
autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70
  targetMemoryUtilizationPercentage: 80

# Health probes
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10

# Key Vault integration
keyVault:
  enabled: true
  name: "kv-beachbreak-dev"
  tenantId: "TENANT_ID_HERE"
  secrets:
    - secretName: PostgreSQL-EventStore-ConnectionString
      envVarName: ConnectionStrings__EventStore
```

### Environment-Specific Overrides

**values-dev.yaml**:
```yaml
replicaCount: 1
autoscaling:
  minReplicas: 1
  maxReplicas: 5
resources:
  requests:
    cpu: 100m
    memory: 256Mi
```

**values-production.yaml**:
```yaml
replicaCount: 3
autoscaling:
  minReplicas: 3
  maxReplicas: 20
resources:
  requests:
    cpu: 500m
    memory: 1Gi
podDisruptionBudget:
  minAvailable: 2
```

## Common Operations

### Update Application

```bash
# Update CommandApi to new version
helm upgrade commandapi ./beachbreak-commandapi \
  --namespace beachbreak \
  --reuse-values \
  --set image.tag=v1.2.3

# Check rollout status
kubectl rollout status deployment/commandapi -n beachbreak

# View rollout history
kubectl rollout history deployment/commandapi -n beachbreak
```

### Rollback

```bash
# Rollback CommandApi to previous version
helm rollback commandapi -n beachbreak

# Or rollback to specific revision
helm rollback commandapi 5 -n beachbreak

# View rollback status
kubectl rollout status deployment/commandapi -n beachbreak
```

### Scale Manually

```bash
# Scale CommandApi to 5 replicas
kubectl scale deployment commandapi -n beachbreak --replicas=5

# Or via Helm
helm upgrade commandapi ./beachbreak-commandapi \
  --namespace beachbreak \
  --reuse-values \
  --set replicaCount=5
```

### Update Configuration

```bash
# Update environment variable
helm upgrade commandapi ./beachbreak-commandapi \
  --namespace beachbreak \
  --reuse-values \
  --set env[0].name=LOG_LEVEL \
  --set env[0].value=Debug

# Or edit values.yaml and upgrade
helm upgrade commandapi ./beachbreak-commandapi \
  --namespace beachbreak \
  --values values-dev.yaml
```

### View Logs

```bash
# All CommandApi pods
kubectl logs -n beachbreak -l app=commandapi --tail=100 -f

# Specific pod
kubectl logs -n beachbreak commandapi-7d8f4c9b5d-xq2hj -f

# Previous container instance (after crash)
kubectl logs -n beachbreak commandapi-7d8f4c9b5d-xq2hj --previous
```

### Debug Pods

```bash
# Describe pod
kubectl describe pod commandapi-7d8f4c9b5d-xq2hj -n beachbreak

# Get pod events
kubectl get events -n beachbreak --field-selector involvedObject.name=commandapi-7d8f4c9b5d-xq2hj

# Execute shell in pod
kubectl exec -it commandapi-7d8f4c9b5d-xq2hj -n beachbreak -- /bin/sh

# Port forward for local testing
kubectl port-forward -n beachbreak svc/commandapi 8080:80
```

### Uninstall

```bash
# Uninstall CommandApi
helm uninstall commandapi -n beachbreak

# Uninstall all charts
helm uninstall commandapi queryapi frontend -n beachbreak

# Delete namespace (including all resources)
kubectl delete namespace beachbreak
```

## Troubleshooting

### Pod Not Starting

**Check pod status**:
```bash
kubectl get pods -n beachbreak
kubectl describe pod POD_NAME -n beachbreak
```

**Common issues**:
1. Image pull error: Check ACR integration
   ```bash
   az aks check-acr --resource-group rg-beachbreak-dev --name aks-beachbreak-dev --acr acrbeachbreakdev.azurecr.io
   ```

2. CrashLoopBackOff: Check logs
   ```bash
   kubectl logs POD_NAME -n beachbreak --previous
   ```

3. Pending: Check node resources
   ```bash
   kubectl describe node NODE_NAME
   kubectl top nodes
   ```

### Key Vault Access Issues

**Check Workload Identity**:
```bash
# Verify service account annotation
kubectl get sa commandapi -n beachbreak -o yaml | grep client-id

# Check federated credentials
az identity federated-credential list \
  --identity-name commandapi-identity \
  --resource-group rg-beachbreak-dev

# Verify RBAC on Key Vault
az role assignment list \
  --scope /subscriptions/SUB_ID/resourceGroups/rg-beachbreak-dev/providers/Microsoft.KeyVault/vaults/kv-beachbreak-dev
```

**Check CSI driver**:
```bash
# Verify CSI driver pods
kubectl get pods -n kube-system | grep secrets-store

# Check SecretProviderClass
kubectl get secretproviderclass -n beachbreak
kubectl describe secretproviderclass commandapi-keyvault -n beachbreak
```

### Ingress Not Working

**Check ingress**:
```bash
# Get ingress details
kubectl get ingress -n beachbreak
kubectl describe ingress commandapi -n beachbreak

# Check NGINX ingress controller
kubectl get pods -n ingress-nginx
kubectl logs -n ingress-nginx -l app.kubernetes.io/component=controller -f
```

**Check DNS**:
```bash
# Verify DNS resolution
nslookup commandapi.beachbreak-dev.ti8m.com

# Get external IP
kubectl get svc -n ingress-nginx ingress-nginx-controller
```

**Check TLS certificate**:
```bash
# Check cert-manager
kubectl get certificate -n beachbreak
kubectl describe certificate commandapi-tls -n beachbreak

# Check certificate secret
kubectl get secret commandapi-tls -n beachbreak
```

### HPA Not Scaling

**Check HPA status**:
```bash
kubectl get hpa -n beachbreak
kubectl describe hpa commandapi -n beachbreak
```

**Check metrics**:
```bash
# Verify metrics server
kubectl get deployment metrics-server -n kube-system
kubectl top pods -n beachbreak

# If metrics unavailable, restart metrics server
kubectl rollout restart deployment metrics-server -n kube-system
```

### Database Connection Issues

**Check connection string from Key Vault**:
```bash
# Get secret from Key Vault
az keyvault secret show \
  --vault-name kv-beachbreak-dev \
  --name PostgreSQL-EventStore-ConnectionString

# Check if secret is mounted in pod
kubectl exec -it POD_NAME -n beachbreak -- env | grep ConnectionStrings
```

**Check network policy**:
```bash
kubectl get networkpolicy -n beachbreak
kubectl describe networkpolicy commandapi -n beachbreak
```

**Test PostgreSQL connectivity**:
```bash
# From pod
kubectl exec -it POD_NAME -n beachbreak -- sh
# Then inside pod:
# nc -zv psql-beachbreak-dev.postgres.database.azure.com 5432
```

## Security Best Practices

1. **Image Security**:
   - Use specific image tags, not `latest` in production
   - Scan images with Trivy or Microsoft Defender
   - Use non-root containers (already configured)
   - Enable read-only root filesystem where possible

2. **Secrets Management**:
   - Store all secrets in Azure Key Vault
   - Use Workload Identity (no service principal keys)
   - Rotate secrets regularly
   - Never commit secrets to Git

3. **Network Security**:
   - Enable network policies (already configured)
   - Use TLS for all ingress traffic
   - Implement egress filtering
   - Use private endpoints for Azure services

4. **Pod Security**:
   - Run as non-root user (UID 1000)
   - Drop all capabilities
   - Disable privilege escalation
   - Use Pod Security Standards

5. **RBAC**:
   - Follow least privilege principle
   - Use service accounts for pod identity
   - Audit RBAC regularly
   - Separate dev/prod permissions

## CI/CD Integration

See [azure-pipelines/app-cd-pipeline.yml](../../azure-pipelines/app-cd-pipeline.yml) for automated deployment pipeline.

The pipeline:
1. Builds and pushes Docker images to ACR
2. Updates Helm values with new image tags
3. Deploys to Dev environment automatically
4. Deploys to Production with manual approval

## Monitoring

### Application Insights

Each application sends telemetry to Azure Application Insights:

```bash
# View connection string
kubectl exec -it POD_NAME -n beachbreak -- env | grep ApplicationInsights

# Query logs
az monitor app-insights query \
  --app appi-beachbreak-dev \
  --analytics-query "traces | where timestamp > ago(1h) | order by timestamp desc"
```

### Kubernetes Metrics

```bash
# Pod metrics
kubectl top pods -n beachbreak

# Node metrics
kubectl top nodes

# HPA metrics
kubectl get hpa -n beachbreak -w
```

### Alerts

Configured in Terraform monitoring module:
- Pod restart count > 3
- High CPU/memory usage
- Failed health probes
- HPA at max replicas

## Performance Tuning

### Resource Limits

Adjust based on actual usage:
```yaml
resources:
  limits:
    cpu: 2000m      # Increase for CPU-bound workloads
    memory: 2Gi     # Increase for memory-intensive operations
  requests:
    cpu: 500m       # Ensures pod gets scheduled on appropriate node
    memory: 1Gi     # Reserve memory for optimal performance
```

### HPA Configuration

Tune auto-scaling behavior:
```yaml
autoscaling:
  minReplicas: 3                      # Higher minimum for high-traffic
  maxReplicas: 20                     # Increase for peak loads
  targetCPUUtilizationPercentage: 60  # Lower = more aggressive scaling
  behavior:                           # Custom scaling behavior
    scaleDown:
      stabilizationWindowSeconds: 300 # Wait 5min before scaling down
      policies:
      - type: Percent
        value: 50                     # Scale down max 50% at a time
        periodSeconds: 60
```

### Connection Pooling

Configure PostgreSQL connection pooling in application:
```yaml
env:
  - name: ConnectionStrings__EventStore
    value: "Host=...;Pooling=true;MinPoolSize=5;MaxPoolSize=100"
```

## Additional Resources

- [Helm Documentation](https://helm.sh/docs/)
- [Kubernetes Best Practices](https://kubernetes.io/docs/concepts/configuration/overview/)
- [AKS Best Practices](https://learn.microsoft.com/en-us/azure/aks/best-practices)
- [Azure Key Vault CSI Driver](https://learn.microsoft.com/en-us/azure/aks/csi-secrets-store-driver)
- [NGINX Ingress Controller](https://kubernetes.github.io/ingress-nginx/)

## Support

- **Platform Team**: platform@ti8m.com
- **Helm Issues**: Create issue in repository
- **Kubernetes Support**: [AKS Documentation](https://learn.microsoft.com/en-us/azure/aks/)

---

**Last Updated**: 2025-01-28
