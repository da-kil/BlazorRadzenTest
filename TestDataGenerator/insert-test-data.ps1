# Test data insertion commands for ti8m BeachBreak API (PowerShell)
# Make sure the CommandApi is running on the expected port

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ClientSecret = $null,

    [Parameter(Mandatory=$false)]
    [string]$BaseUrl = "https://localhost:7062",

    [Parameter(Mandatory=$false)]
    [string]$TenantId = "bf5cf359-8bee-4827-bb13-40606017dabc",

    [Parameter(Mandatory=$false)]
    [string]$ClientId = "e8f34612-d603-4e6a-bfec-da90b5198ea8",

    [Parameter(Mandatory=$false)]
    [switch]$SkipAuth = $false
)

$BASE_URL = $BaseUrl
$API_VERSION = "1.0"

# Azure AD Configuration
$TENANT_ID = $TenantId
$CLIENT_ID = $ClientId
$SCOPE = "api://$CLIENT_ID/.default"

# Client secret for service principal authentication
# Get this from Azure Portal → App Registrations → Certificates & secrets
# The service principal must have "DataSeeder" app role assigned
$CLIENT_SECRET = $ClientSecret

# Set to $true to skip authentication (for local development when [Authorize] is commented out)
$SKIP_AUTH = $SkipAuth

# Skip certificate validation for development
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

# Function to get Azure AD token
function Get-AzureAdToken {
    param (
        [string]$TenantId,
        [string]$ClientId,
        [string]$Scope,
        [string]$ClientSecret
    )

    Write-Host "Authenticating with Azure AD..." -ForegroundColor Cyan

    if ($ClientSecret) {
        # Use client credentials flow (for confidential clients with secret)
        Write-Host "Using client secret authentication..." -ForegroundColor Yellow

        $tokenEndpoint = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"
        $body = @{
            client_id     = $ClientId
            client_secret = $ClientSecret
            scope         = $Scope
            grant_type    = "client_credentials"
        }

        try {
            $response = Invoke-RestMethod -Method Post -Uri $tokenEndpoint -Body $body -ContentType "application/x-www-form-urlencoded"
            return $response.access_token
        } catch {
            Write-Host "Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "Please verify your client secret is correct." -ForegroundColor Yellow
            exit 1
        }
    } else {
        # Use interactive browser authentication (for public clients)
        Write-Host "Using interactive browser authentication..." -ForegroundColor Yellow
        Write-Host "Note: If this fails, you may need to either:" -ForegroundColor Yellow
        Write-Host "  1. Enable 'Allow public client flows' in Azure AD app registration, OR" -ForegroundColor Yellow
        Write-Host "  2. Set CLIENT_SECRET variable in this script" -ForegroundColor Yellow

        # Check if MSAL.PS module is installed
        if (-not (Get-Module -ListAvailable -Name MSAL.PS)) {
            Write-Host "Installing MSAL.PS module..." -ForegroundColor Yellow
            Install-Module -Name MSAL.PS -Force -Scope CurrentUser
        }

        Import-Module MSAL.PS

        try {
            $token = Get-MsalToken -ClientId $ClientId -TenantId $TenantId -Scopes $Scope -Interactive
            return $token.AccessToken
        } catch {
            Write-Host "Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "Please ensure you have access to the BeachBreak API and have Admin or HR role." -ForegroundColor Yellow
            exit 1
        }
    }
}

# Get authentication token (skip if SKIP_AUTH is true)
if (-not $SKIP_AUTH) {
    $accessToken = Get-AzureAdToken -TenantId $TENANT_ID -ClientId $CLIENT_ID -Scope $SCOPE -ClientSecret $CLIENT_SECRET
    $headers = @{
        "Authorization" = "Bearer $accessToken"
        "Content-Type" = "application/json"
    }
} else {
    Write-Host "Skipping authentication (endpoints have [Authorize] disabled for local development)" -ForegroundColor Yellow
    $headers = @{
        "Content-Type" = "application/json"
    }
}

