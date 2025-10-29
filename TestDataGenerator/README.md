# Test Data Generator

This script inserts test data into the BeachBreak API for development and testing purposes.

## Prerequisites

### 1. Azure AD App Role Configuration

The script uses service principal (client credentials) authentication with an app role. You need to configure the "DataSeeder" app role in your Azure AD app registration:

#### Step 1: Add App Role to App Registration Manifest

1. Go to **Azure Portal** → **Azure Active Directory** → **App registrations**
2. Find your app: `e8f34612-d603-4e6a-bfec-da90b5198ea8`
3. Click **Manifest**
4. Find the `"appRoles"` section and add this role:

```json
{
  "appRoles": [
    {
      "allowedMemberTypes": [
        "Application"
      ],
      "description": "Allows the application to seed test data via bulk-insert APIs",
      "displayName": "Data Seeder",
      "id": "12345678-1234-1234-1234-123456789012",
      "isEnabled": true,
      "lang": null,
      "origin": "Application",
      "value": "DataSeeder"
    }
  ]
}
```

**Important:** Replace the `"id"` value with a new GUID. You can generate one at https://guidgenerator.com/ or use PowerShell: `[guid]::NewGuid()`

5. Click **Save**

#### Step 2: Assign App Role to the App Registration

After adding the app role to the manifest, you need to assign it:

1. Go to **Azure Portal** → **Azure Active Directory** → **Enterprise applications**
2. Search for your app: `e8f34612-d603-4e6a-bfec-da90b5198ea8`
3. Click on the app
4. Go to **Permissions** (or **API permissions** in App registrations view)
5. Click **Add a permission** → **My APIs** → Select your API app
6. Select **Application permissions** → Check **DataSeeder**
7. Click **Add permissions**
8. Click **Grant admin consent** for your tenant

Alternatively, you can assign the role programmatically using Azure CLI or PowerShell.

### 2. Client Secret

The client secret must be provided as a parameter when running the script:
- **NEVER hardcode secrets in source code or documentation**
- Generate client secret in: Azure Portal → App registrations → Certificates & secrets
- Pass the secret via the `-ClientSecret` parameter at runtime
- Store secrets securely in Azure Key Vault, environment variables, or CI/CD secret storage

### 3. API Must Be Running

Ensure the CommandApi is running before executing the script:
- **Local development**: Start the Aspire app host
- **Azure**: Ensure the API is deployed and accessible at the configured URL

## Usage

### Running the Script

#### Basic Usage (Local Development)
```powershell
cd C:\projects\BlazorRadzenTest\TestDataGenerator
.\insert-test-data.ps1 -ClientSecret "your-secret-here"
```

#### Using Environment Variable
```powershell
$env:AZURE_CLIENT_SECRET = "your-secret-here"
.\insert-test-data.ps1 -ClientSecret $env:AZURE_CLIENT_SECRET
```

#### Azure/Production Usage
```powershell
.\insert-test-data.ps1 `
    -ClientSecret "your-secret-here" `
    -BaseUrl "https://your-api.azurewebsites.net" `
    -TenantId "your-tenant-id" `
    -ClientId "your-client-id"
```

#### Skip Authentication (Local Development Only)
```powershell
# Only use when [Authorize] attributes are commented out
.\insert-test-data.ps1 -SkipAuth
```

### Script Parameters

| Parameter | Description | Default | Required |
|-----------|-------------|---------|----------|
| `-ClientSecret` | Azure AD client secret for service principal authentication | `$null` (uses interactive auth if available) | No |
| `-BaseUrl` | Base URL of the CommandApi | `https://localhost:7062` | No |
| `-TenantId` | Azure AD tenant ID | `bf5cf359-8bee-4827-bb13-40606017dabc` | No |
| `-ClientId` | Azure AD client (application) ID | `e8f34612-d603-4e6a-bfec-da90b5198ea8` | No |
| `-SkipAuth` | Skip authentication entirely (for local dev only) | `$false` | No |

### What Gets Inserted

The script inserts test data in this order:
1. **Category**: Beach Break category (from `test-categories.json`)
2. **Questionnaire Template**: Beach Break 2026 template with 3 sections (from `test-questionnaire-template.json`)
   - Creates the template in Draft status
   - Publishes the template immediately
3. **Organizations**: Organizational units (from `test-organizations.json`)
4. **Employees**: Employee records (from `test-employees.json`)

### CI/CD Pipeline Usage

For automated deployments, use pipeline variables:

