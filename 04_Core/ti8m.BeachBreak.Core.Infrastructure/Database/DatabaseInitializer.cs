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
        // Only create tables for entities that are NOT using event sourcing
        // Categories, QuestionnaireTemplates, Employees, and QuestionnaireAssignments are now event-sourced via MartenDB
        await CreateResponsesTableAsync(connection);
    }





    private async Task CreateResponsesTableAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS questionnaire_responses (
                id UUID PRIMARY KEY,
                assignment_id UUID NOT NULL,
                template_id UUID NOT NULL,
                employee_id UUID NOT NULL,
                status VARCHAR(50) NOT NULL DEFAULT 'InProgress',
                section_responses JSONB NOT NULL,
                submitted_date TIMESTAMP WITH TIME ZONE,
                last_modified TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );

            -- Create unique index for upsert operations
            CREATE UNIQUE INDEX IF NOT EXISTS idx_responses_assignment_employee
            ON questionnaire_responses(assignment_id, employee_id);

            -- Create indexes for better performance
            CREATE INDEX IF NOT EXISTS idx_responses_assignment_id ON questionnaire_responses(assignment_id);
            CREATE INDEX IF NOT EXISTS idx_responses_template_id ON questionnaire_responses(template_id);
            CREATE INDEX IF NOT EXISTS idx_responses_employee_id ON questionnaire_responses(employee_id);
            CREATE INDEX IF NOT EXISTS idx_responses_status ON questionnaire_responses(status);
            CREATE INDEX IF NOT EXISTS idx_responses_submitted_date ON questionnaire_responses(submitted_date);
            CREATE INDEX IF NOT EXISTS idx_responses_last_modified ON questionnaire_responses(last_modified);
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        _logger.LogInformation("Creating questionnaire_responses table...");
        await command.ExecuteNonQueryAsync();
        _logger.LogInformation("Questionnaire responses table created successfully.");
    }
}