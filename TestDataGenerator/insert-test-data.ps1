# Test data insertion commands for ti8m BeachBreak API (PowerShell)
# Make sure the CommandApi is running on the expected port

$BASE_URL = "https://localhost:7062"
$API_VERSION = "1.0"

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

Write-Host "Inserting organizations..."
try {
    Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/organizations/bulk-import" `
      -Method POST `
      -ContentType "application/json" `
      -InFile "test-organizations.json"
    Write-Host "Organizations inserted successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error inserting organizations: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Waiting 2 seconds..."
Start-Sleep -Seconds 2

Write-Host "Inserting employees..."
try {
    Invoke-RestMethod -Uri "$BASE_URL/c/api/v$API_VERSION/employees/bulk-insert" `
      -Method POST `
      -ContentType "application/json" `
      -InFile "test-employees.json"
    Write-Host "Employees inserted successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error inserting employees: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Test data insertion completed!" -ForegroundColor Cyan
