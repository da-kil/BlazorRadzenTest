using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Text.Json;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

public class QuestionnaireTemplateQueryHandler : 
    IQueryHandler<QuestionnaireTemplateListQuery, Result<IEnumerable<QuestionnaireTemplate>>>,
    IQueryHandler<QuestionnaireTemplateQuery, Result<QuestionnaireTemplate>>
{
    private readonly NpgsqlDataSource dataSource;
    private readonly ILogger<QuestionnaireTemplateQueryHandler> logger;

    public QuestionnaireTemplateQueryHandler(NpgsqlDataSource dataSource, ILogger<QuestionnaireTemplateQueryHandler> logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(QuestionnaireTemplateListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();
            
            cmd.CommandText = """
                SELECT id, name, description, category, sections, settings, created_at, updated_at
                FROM questionnaire_templates 
                ORDER BY created_at DESC
                """;

            var templates = new List<QuestionnaireTemplate>();
            
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var template = new QuestionnaireTemplate
                {
                    Id = reader.GetGuid("id"),
                    Name = reader.GetString("name"),
                    Description = reader.IsDBNull("description") ? string.Empty : reader.GetString("description"),
                    Category = reader.IsDBNull("category") ? string.Empty : reader.GetString("category"),
                    CreatedDate = reader.GetDateTime("created_at"),
                    LastModified = reader.GetDateTime("updated_at"),
                    IsActive = true,
                    Sections = JsonSerializer.Deserialize<List<QuestionSection>>(reader.GetString("sections")) ?? new(),
                    Settings = JsonSerializer.Deserialize<QuestionnaireSettings>(reader.GetString("settings")) ?? new()
                };
                
                templates.Add(template);
            }

            logger.LogInformation("Retrieved {Count} questionnaire templates", templates.Count);
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve questionnaire templates");
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<QuestionnaireTemplate>> HandleAsync(QuestionnaireTemplateQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();
            
            cmd.CommandText = """
                SELECT id, name, description, category, sections, settings, created_at, updated_at
                FROM questionnaire_templates 
                WHERE id = @id
                """;
            
            cmd.Parameters.AddWithValue("@id", query.Id);
            
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var template = new QuestionnaireTemplate
                {
                    Id = reader.GetGuid("id"),
                    Name = reader.GetString("name"),
                    Description = reader.IsDBNull("description") ? string.Empty : reader.GetString("description"),
                    Category = reader.IsDBNull("category") ? string.Empty : reader.GetString("category"),
                    CreatedDate = reader.GetDateTime("created_at"),
                    LastModified = reader.GetDateTime("updated_at"),
                    IsActive = true,
                    Sections = JsonSerializer.Deserialize<List<QuestionSection>>(reader.GetString("sections")) ?? new(),
                    Settings = JsonSerializer.Deserialize<QuestionnaireSettings>(reader.GetString("settings")) ?? new()
                };
                
                logger.LogInformation("Retrieved questionnaire template with ID {Id}", query.Id);
                return Result<QuestionnaireTemplate>.Success(template);
            }
            
            logger.LogWarning("Questionnaire template with ID {Id} not found", query.Id);
            return Result<QuestionnaireTemplate>.Fail($"Questionnaire template with ID {query.Id} not found", 404);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve questionnaire template with ID {Id}", query.Id);
            return Result<QuestionnaireTemplate>.Fail($"Failed to retrieve questionnaire template: {ex.Message}", 500);
        }
    }
}
