# =========================================
# BeachBreak - Helm Deployment Script
# =========================================

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment,

    [Parameter(Mandatory = $false)]
    [string]$Namespace = "beachbreak-$Environment",

    [Parameter(Mandatory = $false)]
    [string]$ReleaseName = "beachbreak-$Environment",

    [Parameter(Mandatory = $false)]
    [string]$ImageTag = "latest",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$CreateNamespace,

    [Parameter(Mandatory = $false)]
    [switch]$Upgrade
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "Deploying BeachBreak to Kubernetes..." -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Namespace: $Namespace" -ForegroundColor Yellow
Write-Host "Release Name: $ReleaseName" -ForegroundColor Yellow
Write-Host "Image Tag: $ImageTag" -ForegroundColor Yellow

# Navigate to k8s directory
$k8sRoot = Split-Path -Parent -Path $PSScriptRoot
Set-Location $k8sRoot

# Check if Helm is installed
try {
    helm version --short | Out-Null
} catch {
    throw "Helm is not installed or not in PATH. Please install Helm first."
}

# Check if kubectl is installed and configured
try {
    kubectl version --client=true | Out-Null
} catch {
    throw "kubectl is not installed or not configured. Please install and configure kubectl first."
}

# Create namespace if requested
if ($CreateNamespace) {
    Write-Host "Creating namespace: $Namespace" -ForegroundColor Cyan
    kubectl create namespace $Namespace --dry-run=client -o yaml | kubectl apply -f -
}

# Build Helm command
$helmArgs = @(
    if ($Upgrade) { "upgrade" } else { "install" }
    $ReleaseName
    "charts/beachbreak"
    "--namespace", $Namespace
    "--values", "values/$Environment-values.yaml"
    "--set", "global.imageTag=$ImageTag"
    "--timeout", "600s"
    "--wait"
)

if ($DryRun) {
    $helmArgs += "--dry-run"
}

if (-not $Upgrade) {
    $helmArgs += "--create-namespace"
}

Write-Host "Running Helm command:" -ForegroundColor Cyan
Write-Host "helm $($helmArgs -join ' ')" -ForegroundColor White

# Execute Helm command
& helm @helmArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deployment completed successfully!" -ForegroundColor Green

    if (-not $DryRun) {
        Write-Host "Checking deployment status..." -ForegroundColor Cyan
        kubectl get pods -n $Namespace

        Write-Host "Services:" -ForegroundColor Cyan
        kubectl get services -n $Namespace

        Write-Host "Ingress:" -ForegroundColor Cyan
        kubectl get ingress -n $Namespace
    }
} else {
    Write-Host "Deployment failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}