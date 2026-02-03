# BeachBreak Kubernetes Deployment

This directory contains Helm charts and deployment scripts for the ti8m BeachBreak application on Kubernetes.

## Architecture Overview

BeachBreak is a .NET 9 CQRS/Event Sourcing application with three main components:

- **Command API**: Handles state-changing operations (commands)
- **Query API**: Handles read operations (queries)
- **Frontend**: Blazor Server with WebAssembly client
- **PostgreSQL**: Database for event sourcing and read models

## Prerequisites

### Required Tools
- [Docker](https://docs.docker.com/get-docker/) - For building container images
- [Kubernetes cluster](https://kubernetes.io/docs/setup/) - Target deployment environment
- [kubectl](https://kubernetes.io/docs/tasks/tools/) - Kubernetes command-line tool
- [Helm 3.x](https://helm.sh/docs/intro/install/) - Package manager for Kubernetes
- [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell) - For deployment scripts

### Kubernetes Requirements
- Kubernetes 1.21+
- Ingress controller (nginx recommended)
- Storage class for persistent volumes
- Optional: cert-manager for SSL certificates
- Optional: Prometheus + Grafana for monitoring

### Azure Requirements (for Authentication)
- Azure AD application registration
- Client ID and Client Secret
- Configured redirect URIs

## Quick Start

### 1. Configure Environment

Copy and edit the appropriate values file for your environment:

```bash
cp k8s/values/dev-values.yaml k8s/values/my-dev-values.yaml
```

Update the following sections in your values file:
- `global.imageRegistry`: Your container registry
- `global.azureAd.*`: Your Azure AD configuration
- `global.database.*`: Your database configuration
- `ingress.hosts`: Your domain configuration

### 2. Build and Push Images

```powershell
# Build and push all images
.\k8s\scripts\build-and-push-images.ps1 -Registry "your-registry.azurecr.io" -Tag "v1.0.0"
```

### 3. Deploy to Kubernetes

```powershell
# Deploy to development
.\k8s\scripts\deploy.ps1 -Environment dev -ImageTag "v1.0.0"

# Deploy to staging
.\k8s\scripts\deploy.ps1 -Environment staging -ImageTag "v1.0.0"

# Deploy to production
.\k8s\scripts\deploy.ps1 -Environment prod -ImageTag "v1.0.0"
```

## Chart Structure

```
k8s/
├── charts/
│   ├── beachbreak/                 # Main umbrella chart
│   ├── beachbreak-commandapi/      # Command API service chart
│   ├── beachbreak-queryapi/        # Query API service chart
│   └── beachbreak-frontend/        # Frontend service chart
├── values/
│   ├── dev-values.yaml            # Development environment
│   ├── staging-values.yaml        # Staging environment
│   └── prod-values.yaml           # Production environment
├── scripts/
│   ├── build-and-push-images.ps1  # Build and push Docker images
│   └── deploy.ps1                 # Deploy to Kubernetes
└── README.md                      # This file
```

## Environment Configurations

### Development
- Single replicas for all services
- Embedded PostgreSQL database
- No autoscaling
- Local domain (beachbreak-dev.local)
- No SSL/TLS
- Monitoring disabled

### Staging
- 2 replicas per service
- External managed PostgreSQL
- Autoscaling enabled (limited)
- SSL with Let's Encrypt staging
- Monitoring enabled

### Production
- 3+ replicas per service
- External managed PostgreSQL with high availability
- Aggressive autoscaling
- SSL with Let's Encrypt production
- Full monitoring and alerting
- Network policies and security constraints
- Resource quotas

## Configuration Reference

### Global Configuration

```yaml
global:
  imageRegistry: ""                 # Container registry URL
  imageRepository: beachbreak       # Base repository name
  imageTag: "latest"               # Image tag
  imagePullPolicy: IfNotPresent    # Pull policy

  azureAd:
    instance: "https://login.microsoftonline.com/"
    domain: "your-domain.onmicrosoft.com"
    tenantId: "your-tenant-id"
    clientId: "your-client-id"
    clientSecret: "your-secret"
    audience: "api://your-client-id"
    scope: "api://your-client-id/.default"

  database:
    host: "postgres-host"
    port: 5432
    name: "beachbreakdb"
    username: "beachbreak"
    password: "secure-password"
```

### Service Configuration

Each service (commandapi, queryapi, frontend) supports:

```yaml
servicename:
  enabled: true
  replicaCount: 2
  image:
    repository: beachbreak/servicename
    tag: "latest"
  resources:
    requests:
      memory: "256Mi"
      cpu: "250m"
    limits:
      memory: "512Mi"
      cpu: "500m"
  autoscaling:
    enabled: true
    minReplicas: 2
    maxReplicas: 10
```

## Security Configuration

### RBAC
- ServiceAccounts created for each service
- Minimal required permissions
- Namespace-scoped roles

### Network Policies
- Ingress: Only from ingress controller and monitoring
- Egress: Database, DNS, and HTTPS only
- Inter-service communication within namespace

### Pod Security
- Non-root user (UID 10001)
- Read-only root filesystem where possible
- Dropped capabilities
- Security context enforcement

## Monitoring

### Health Checks
- **Liveness Probe**: `/alive` - Service is running
- **Readiness Probe**: `/health` - Service is ready for traffic
- **Startup Probe**: `/alive` - Service is starting up

### Metrics
- Prometheus metrics at `/metrics`
- ServiceMonitor for automatic discovery
- Custom alerting rules
- Grafana dashboards

### Logging
- Structured logging to stdout
- Log aggregation via cluster logging solution
- Different log levels per environment

## Database Management

### Event Sourcing Schema
- `events` schema: Immutable event stream
- `readmodels` schema: Denormalized projections

### Initialization
- Automatic schema creation in development
- Manual migration scripts for production
- Marten handles event store setup

### Backup and Recovery
- Regular automated backups
- Point-in-time recovery capability
- Cross-region replication for production

## Deployment Workflows

### CI/CD Integration

```yaml
# Example GitHub Actions workflow
- name: Build and Push Images
  run: |
    .\k8s\scripts\build-and-push-images.ps1 -Registry ${{ secrets.REGISTRY }} -Tag ${{ github.sha }}

- name: Deploy to Staging
  run: |
    .\k8s\scripts\deploy.ps1 -Environment staging -ImageTag ${{ github.sha }}
```

### Rolling Updates
- Zero-downtime deployments
- Configurable rolling update strategy
- Health check validation
- Automatic rollback on failure

### Blue-Green Deployments
- Use separate namespaces
- Traffic switching via ingress
- Full validation before switch

## Troubleshooting

### Common Issues

1. **Image Pull Errors**
   ```bash
   # Check image exists and credentials
   kubectl get events -n beachbreak-dev
   kubectl describe pod <pod-name> -n beachbreak-dev
   ```

2. **Database Connection Issues**
   ```bash
   # Check database connectivity
   kubectl logs -f deployment/beachbreak-commandapi -n beachbreak-dev
   kubectl exec -it deployment/beachbreak-commandapi -n beachbreak-dev -- bash
   ```

3. **Authentication Issues**
   ```bash
   # Check Azure AD configuration
   kubectl get secret beachbreak-dev-secrets -n beachbreak-dev -o yaml
   kubectl logs -f deployment/beachbreak-frontend -n beachbreak-dev
   ```

### Debugging Commands

```bash
# Check all resources
kubectl get all -n beachbreak-dev

# Check configuration
kubectl get configmap beachbreak-dev-config -n beachbreak-dev -o yaml
kubectl get secret beachbreak-dev-secrets -n beachbreak-dev -o yaml

# Check logs
kubectl logs -f deployment/beachbreak-commandapi -n beachbreak-dev
kubectl logs -f deployment/beachbreak-queryapi -n beachbreak-dev
kubectl logs -f deployment/beachbreak-frontend -n beachbreak-dev

# Check ingress
kubectl describe ingress beachbreak-dev -n beachbreak-dev

# Port forward for testing
kubectl port-forward svc/beachbreak-commandapi 8080:80 -n beachbreak-dev
kubectl port-forward svc/beachbreak-queryapi 8081:80 -n beachbreak-dev
kubectl port-forward svc/beachbreak-frontend 8082:80 -n beachbreak-dev
```

## Scaling

### Horizontal Pod Autoscaling
- Automatic scaling based on CPU/memory usage
- Custom metrics scaling (requests per second, etc.)
- Different scaling policies per service

### Vertical Pod Autoscaling
- Automatic resource request adjustments
- Based on actual usage patterns
- Requires VPA operator

### Cluster Autoscaling
- Node scaling based on resource demands
- Multi-zone deployment for availability
- Spot instances for cost optimization

## Security Best Practices

### Image Security
- Use official .NET runtime images
- Regular base image updates
- Vulnerability scanning
- Multi-stage builds for minimal attack surface

### Secrets Management
- External secret operators (Azure Key Vault, etc.)
- Secret rotation policies
- Encrypted secrets at rest
- RBAC for secret access

### Network Security
- Network policies for traffic isolation
- Service mesh for advanced traffic management
- Regular security audits
- Penetration testing

## Performance Optimization

### Resource Tuning
- Right-sizing based on metrics
- JVM/GC tuning for .NET applications
- Database connection pooling
- Caching strategies

### Database Optimization
- Read replicas for query scaling
- Connection pooling
- Query optimization
- Index management

### CDN and Caching
- Static asset caching
- API response caching
- Browser caching headers
- Edge caching for global distribution

## Disaster Recovery

### Backup Strategy
- Database backups (automated)
- Configuration backups
- Image registry backups
- Cross-region replication

### Recovery Procedures
- RTO/RPO requirements
- Recovery testing schedules
- Runbook documentation
- Communication plans

## Support and Maintenance

### Monitoring and Alerting
- 24/7 monitoring setup
- Alert fatigue prevention
- Escalation procedures
- Dashboard maintenance

### Updates and Patching
- Regular security updates
- Kubernetes version upgrades
- Application updates
- Dependency management

---

For additional support, please contact the ti8m Development Team or create an issue in the project repository.