using System.Text.Json;
using System.Text;

namespace TestDataGenerator;

public class SyncOrganizationDto
{
    public required string Number { get; set; }
    public string? ParentNumber { get; set; }
    public required string Name { get; set; }
    public string? ManagerUserId { get; set; }
}

public class EmployeeDto
{
    public Guid Id { get; set; }
    public required string EmployeeId { get; set; }
    public required string LoginName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string EMail { get; set; }
    public required string Role { get; set; }
    public required string OrganizationNumber { get; set; }
    public required string StartDate { get; set; }
    public required string EndDate { get; set; }
    public required string LastStartDate { get; set; }
    public required string ManagerId { get; set; }
    public required string Manager { get; set; }
}

public class UITranslationDto
{
    public required string Key { get; set; }
    public required string German { get; set; }
    public required string English { get; set; }
    public required string Category { get; set; }
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
}

public class TestDataGenerator
{
    private readonly Random random = new Random(42); // Fixed seed for reproducible results

    // Sample data for generating realistic names
    private readonly string[] firstNames = {
        "Anna", "Ben", "Clara", "David", "Emma", "Felix", "Greta", "Hans", "Ida", "Jonas",
        "Klara", "Lukas", "Maria", "Noah", "Olivia", "Paul", "Quinn", "Rosa", "Stefan", "Tina",
        "Ulrich", "Victoria", "Wolfgang", "Xenia", "Yves", "Zoe", "Alexander", "Bianca", "Christian", "Diana"
    };

    private readonly string[] lastNames = {
        "Schmidt", "Mueller", "Schneider", "Fischer", "Weber", "Meyer", "Wagner", "Becker", "Schulz", "Hoffmann",
        "Schaefer", "Koch", "Bauer", "Richter", "Klein", "Wolf", "Schroeder", "Neumann", "Schwarz", "Zimmermann",
        "Braun", "Krueger", "Hofmann", "Hartmann", "Lange", "Schmitt", "Werner", "Schmitz", "Krause", "Meier"
    };

    private readonly string[] roles = {
        "Software Engineer", "Senior Software Engineer", "Lead Developer", "Product Manager",
        "Project Manager", "DevOps Engineer", "Data Analyst", "UX Designer", "QA Engineer",
        "Scrum Master", "Technical Lead", "Business Analyst", "System Administrator",
        "Security Specialist", "Database Administrator", "Frontend Developer", "Backend Developer"
    };

    private readonly string[] departmentTypes = {
        "Engineering", "Product", "Marketing", "Sales", "HR", "Finance", "Operations",
        "Customer Success", "Legal", "IT", "Research", "Quality Assurance"
    };

    public List<SyncOrganizationDto> GenerateOrganizations(int count = 50)
    {
        var organizations = new List<SyncOrganizationDto>();
        var orgCounter = 1000;

        // Create root organizations (departments)
        var rootOrgs = new List<SyncOrganizationDto>();
        for (int i = 0; i < Math.Min(count / 5, departmentTypes.Length); i++)
        {
            var orgNumber = orgCounter.ToString();
            var rootOrg = new SyncOrganizationDto
            {
                Number = orgNumber,
                ParentNumber = null,
                Name = departmentTypes[i],
                ManagerUserId = null // Will be assigned later when we create employees
            };
            rootOrgs.Add(rootOrg);
            organizations.Add(rootOrg);
            orgCounter++;
        }

        // Create sub-organizations (teams within departments)
        var remainingCount = count - rootOrgs.Count;
        var teamsPerDept = Math.Max(1, remainingCount / rootOrgs.Count);

        foreach (var rootOrg in rootOrgs)
        {
            for (int i = 0; i < teamsPerDept && organizations.Count < count; i++)
            {
                var teamNumber = random.Next(1, 10);
                var orgNumber = orgCounter.ToString();
                var subOrg = new SyncOrganizationDto
                {
                    Number = orgNumber,
                    ParentNumber = rootOrg.Number,
                    Name = $"{rootOrg.Name} Team {teamNumber}",
                    ManagerUserId = null // Will be assigned later
                };
                organizations.Add(subOrg);
                orgCounter++;
            }
        }

        return organizations;
    }

    // NOTE: UI translations are now managed in test-translations.json
    // This file is the single source of truth for all UI translations.
    // Do not regenerate translations programmatically as it will overwrite manual additions.