**Azure DevOps:**
```yaml
- task: PowerShell@2
  displayName: 'Insert Test Data'
  inputs:
    filePath: '$(System.DefaultWorkingDirectory)/TestDataGenerator/insert-test-data.ps1'
    arguments: >
      -ClientSecret "$(AZURE_CLIENT_SECRET)"
      -BaseUrl "$(API_BASE_URL)"
      -TenantId "$(AZURE_TENANT_ID)"
      -ClientId "$(AZURE_CLIENT_ID)"
```

**GitHub Actions:**
```yaml
- name: Insert Test Data
  shell: pwsh
  run: |
    ./TestDataGenerator/insert-test-data.ps1 `
      -ClientSecret "${{ secrets.AZURE_CLIENT_SECRET }}" `
      -BaseUrl "${{ vars.API_BASE_URL }}" `
      -TenantId "${{ vars.AZURE_TENANT_ID }}" `
      -ClientId "${{ vars.AZURE_CLIENT_ID }}"
```

## Authentication Models

The API endpoints support two authentication modes:

### 1. User Authentication (Interactive)
- Used by: Web UI, manual API calls
- Authorization policies: `Admin`, `HR`, `HRLead`, etc.
- Checks `ApplicationRole` claim from user database

### 2. Service Principal Authentication (App-Only)
- Used by: Automation scripts, CI/CD pipelines
- Authorization policies: `AdminOrApp`, `HROrApp`
- Checks `DataSeeder` app role from Azure AD

The bulk-insert endpoints accept **both** authentication modes:
- `POST /categories` → `HROrApp` policy
- `POST /questionnaire-templates` → `HROrApp` policy
- `POST /employees/bulk-insert` → `AdminOrApp` policy
- `POST /organizations/bulk-import` → `AdminOrApp` policy

## Troubleshooting

### Error: "AADSTS7000218: client_assertion or client_secret required"
- The client secret is missing or expired
- Update `$CLIENT_SECRET` in the script with a valid secret from Azure Portal

### Error: "403 Forbidden" or "Authorization failed"
- The app role "DataSeeder" is not configured or assigned
- Follow the Azure AD App Role Configuration steps above
- Ensure you clicked "Grant admin consent" after adding permissions

### Error: "The remote server returned an error: (400) Bad Request"
- Check the scope is set to `.default` for client credentials flow: `api://{CLIENT_ID}/.default`
- Verify the Tenant ID and Client ID are correct

### Error: Connection refused or timeout
- Ensure the CommandApi is running
- Check the `$BASE_URL` matches your API's actual URL
- For local development, verify Aspire is running

## Security Notes

⚠️ **Important Security Considerations:**

1. **Never Hardcode Secrets**:
   - Use the `-ClientSecret` parameter to pass secrets at runtime
   - Store secrets in Azure Key Vault, GitHub Secrets, or Azure DevOps variable groups
   - Use environment variables or secure storage for local development
   - **Never commit secrets to source control**

2. **Local Development**:
   - Use the `-SkipAuth` parameter instead of hardcoding `$SKIP_AUTH = $true`
   - Only use `-SkipAuth` when `[Authorize]` attributes are commented out
   - **Never deploy with `-SkipAuth` to Azure**

3. **Production**: In Azure deployments, always keep authentication enabled and use:
   - Azure Key Vault for secrets (recommended)
   - Pipeline secret variables for CI/CD
   - Managed Identity where possible
   - Rotate secrets regularly before expiration

4. **App Role Assignment**: Only grant the DataSeeder role to trusted service principals used for automation

5. **Secret Rotation**: When rotating secrets:
   ```powershell
   # Generate new secret in Azure Portal
   # Update in secure storage (Key Vault, pipeline variables, etc.)
   # Test with new secret
   .\insert-test-data.ps1 -ClientSecret "new-secret"
   # Delete old secret from Azure Portal
   ```

## Files

- `insert-test-data.ps1` - Main PowerShell script
- `test-categories.json` - Category test data
- `test-questionnaire-template.json` - Questionnaire template test data (Beach Break 2026)
- `test-organizations.json` - Organization test data
- `test-employees.json` - Employee test data
- `README.md` - This file

## Questionnaire Template Details

The test questionnaire template "Beach Break 2026" includes:

**Section 1: Rückblick 2025 - Selbsteinschätzung für Mitarbeitende**
- Employee self-assessment section
- Competency assessment question with 9 competencies
- Completed by: Both (Employee perspective)

**Section 2: Rückblick - Mein/e Vorgesetzte/r**
- Manager feedback section
- Text questions about manager collaboration
- Completed by: Employee

**Section 3: Ausblick 2026 - Zielvereinbarung**
- Goal planning section
- Goal management question
- Completed by: Employee

The template is created in **Draft** status (Status = 0) and is automatically published by the script, making it immediately available for creating questionnaire assignments.
