# =========================================
# BeachBreak - Cleanup Deployment Script
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
    [switch]$DeleteNamespace,

    [Parameter(Mandatory = $false)]
    [switch]$KeepPVC,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$Force
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "Cleaning up BeachBreak deployment..." -ForegroundColor Yellow
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Namespace: $Namespace" -ForegroundColor Yellow
Write-Host "Release Name: $ReleaseName" -ForegroundColor Yellow

if (-not $Force) {
    $confirmation = Read-Host "Are you sure you want to delete the deployment? This action cannot be undone. Type 'yes' to continue"
    if ($confirmation -ne "yes") {
        Write-Host "Cleanup cancelled by user." -ForegroundColor Green
        exit 0
    }
}

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

# Check if release exists
$releaseExists = $false
try {
    helm status $ReleaseName -n $Namespace 2>$null | Out-Null
    $releaseExists = $true
} catch {
    Write-Host "Helm release '$ReleaseName' not found in namespace '$Namespace'" -ForegroundColor Yellow
}

if ($releaseExists) {
    Write-Host "Uninstalling Helm release: $ReleaseName" -ForegroundColor Cyan

    if ($DryRun) {
        Write-Host "DRY RUN: Would run: helm uninstall $ReleaseName --namespace $Namespace" -ForegroundColor White
    } else {
        helm uninstall $ReleaseName --namespace $Namespace
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Helm release uninstalled successfully." -ForegroundColor Green
        } else {
            Write-Host "Failed to uninstall Helm release." -ForegroundColor Red
        }
    }
}

# Delete persistent volume claims if not keeping them
if (-not $KeepPVC) {
    Write-Host "Checking for persistent volume claims..." -ForegroundColor Cyan

    $pvcs = kubectl get pvc -n $Namespace -o name 2>$null
    if ($pvcs) {
        Write-Host "Found PVCs to delete:" -ForegroundColor Yellow
        foreach ($pvc in $pvcs) {
            Write-Host "  - $pvc" -ForegroundColor White
        }

        if ($DryRun) {
            Write-Host "DRY RUN: Would delete PVCs" -ForegroundColor White
        } else {
            kubectl delete pvc --all -n $Namespace
            Write-Host "PVCs deleted." -ForegroundColor Green
        }
    } else {
        Write-Host "No PVCs found." -ForegroundColor Green
    }
}

# Delete namespace if requested
if ($DeleteNamespace) {
    Write-Host "Checking if namespace exists..." -ForegroundColor Cyan

    $namespaceExists = $false
    try {
        kubectl get namespace $Namespace 2>$null | Out-Null
        $namespaceExists = $true
    } catch {
        Write-Host "Namespace '$Namespace' does not exist." -ForegroundColor Green
    }

    if ($namespaceExists) {
        Write-Host "Deleting namespace: $Namespace" -ForegroundColor Cyan

        if ($DryRun) {
            Write-Host "DRY RUN: Would run: kubectl delete namespace $Namespace" -ForegroundColor White
        } else {
            kubectl delete namespace $Namespace
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Namespace deleted successfully." -ForegroundColor Green
            } else {
                Write-Host "Failed to delete namespace." -ForegroundColor Red
            }
        }
    }
} else {
    # Check remaining resources
    Write-Host "Checking for remaining resources in namespace..." -ForegroundColor Cyan
    $remainingResources = kubectl get all -n $Namespace 2>$null
    if ($remainingResources) {
        Write-Host "Remaining resources in namespace $Namespace:" -ForegroundColor Yellow
        Write-Host $remainingResources -ForegroundColor White
        Write-Host "Use -DeleteNamespace flag to remove the entire namespace." -ForegroundColor Yellow
    } else {
        Write-Host "No resources remaining in namespace." -ForegroundColor Green
    }
}

Write-Host "Cleanup completed!" -ForegroundColor Green

# Summary
Write-Host "`nCleanup Summary:" -ForegroundColor Cyan
Write-Host "  Environment: $Environment" -ForegroundColor White
Write-Host "  Release: $(if ($releaseExists) { 'Deleted' } else { 'Not found' })" -ForegroundColor White
Write-Host "  PVCs: $(if ($KeepPVC) { 'Preserved' } else { 'Deleted' })" -ForegroundColor White
Write-Host "  Namespace: $(if ($DeleteNamespace) { 'Deleted' } else { 'Preserved' })" -ForegroundColor White