    public List<EmployeeDto> GenerateEmployees(List<SyncOrganizationDto> organizations, int employeesPerOrg = 5)
    {
        var employees = new List<EmployeeDto>();
        var employeeCounter = 1;

        foreach (var org in organizations)
        {
            var orgEmployees = new List<EmployeeDto>();

            for (int i = 0; i < employeesPerOrg; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var role = roles[random.Next(roles.Length)];
                var employeeId = $"EMP{employeeCounter:D4}";
                var loginName = $"{firstName.ToLower()}.{lastName.ToLower()}";
                var email = $"{loginName}@beachbreak.com";

                // Generate realistic dates
                var startDate = DateTime.Now.AddDays(-random.Next(30, 1095)); // Started 30 days to 3 years ago
                var lastStartDate = startDate.AddDays(random.Next(-30, 30)); // Within 30 days of start
                var endDate = random.NextDouble() < 0.1 ?
                    startDate.AddDays(random.Next(90, 700)) : // 10% chance of having left
                    DateTime.MaxValue; // Still employed

                var employee = new EmployeeDto
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    LoginName = loginName,
                    FirstName = firstName,
                    LastName = lastName,
                    EMail = email,
                    Role = role,
                    OrganizationNumber = org.Number,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate == DateTime.MaxValue ? "9999-12-31" : endDate.ToString("yyyy-MM-dd"),
                    LastStartDate = lastStartDate.ToString("yyyy-MM-dd"),
                    ManagerId = "", // Will be set later
                    Manager = "" // Will be set later
                };

                orgEmployees.Add(employee);
                employees.Add(employee);
                employeeCounter++;
            }

            // Assign managers within each organization
            if (orgEmployees.Count > 1)
            {
                var manager = orgEmployees[0]; // First employee becomes manager
                for (int i = 1; i < orgEmployees.Count; i++)
                {
                    orgEmployees[i].ManagerId = manager.Id.ToString();
                    orgEmployees[i].Manager = $"{manager.FirstName} {manager.LastName}";
                }
            }
        }

