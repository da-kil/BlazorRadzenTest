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
}