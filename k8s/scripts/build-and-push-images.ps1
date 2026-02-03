# =========================================
# BeachBreak - Build and Push Docker Images
# =========================================

param(
    [Parameter(Mandatory = $true)]
    [string]$Registry,

    [Parameter(Mandatory = $false)]
    [string]$Tag = "latest",

    [Parameter(Mandatory = $false)]
    [string]$Namespace = "beachbreak"
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "Building and pushing BeachBreak Docker images..." -ForegroundColor Green
Write-Host "Registry: $Registry" -ForegroundColor Yellow
Write-Host "Tag: $Tag" -ForegroundColor Yellow
Write-Host "Namespace: $Namespace" -ForegroundColor Yellow

# Navigate to solution root
$solutionRoot = Split-Path -Parent -Path (Split-Path -Parent -Path $PSScriptRoot)
Set-Location $solutionRoot

# Build and push Command API
Write-Host "Building Command API..." -ForegroundColor Cyan
$commandApiImage = "$Registry/$Namespace/commandapi:$Tag"
docker build -f "03_Infrastructure/ti8m.BeachBreak.CommandApi/Dockerfile" -t $commandApiImage .
if ($LASTEXITCODE -ne 0) { throw "Failed to build Command API image" }

Write-Host "Pushing Command API..." -ForegroundColor Cyan
docker push $commandApiImage
if ($LASTEXITCODE -ne 0) { throw "Failed to push Command API image" }

# Build and push Query API
Write-Host "Building Query API..." -ForegroundColor Cyan
$queryApiImage = "$Registry/$Namespace/queryapi:$Tag"
docker build -f "03_Infrastructure/ti8m.BeachBreak.QueryApi/Dockerfile" -t $queryApiImage .
if ($LASTEXITCODE -ne 0) { throw "Failed to build Query API image" }

Write-Host "Pushing Query API..." -ForegroundColor Cyan
docker push $queryApiImage
if ($LASTEXITCODE -ne 0) { throw "Failed to push Query API image" }

# Build and push Frontend
Write-Host "Building Frontend..." -ForegroundColor Cyan
$frontendImage = "$Registry/$Namespace/frontend:$Tag"
docker build -f "05_Frontend/ti8m.BeachBreak/Dockerfile" -t $frontendImage .
if ($LASTEXITCODE -ne 0) { throw "Failed to build Frontend image" }

Write-Host "Pushing Frontend..." -ForegroundColor Cyan
docker push $frontendImage
if ($LASTEXITCODE -ne 0) { throw "Failed to push Frontend image" }

Write-Host "All images built and pushed successfully!" -ForegroundColor Green
Write-Host "Images:" -ForegroundColor Yellow
Write-Host "  - $commandApiImage" -ForegroundColor White
Write-Host "  - $queryApiImage" -ForegroundColor White
Write-Host "  - $frontendImage" -ForegroundColor White