        return employees;
    }

    public async Task SaveTestDataToFiles()
    {
        Console.WriteLine("Generating test data...");

        // Generate organizations
        var organizations = GenerateOrganizations(25);
        Console.WriteLine($"Generated {organizations.Count} organizations");

        // Generate employees
        var employees = GenerateEmployees(organizations, 8);
        Console.WriteLine($"Generated {employees.Count} employees");

        // Load existing translations from file (do not regenerate)
        string translationsJson;
        if (File.Exists("test-translations.json"))
        {
            translationsJson = await File.ReadAllTextAsync("test-translations.json");
            var deserializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var translations = JsonSerializer.Deserialize<List<UITranslationDto>>(translationsJson, deserializeOptions);
            Console.WriteLine($"Loaded {translations?.Count ?? 0} existing UI translations from test-translations.json");
        }
        else
        {
            Console.WriteLine("Warning: test-translations.json not found. Translation import will be skipped.");
            translationsJson = "[]";
        }

        // Configure JSON options for readable output
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Save organizations to file
        var organizationsJson = JsonSerializer.Serialize(organizations, jsonOptions);
        await File.WriteAllTextAsync("test-organizations.json", organizationsJson);
        Console.WriteLine("Saved organizations to test-organizations.json");

        // Save employees to file
        var employeesJson = JsonSerializer.Serialize(employees, jsonOptions);
        await File.WriteAllTextAsync("test-employees.json", employeesJson);
        Console.WriteLine("Saved employees to test-employees.json");

        // NOTE: test-translations.json is NOT regenerated - it is managed manually
        Console.WriteLine("Translations file (test-translations.json) is preserved - no changes made");

        // Generate curl commands for testing
        await GenerateCurlCommands(organizationsJson, employeesJson, translationsJson);
    }

    private async Task GenerateCurlCommands(string organizationsJson, string employeesJson, string translationsJson)
    {
        var curlCommands = new StringBuilder();

        curlCommands.AppendLine("#!/bin/bash");
        curlCommands.AppendLine("# Test data insertion commands for ti8m BeachBreak API");
        curlCommands.AppendLine("# Make sure the CommandApi is running on the expected port");
        curlCommands.AppendLine("");

        curlCommands.AppendLine("# Set the base URL (adjust port if necessary)");
        curlCommands.AppendLine("BASE_URL=\"https://localhost:7001\"");
        curlCommands.AppendLine("API_VERSION=\"1.0\"");
        curlCommands.AppendLine("");

        curlCommands.AppendLine("echo \"Inserting organizations...\"");
        curlCommands.AppendLine("curl -X POST \"$BASE_URL/c/api/v$API_VERSION/organizations/bulk-import\" \\");
        curlCommands.AppendLine("  -H \"Content-Type: application/json\" \\");
        curlCommands.AppendLine("  -H \"Accept: application/json\" \\");
        curlCommands.AppendLine("  -d @test-organizations.json");
        curlCommands.AppendLine("");

        curlCommands.AppendLine("echo \"Waiting 2 seconds...\"");
        curlCommands.AppendLine("sleep 2");
        curlCommands.AppendLine("");

        curlCommands.AppendLine("echo \"Inserting employees...\"");
        curlCommands.AppendLine("curl -X POST \"$BASE_URL/c/api/v$API_VERSION/employees/bulk-insert\" \\");
        curlCommands.AppendLine("  -H \"Content-Type: application/json\" \\");
        curlCommands.AppendLine("  -H \"Accept: application/json\" \\");
        curlCommands.AppendLine("  -d @test-employees.json");
        curlCommands.AppendLine("");

        curlCommands.AppendLine("echo \"Test data insertion completed!\"");

        await File.WriteAllTextAsync("insert-test-data.sh", curlCommands.ToString());
        Console.WriteLine("Generated curl commands in insert-test-data.sh");

        // Also generate PowerShell version for Windows
        var psCommands = new StringBuilder();
        psCommands.AppendLine("# Test data insertion commands for ti8m BeachBreak API (PowerShell)");
        psCommands.AppendLine("# Make sure the CommandApi is running on the expected port");
        psCommands.AppendLine("");

        psCommands.AppendLine("$BASE_URL = \"https://localhost:7001\"");
        psCommands.AppendLine("$API_VERSION = \"1.0\"");
        psCommands.AppendLine("");

        psCommands.AppendLine("Write-Host \"Inserting organizations...\"");
        psCommands.AppendLine("Invoke-RestMethod -Uri \"$BASE_URL/c/api/v$API_VERSION/organizations/bulk-import\" `");
        psCommands.AppendLine("  -Method POST `");
        psCommands.AppendLine("  -ContentType \"application/json\" `");
        psCommands.AppendLine("  -InFile \"test-organizations.json\"");
        psCommands.AppendLine("");

        psCommands.AppendLine("Write-Host \"Waiting 2 seconds...\"");
        psCommands.AppendLine("Start-Sleep -Seconds 2");
        psCommands.AppendLine("");

        psCommands.AppendLine("Write-Host \"Inserting employees...\"");
        psCommands.AppendLine("Invoke-RestMethod -Uri \"$BASE_URL/c/api/v$API_VERSION/employees/bulk-insert\" `");
        psCommands.AppendLine("  -Method POST `");
        psCommands.AppendLine("  -ContentType \"application/json\" `");
        psCommands.AppendLine("  -InFile \"test-employees.json\"");
        psCommands.AppendLine("");

        psCommands.AppendLine("Write-Host \"Test data insertion completed!\"");

        await File.WriteAllTextAsync("insert-test-data.ps1", psCommands.ToString());
        Console.WriteLine("Generated PowerShell commands in insert-test-data.ps1");
    }

    public static async Task Main(string[] args)
    {
        var generator = new TestDataGenerator();
        await generator.SaveTestDataToFiles();

        Console.WriteLine("\nFiles generated:");
        Console.WriteLine("  - test-organizations.json (organization test data)");
        Console.WriteLine("  - test-employees.json (employee test data)");
        Console.WriteLine("  - insert-test-data.sh (bash script for data insertion)");
        Console.WriteLine("  - insert-test-data.ps1 (PowerShell script for data insertion)");
        Console.WriteLine("\nTo use:");
        Console.WriteLine("1. Start the CommandApi (dotnet run in 03_Infrastructure/ti8m.BeachBreak.CommandApi)");
        Console.WriteLine("2. Run: chmod +x insert-test-data.sh && ./insert-test-data.sh");
        Console.WriteLine("   or on Windows: .\\insert-test-data.ps1");
    }
}