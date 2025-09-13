using Microsoft.Extensions.Logging;
using Npgsql;

namespace ti8m.BeachBreak.Core.Infrastructure.Database;

public class DatabaseInitializer
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(NpgsqlDataSource dataSource, ILogger<DatabaseInitializer> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");
            
            await using var connection = await _dataSource.OpenConnectionAsync();
            await CreateTablesAsync(connection);
            
            _logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database initialization");
            throw;
        }
    }

    private async Task CreateTablesAsync(NpgsqlConnection connection)
    {
        await CreateCategoriesTableAsync(connection);
        await CreateQuestionnaireTemplatesTableAsync(connection);
    }

    private async Task CreateQuestionnaireTemplatesTableAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS questionnaire_templates (
                id UUID PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                description TEXT,
                category VARCHAR(100),
                sections JSONB NOT NULL,
                settings JSONB NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                updated_at TIMESTAMP WITH TIME ZONE NOT NULL
            );

            -- Create indexes for better performance
            CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_name ON questionnaire_templates(name);
            CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_category ON questionnaire_templates(category);
            CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_created_at ON questionnaire_templates(created_at);
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        _logger.LogInformation("Creating questionnaire_templates table...");
        await command.ExecuteNonQueryAsync();
        _logger.LogInformation("questionnaire_templates table created successfully.");
    }

    private async Task CreateCategoriesTableAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS categories (
                id UUID PRIMARY KEY,
                name_en VARCHAR(100) NOT NULL,
                name_de VARCHAR(100) NOT NULL,
                description_en TEXT,
                description_de TEXT,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                last_modified TIMESTAMP WITH TIME ZONE,
                sort_order INTEGER NOT NULL DEFAULT 0
            );

            -- Create indexes for better performance
            CREATE INDEX IF NOT EXISTS idx_categories_name_en ON categories(name_en);
            CREATE INDEX IF NOT EXISTS idx_categories_name_de ON categories(name_de);
            CREATE INDEX IF NOT EXISTS idx_categories_is_active ON categories(is_active);
            CREATE INDEX IF NOT EXISTS idx_categories_sort_order ON categories(sort_order);

            -- Insert some default categories if they don't exist
            INSERT INTO categories (id, name_en, name_de, description_en, description_de, is_active, created_date, sort_order)
            SELECT * FROM (VALUES
                ('A84F5D93-EDC4-4F8E-BB30-F03DC7A983C2'::uuid, 'Performance Review', 'Leistungsbeurteilung', 'Annual and periodic performance evaluations', 'Jährliche und regelmäßige Leistungsbewertungen', true, NOW(), 10),
                ('2C2419A0-5368-4F3C-B112-84CF9F2AAB66'::uuid, 'Employee Feedback', 'Mitarbeiterfeedback', 'Employee satisfaction and feedback surveys', 'Mitarbeiterzufriedenheit und Feedback-Umfragen', true, NOW(), 20),
                ('C24C4AF5-B66F-4FE4-9229-4C7BF3FCAA33'::uuid, 'Training Assessment', 'Schulungsbewertung', 'Training effectiveness and skill assessments', 'Schulungseffektivität und Kompetenzbewertungen', true, NOW(), 30),
                ('D0238E7D-863A-4915-9472-1C2E46D33806'::uuid, 'Goal Setting', 'Zielsetzung', 'Objective setting and goal planning', 'Zielsetzung und Zielplanung', true, NOW(), 40),
                ('62FF06CC-CD5D-4332-89EB-93B2A4B18842'::uuid, 'Skills Assessment', 'Kompetenzbewertung', 'Competency and skills evaluation', 'Kompetenz- und Fertigkeitsbewertung', true, NOW(), 50),
                ('68772D86-9737-4D71-8B1C-005BE5AA257D'::uuid, 'Team Evaluation', 'Teambewertung', 'Team performance and collaboration assessment', 'Teamleistung und Bewertung der Zusammenarbeit', true, NOW(), 60),
                ('3EDD49B9-16FF-470D-9554-FE930958DBAE'::uuid, 'Exit Interview', 'Austrittsgespräch', 'Employee departure feedback collection', 'Sammlung von Feedback beim Mitarbeiterausscheiden', true, NOW(), 70),
                ('EBBC50BC-1642-4E3B-A756-EEB731ECB8F4'::uuid, 'Onboarding', 'Einarbeitung', 'New employee orientation and feedback', 'Einführung neuer Mitarbeiter und Feedback', true, NOW(), 80),
                ('078821A0-6C8F-4B5F-9EA7-88318424FC78'::uuid, 'Other', 'Sonstiges', 'Miscellaneous questionnaires', 'Verschiedene Fragebögen', true, NOW(), 90)
            ) AS default_categories(id, name_en, name_de, description_en, description_de, is_active, created_date, sort_order)
            WHERE NOT EXISTS (SELECT 1 FROM categories WHERE name_en = default_categories.name_en);
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        _logger.LogInformation("Creating categories table...");
        await command.ExecuteNonQueryAsync();
        _logger.LogInformation("Categories table created successfully.");
    }
}