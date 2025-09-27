# Test Data Generator for ti8m BeachBreak

This directory contains generated test data and scripts for populating the ti8m BeachBreak application with sample organizations and employees.

## Generated Files

- **`test-organizations.json`** - 25 organizations with hierarchical structure (departments and teams)
- **`test-employees.json`** - 200 employees distributed across organizations (8 per organization)
- **`insert-test-data.sh`** - Bash script for inserting test data via cURL
- **`insert-test-data.ps1`** - PowerShell script for inserting test data via REST API calls

## Test Data Structure

### Organizations
- **Root Organizations**: 5 departments (Engineering, Product, Marketing, Sales, HR)
- **Sub-Organizations**: Teams within each department (e.g., "Engineering Team 1", "Product Team 3")
- **Organization Numbers**: Sequential from 1000 onwards
- **Hierarchical Structure**: Sub-organizations reference parent organizations

### Employees
- **Distribution**: 8 employees per organization (200 total)
- **Realistic Names**: German/European names from predefined lists
- **Email Format**: `firstname.lastname@beachbreak.com`
- **Employee IDs**: Sequential format `EMP0001`, `EMP0002`, etc.
- **Roles**: Various software development and business roles
- **Dates**: Realistic start dates, end dates, and last start dates
- **Management Structure**: First employee in each organization becomes manager for others

## Usage Instructions

### Prerequisites
1. Start the CommandApi: `dotnet run` in `03_Infrastructure/ti8m.BeachBreak.CommandApi`
2. Ensure the API is running on `https://localhost:7001` (adjust scripts if different)

### Option 1: Using PowerShell (Windows)
```powershell
.\insert-test-data.ps1
```

### Option 2: Using Bash (Linux/macOS/WSL)
```bash
chmod +x insert-test-data.sh
./insert-test-data.sh
```

## API Endpoints Used

The scripts use these bulk-insert endpoints:

1. **Organizations**: `POST /c/api/v1.0/organizations/bulk-import`
   - Accepts array of `SyncOrganizationDto` objects
   - Fields: `number`, `parentNumber`, `name`, `managerUserId`

2. **Employees**: `POST /c/api/v1.0/employees/bulk-insert`
   - Accepts array of `EmployeeDto` objects
   - Fields: `id`, `employeeId`, `loginName`, `firstName`, `lastName`, `eMail`, `role`, `organizationNumber`, `startDate`, `endDate`, `lastStartDate`, `managerId`, `manager`

## Verification

After running the scripts, you can verify the data was inserted by:

1. **Query Organizations**: `GET /q/api/v1.0/organizations`
2. **Query Employees**: `GET /q/api/v1.0/employees`

Or check the database directly via the .NET Aspire dashboard or PgAdmin.

## Data Characteristics

- **Deterministic**: Uses fixed seed (42) for Random, so data generation is reproducible
- **Realistic**: Names, roles, and organizational structures reflect real-world scenarios
- **Hierarchical**: Organizations have proper parent-child relationships
- **Manager Assignments**: Each organization has internal management structure
- **Date Variations**: Employment dates span realistic ranges (30 days to 3 years ago)
- **Status Variety**: ~10% of employees have end dates (former employees)

## Regenerating Data

To generate new test data with different parameters:

1. Modify the `TestDataGenerator/Program.cs` file
2. Adjust organization count: `GenerateOrganizations(25)`
3. Adjust employees per org: `GenerateEmployees(organizations, 8)`
4. Run: `dotnet run` in the `TestDataGenerator` directory

## Notes

- The PowerShell script includes SSL certificate bypass for development environments
- Both scripts include error handling and success/failure feedback
- Organization insertion is done before employees to satisfy foreign key constraints
- Manager relationships are established within each organization's employee group