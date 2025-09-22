using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Text.Json;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

public class QuestionnaireTemplateQueryHandler :
    IQueryHandler<QuestionnaireTemplateListQuery, Result<IEnumerable<QuestionnaireTemplate>>>,
    IQueryHandler<QuestionnaireTemplateQuery, Result<QuestionnaireTemplate>>,
    IQueryHandler<PublishedQuestionnaireTemplatesQuery, Result<IEnumerable<QuestionnaireTemplate>>>,
    IQueryHandler<DraftQuestionnaireTemplatesQuery, Result<IEnumerable<QuestionnaireTemplate>>>,
    IQueryHandler<AssignableQuestionnaireTemplatesQuery, Result<IEnumerable<QuestionnaireTemplate>>>
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
                SELECT id, name, description, category, status, published_date, last_published_date, published_by, sections, settings, created_at, updated_at
                FROM questionnaire_templates
                ORDER BY created_at DESC
                """;

            var templates = new List<QuestionnaireTemplate>();

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var template = MapQuestionnaireTemplate(reader);
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
                SELECT id, name, description, category, status, published_date, last_published_date, published_by, sections, settings, created_at, updated_at
                FROM questionnaire_templates
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", query.Id);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var template = MapQuestionnaireTemplate(reader);

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

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(PublishedQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT id, name, description, category, status, published_date, last_published_date, published_by, sections, settings, created_at, updated_at
                FROM questionnaire_templates
                WHERE status = 1
                ORDER BY last_published_date DESC, created_at DESC
                """;

            var templates = new List<QuestionnaireTemplate>();

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var template = MapQuestionnaireTemplate(reader);
                templates.Add(template);
            }

            logger.LogInformation("Retrieved {Count} published questionnaire templates", templates.Count);
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve published questionnaire templates");
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve published questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(DraftQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT id, name, description, category, status, published_date, last_published_date, published_by, sections, settings, created_at, updated_at
                FROM questionnaire_templates
                WHERE status = 0
                ORDER BY updated_at DESC, created_at DESC
                """;

            var templates = new List<QuestionnaireTemplate>();

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var template = MapQuestionnaireTemplate(reader);
                templates.Add(template);
            }

            logger.LogInformation("Retrieved {Count} draft questionnaire templates", templates.Count);
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve draft questionnaire templates");
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve draft questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(AssignableQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT id, name, description, category, status, published_date, last_published_date, published_by, sections, settings, created_at, updated_at
                FROM questionnaire_templates
                WHERE status = 1
                ORDER BY last_published_date DESC, created_at DESC
                """;

            var templates = new List<QuestionnaireTemplate>();

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var template = MapQuestionnaireTemplate(reader);
                templates.Add(template);
            }

            logger.LogInformation("Retrieved {Count} assignable questionnaire templates", templates.Count);
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve assignable questionnaire templates");
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve assignable questionnaire templates: {ex.Message}", 500);
        }
    }

    private static QuestionnaireTemplate MapQuestionnaireTemplate(NpgsqlDataReader reader)
    {
        var statusValue = reader.GetInt32("status");

        return new QuestionnaireTemplate
        {
            Id = reader.GetGuid("id"),
            Name = reader.GetString("name"),
            Description = reader.IsDBNull("description") ? string.Empty : reader.GetString("description"),
            Category = reader.IsDBNull("category") ? string.Empty : reader.GetString("category"),
            CreatedDate = reader.GetDateTime("created_at"),
            LastModified = reader.GetDateTime("updated_at"),
            Status = (TemplateStatus)statusValue,
            PublishedDate = reader.IsDBNull("published_date") ? null : reader.GetDateTime("published_date"),
            LastPublishedDate = reader.IsDBNull("last_published_date") ? null : reader.GetDateTime("last_published_date"),
            PublishedBy = reader.IsDBNull("published_by") ? string.Empty : reader.GetString("published_by"),
            Sections = JsonSerializer.Deserialize<List<QuestionSection>>(reader.GetString("sections")) ?? new(),
            Settings = JsonSerializer.Deserialize<QuestionnaireSettings>(reader.GetString("settings")) ?? new()
        };
    }
}