Write-Host "`nBulk importing translations..." -ForegroundColor Cyan
try {
    $translationResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/translations/bulk-import" `
      -Method POST `
      -Headers $headers `
      -InFile "test-translations.json"

    Write-Host "Translations bulk imported successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error bulk importing translations: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`nWaiting 2 seconds..." -ForegroundColor Cyan
Start-Sleep -Seconds 2

Write-Host "`nInserting category..." -ForegroundColor Cyan
try {
    $categoryResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/categories" `
      -Method POST `
      -Headers $headers `
      -InFile "test-categories.json"

    $categoryResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/categories" `
      -Method POST `
      -Headers $headers `
      -InFile "test-categories-2.json"
    Write-Host "Category inserted successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error inserting category: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`nWaiting 2 seconds..." -ForegroundColor Cyan
Start-Sleep -Seconds 2

Write-Host "`nInserting questionnaire template..." -ForegroundColor Cyan
try {
    $templateResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/questionnaire-templates" `
      -Method POST `
      -Headers $headers `
      -InFile "test-questionnaire-template.json"

    $templateResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/questionnaire-templates" `
      -Method POST `
      -Headers $headers `
      -InFile "test-questionnaire-template-2.json"
    Write-Host "Questionnaire template inserted successfully!" -ForegroundColor Green

    # Publish the template
    Write-Host "Publishing questionnaire template..." -ForegroundColor Cyan
    $templateId = "960e40e9-30bd-4e4a-a16b-b0107cbe3dba"
    $publishResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/questionnaire-templates/$templateId/publish" `
      -Method POST `
      -Headers $headers

    $templateId = "68f7fb75-0e17-4593-9e68-83091d4e7e71"
    $publishResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/questionnaire-templates/$templateId/publish" `
      -Method POST `
      -Headers $headers
    Write-Host "Questionnaire template published successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error with questionnaire template: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`nWaiting 2 seconds..." -ForegroundColor Cyan
Start-Sleep -Seconds 2

Write-Host "`nInserting organizations..." -ForegroundColor Cyan
try {
    $orgResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/organizations/bulk-import" `
      -Method POST `
      -Headers $headers `
      -InFile "test-organizations.json"
    Write-Host "Organizations inserted successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error inserting organizations: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`nWaiting 2 seconds..." -ForegroundColor Cyan
Start-Sleep -Seconds 2

Write-Host "`nInserting employees..." -ForegroundColor Cyan
try {
    $empResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/employees/bulk-insert" `
      -Method POST `
      -Headers $headers `
      -InFile "test-employees.json"
    Write-Host "Employees inserted successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error inserting employees: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`nWaiting 2 seconds..." -ForegroundColor Cyan
Start-Sleep -Seconds 2

# Update employee application roles
Write-Host "`nUpdating employee application roles..." -ForegroundColor Cyan

# Define role updates
$roleUpdates = @(
    @{
        Id = "9d159666-0126-4d36-beff-057b68512efa"
        NewRole = 1
        RoleName = "TeamLead"
    },
    @{
        Id = "e91731e2-fb48-4a69-b740-075bf5d39eaf"
        NewRole = 4
        RoleName = "Admin"
    }
)

foreach ($update in $roleUpdates) {
    try {
        $body = @{
            NewRole = $update.NewRole
        } | ConvertTo-Json

        Write-Host "  Updating employee $($update.Id) to $($update.RoleName)..." -ForegroundColor Yellow

        $roleResponse = Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/employees/$($update.Id)/application-role" `
          -Method PUT `
          -Headers $headers `
          -Body $body

        Write-Host "  Successfully updated employee to $($update.RoleName)" -ForegroundColor Green
    } catch {
        Write-Host "  Error updating employee role: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "  Response: $responseBody" -ForegroundColor Red
        }
    }
}

Write-Host "`nTest data insertion completed!" -ForegroundColor Cyan