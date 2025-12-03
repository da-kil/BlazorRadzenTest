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

    public List<UITranslationDto> GenerateUITranslations()
    {
        return new List<UITranslationDto>
        {
            // Navigation
            new() { Key = "nav.menu-toggle", German = "Navigationsmenü", English = "Navigation menu", Category = "navigation" },
            new() { Key = "nav.dashboard", German = "Dashboard", English = "Dashboard", Category = "navigation" },
            new() { Key = "nav.my-work", German = "Meine Arbeit", English = "My Work", Category = "navigation" },
            new() { Key = "nav.questionnaires", German = "Fragebogen", English = "Questionnaires", Category = "navigation" },
            new() { Key = "nav.my-questionnaires", German = "Meine Fragebogen", English = "My Questionnaires", Category = "navigation" },
            new() { Key = "nav.team-overview", German = "Team Übersicht", English = "Team Overview", Category = "navigation" },
            new() { Key = "nav.team-questionnaires", German = "Team Fragebogen", English = "Team Questionnaires", Category = "navigation" },
            new() { Key = "nav.organization", German = "Organisation", English = "Organization", Category = "navigation" },
            new() { Key = "nav.management", German = "Verwaltung", English = "Management", Category = "navigation" },
            new() { Key = "nav.create-questionnaire", German = "Fragebogen erstellen", English = "Create Questionnaire", Category = "navigation" },
            new() { Key = "nav.manage-questionnaires", German = "Fragebogen verwalten", English = "Manage Questionnaires", Category = "navigation" },
            new() { Key = "nav.assignments", German = "Zuweisungen", English = "Assignments", Category = "navigation" },
            new() { Key = "nav.administration", German = "Administration", English = "Administration", Category = "navigation" },
            new() { Key = "nav.questionnaire-management", German = "Fragebogen Verwaltung", English = "Questionnaire Management", Category = "navigation" },
            new() { Key = "nav.categories", German = "Kategorien", English = "Categories", Category = "navigation" },
            new() { Key = "nav.role-management", German = "Rollenverwaltung", English = "Role Management", Category = "navigation" },
            new() { Key = "nav.projection-replay", German = "Projektions-Wiederholung", English = "Projection Replay", Category = "navigation" },

            // Common buttons
            new() { Key = "buttons.save", German = "Speichern", English = "Save", Category = "buttons" },
            new() { Key = "buttons.cancel", German = "Abbrechen", English = "Cancel", Category = "buttons" },
            new() { Key = "buttons.delete", German = "Löschen", English = "Delete", Category = "buttons" },
            new() { Key = "buttons.edit", German = "Bearbeiten", English = "Edit", Category = "buttons" },
            new() { Key = "buttons.add", German = "Hinzufügen", English = "Add", Category = "buttons" },
            new() { Key = "buttons.submit", German = "Absenden", English = "Submit", Category = "buttons" },
            new() { Key = "buttons.close", German = "Schließen", English = "Close", Category = "buttons" },
            new() { Key = "buttons.confirm", German = "Bestätigen", English = "Confirm", Category = "buttons" },
            new() { Key = "buttons.previous", German = "Zurück", English = "Previous", Category = "buttons" },
            new() { Key = "buttons.next", German = "Weiter", English = "Next", Category = "buttons" },
            new() { Key = "buttons.save-progress", German = "Fortschritt speichern", English = "Save Progress", Category = "buttons" },
            new() { Key = "buttons.saving", German = "Speichere...", English = "Saving...", Category = "buttons" },
            new() { Key = "buttons.submitting", German = "Sende...", English = "Submitting...", Category = "buttons" },
            new() { Key = "buttons.retry", German = "Wiederholen", English = "Retry", Category = "buttons" },
            new() { Key = "buttons.loading", German = "Lädt", English = "Loading", Category = "buttons" },
            new() { Key = "buttons.create-new", German = "Neu erstellen", English = "Create New", Category = "buttons" },

            // Common labels
            new() { Key = "labels.name", German = "Name", English = "Name", Category = "labels" },
            new() { Key = "labels.description", German = "Beschreibung", English = "Description", Category = "labels" },
            new() { Key = "labels.category", German = "Kategorie", English = "Category", Category = "labels" },
            new() { Key = "labels.status", German = "Status", English = "Status", Category = "labels" },

            // Form labels and placeholders
            new() { Key = "forms.objective-description-required", German = "Zielbeschreibung*", English = "Objective Description*", Category = "forms" },
            new() { Key = "forms.measurement-metric-required", German = "Messgröße*", English = "Measurement Metric*", Category = "forms" },
            new() { Key = "forms.start-date-required", German = "Startdatum*", English = "Start Date*", Category = "forms" },
            new() { Key = "forms.end-date-required", German = "Enddatum*", English = "End Date*", Category = "forms" },
            new() { Key = "forms.weighting", German = "Gewichtung", English = "Weighting", Category = "forms" },
            new() { Key = "forms.weighting-percentage-required", German = "Gewichtung Prozentsatz*", English = "Weighting Percentage*", Category = "forms" },
            new() { Key = "forms.reason-for-change-required", German = "Grund für Änderung*", English = "Reason for Change*", Category = "forms" },
            new() { Key = "forms.describe-goal-objective", German = "Beschreiben Sie das Ziel...", English = "Describe the goal objective...", Category = "forms" },
            new() { Key = "forms.how-measured", German = "Wie wird dieses Ziel gemessen?", English = "How will this goal be measured?", Category = "forms" },
            new() { Key = "forms.weighting-help-text", German = "Prozentsatz der Gesamtziele (0-100%). Setzen Sie die Gewichtung, um die Wichtigkeit zwischen den Zielen zu verteilen.", English = "Percentage of total goals (0-100%). Set weighting to allocate importance across goals.", Category = "forms" },
            new() { Key = "forms.explain-goal-modification", German = "Erklären Sie, warum dieses Ziel geändert wird...", English = "Explain why this goal is being modified...", Category = "forms" },
            new() { Key = "forms.characters", German = "Zeichen", English = "characters", Category = "forms" },

            // Validation messages
            new() { Key = "validation.fix-errors", German = "Bitte beheben Sie die folgenden Fehler", English = "Please fix the following errors", Category = "validation" },
            new() { Key = "validation.required", German = "Erforderlich", English = "Required", Category = "validation" },
            new() { Key = "validation.email", German = "Ungültige E-Mail-Adresse", English = "Invalid email address", Category = "validation" },
            new() { Key = "validation.min-length", German = "Mindestlänge erforderlich", English = "Minimum length required", Category = "validation" },

            // Notification titles
            new() { Key = "notifications.success", German = "Erfolg", English = "Success", Category = "notifications" },
            new() { Key = "notifications.error", German = "Fehler", English = "Error", Category = "notifications" },
            new() { Key = "notifications.warning", German = "Warnung", English = "Warning", Category = "notifications" },
            new() { Key = "notifications.info", German = "Information", English = "Information", Category = "notifications" },
            new() { Key = "notifications.failed", German = "Fehlgeschlagen", English = "Failed", Category = "notifications" },

            // Dialog buttons and actions
            new() { Key = "dialogs.add-goal", German = "Ziel hinzufügen", English = "Add Goal", Category = "dialogs" },
            new() { Key = "dialogs.update-goal", German = "Ziel aktualisieren", English = "Update Goal", Category = "dialogs" },
            new() { Key = "dialogs.adding-goal", German = "Ziel wird hinzugefügt...", English = "Adding Goal...", Category = "dialogs" },
            new() { Key = "dialogs.updating-goal", German = "Ziel wird aktualisiert...", English = "Updating Goal...", Category = "dialogs" },
            new() { Key = "dialogs.save-changes", German = "Änderungen speichern", English = "Save Changes", Category = "dialogs" },
            new() { Key = "dialogs.saving", German = "Speichere...", English = "Saving", Category = "dialogs" },
            new() { Key = "dialogs.employee-sign-off", German = "Mitarbeiter Bestätigung", English = "Employee Sign-Off", Category = "dialogs" },
            new() { Key = "dialogs.confirm-review-message", German = "Sie bestätigen, dass Sie die Fragebogen-Ergebnisse besprochen und zur Kenntnis genommen haben.", English = "You are about to confirm that you have reviewed and acknowledged the questionnaire results discussed with your manager.", Category = "dialogs" },
            new() { Key = "dialogs.sign-off-review", German = "Besprechung bestätigen", English = "Sign Off on Review", Category = "dialogs" },
            new() { Key = "dialogs.signing-off", German = "Bestätige...", English = "Signing Off", Category = "dialogs" },
            new() { Key = "dialogs.close", German = "Schließen", English = "Close", Category = "dialogs" },
            new() { Key = "dialogs.confirm", German = "Bestätigen", English = "Confirm", Category = "dialogs" },

            // Status indicators
            new() { Key = "status.overdue", German = "Überfällig", English = "Overdue", Category = "status" },
            new() { Key = "status.due-soon", German = "Bald fällig", English = "Due Soon", Category = "status" },

            // Dashboard messages
            new() { Key = "dashboard.no-data", German = "Keine Dashboard-Daten", English = "No Dashboard Data", Category = "dashboard" },
            new() { Key = "dashboard.unable-to-load", German = "Dashboard-Informationen können nicht geladen werden.", English = "Unable to load dashboard information.", Category = "dashboard" },

            // Tab labels
            new() { Key = "tabs.templates", German = "Vorlagen", English = "Templates", Category = "tabs" },
            new() { Key = "tabs.settings", German = "Einstellungen", English = "Settings", Category = "tabs" },

            // Filter labels
            new() { Key = "filters.filter-by-type", German = "Nach Typ filtern", English = "Filter by Type", Category = "filters" },
            new() { Key = "filters.search-templates", German = "Vorlagen suchen...", English = "Search templates...", Category = "filters" },
            new() { Key = "filters.show-archived", German = "Archivierte anzeigen", English = "Show Archived", Category = "filters" },

            // Settings labels
            new() { Key = "settings.send-assignment-notifications", German = "Zuweisungsbenachrichtigungen senden", English = "Send assignment notifications", Category = "settings" },
            new() { Key = "settings.send-reminder-emails", German = "Erinnerungs-E-Mails senden", English = "Send reminder emails", Category = "settings" },
            new() { Key = "settings.send-completion-confirmations", German = "Abschlussbestätigungen senden", English = "Send completion confirmations", Category = "settings" },

            // Page titles and descriptions
            new() { Key = "pages.team-questionnaires", German = "Team Fragebogen", English = "Team Questionnaires", Category = "pages" },
            new() { Key = "pages.team-questionnaires-description", German = "Fragebogen für Ihre Teammitglieder anzeigen und verwalten", English = "View and manage questionnaires for your team members", Category = "pages" },

            // Data grid column headers
            new() { Key = "columns.first-name", German = "Vorname", English = "First Name", Category = "columns" },
            new() { Key = "columns.last-name", German = "Nachname", English = "Last Name", Category = "columns" },
            new() { Key = "columns.job-role", German = "Berufsrolle", English = "Job Role", Category = "columns" },
            new() { Key = "columns.organization", German = "Organisation", English = "Organization", Category = "columns" },
            new() { Key = "columns.application-role", German = "Anwendungsrolle", English = "Application Role", Category = "columns" },
            new() { Key = "columns.template-name", German = "Vorlagenname", English = "Template Name", Category = "columns" },
            new() { Key = "columns.category", German = "Kategorie", English = "Category", Category = "columns" },
            new() { Key = "columns.sections", German = "Abschnitte", English = "Sections", Category = "columns" },
            new() { Key = "columns.questions", German = "Fragen", English = "Questions", Category = "columns" },
            new() { Key = "columns.status", German = "Status", English = "Status", Category = "columns" },
            new() { Key = "columns.created", German = "Erstellt", English = "Created", Category = "columns" },
            new() { Key = "columns.published", German = "Veröffentlicht", English = "Published", Category = "columns" },
            new() { Key = "columns.name-en", German = "Name EN", English = "Name EN", Category = "columns" },
            new() { Key = "columns.name-de", German = "Name DE", English = "Name DE", Category = "columns" },
            new() { Key = "columns.description-en", German = "Beschreibung EN", English = "Description EN", Category = "columns" },
            new() { Key = "columns.description-de", German = "Beschreibung DE", English = "Description DE", Category = "columns" },
            new() { Key = "columns.sort-order", German = "Sortierreihenfolge", English = "Sort Order", Category = "columns" },
            new() { Key = "columns.actions", German = "Aktionen", English = "Actions", Category = "columns" },

            // Additional filter options
            new() { Key = "filters.search-employees", German = "Mitarbeiter suchen...", English = "Search employees...", Category = "filters" },
            new() { Key = "filters.search-types", German = "Typen suchen...", English = "Search types...", Category = "filters" },
            new() { Key = "filters.show-inactive", German = "Inaktive anzeigen", English = "Show Inactive", Category = "filters" },

            // Status labels
            new() { Key = "status.active", German = "Aktiv", English = "Active", Category = "status" },
            new() { Key = "status.inactive", German = "Inaktiv", English = "Inactive", Category = "status" },

            // Button types
            new() { Key = "buttons.add-new-type", German = "Neuen Typ hinzufügen", English = "Add New Type", Category = "buttons" },

            // Placeholder text for forms
            new() { Key = "placeholders.enter-english-name", German = "Englischen Namen eingeben", English = "Enter English name", Category = "placeholders" },
            new() { Key = "placeholders.enter-german-name", German = "Deutschen Namen eingeben", English = "Enter German name", Category = "placeholders" },
            new() { Key = "placeholders.enter-english-description", German = "Englische Beschreibung eingeben (optional)", English = "Enter English description (optional)", Category = "placeholders" },
            new() { Key = "placeholders.enter-german-description", German = "Deutsche Beschreibung eingeben (optional)", English = "Enter German description (optional)", Category = "placeholders" },

            // Common labels
            new() { Key = "labels.uncategorized", German = "Unkategorisiert", English = "Uncategorized", Category = "labels" },

            // Language names
            new() { Key = "language.german", German = "Deutsch", English = "German", Category = "language" },
            new() { Key = "language.english", German = "Englisch", English = "English", Category = "language" }
        };
    }

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

        // Generate UI translations
        var translations = GenerateUITranslations();
        Console.WriteLine($"Generated {translations.Count} UI translations");

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

        // Save translations to file
        var translationsJson = JsonSerializer.Serialize(translations, jsonOptions);
        await File.WriteAllTextAsync("test-translations.json", translationsJson);
        Console.WriteLine("Saved translations to test-translations.json");

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