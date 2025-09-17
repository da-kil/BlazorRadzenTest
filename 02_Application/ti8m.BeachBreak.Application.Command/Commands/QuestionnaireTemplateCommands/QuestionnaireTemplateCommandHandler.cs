using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class QuestionnaireTemplateCommandHandler :
    ICommandHandler<CreateQuestionnaireTemplateCommand, Result>,
    ICommandHandler<UpdateQuestionnaireTemplateCommand, Result>,
    ICommandHandler<DeleteQuestionnaireTemplateCommand, Result>,
    ICommandHandler<PublishQuestionnaireTemplateCommand, Result>,
    ICommandHandler<UnpublishQuestionnaireTemplateCommand, Result>,
    ICommandHandler<ActivateQuestionnaireTemplateCommand, Result>,
    ICommandHandler<DeactivateQuestionnaireTemplateCommand, Result>
{
    private readonly NpgsqlDataSource dataSource;

    public QuestionnaireTemplateCommandHandler(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<Result> HandleAsync(CreateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.NewGuid();
            var sectionsJson = JsonSerializer.Serialize(command.QuestionnaireTemplate.Sections);
            var settingsJson = JsonSerializer.Serialize(command.QuestionnaireTemplate.Settings);

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                INSERT INTO questionnaire_templates (id, name, description, category, is_active, is_published, published_date, last_published_date, published_by, sections, settings, created_at, updated_at)
                VALUES (@id, @name, @description, @category, @is_active, @is_published, @published_date, @last_published_date, @published_by, @sections, @settings, @created_at, @updated_at)
                """;

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", command.QuestionnaireTemplate.Name);
            cmd.Parameters.AddWithValue("@description", command.QuestionnaireTemplate.Description);
            cmd.Parameters.AddWithValue("@category", command.QuestionnaireTemplate.Category);
            cmd.Parameters.AddWithValue("@is_active", command.QuestionnaireTemplate.IsActive);
            cmd.Parameters.AddWithValue("@is_published", command.QuestionnaireTemplate.IsPublished);
            cmd.Parameters.AddWithValue("@published_date", (object?)command.QuestionnaireTemplate.PublishedDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@last_published_date", (object?)command.QuestionnaireTemplate.LastPublishedDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@published_by", command.QuestionnaireTemplate.PublishedBy);
            cmd.Parameters.Add("@sections", NpgsqlDbType.Jsonb).Value = sectionsJson;
            cmd.Parameters.Add("@settings", NpgsqlDbType.Jsonb).Value = settingsJson;
            cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create questionnaire template: {ex.Message}", 400);
        }
    }

    public async Task<Result> HandleAsync(UpdateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sectionsJson = JsonSerializer.Serialize(command.QuestionnaireTemplate.Sections);
            var settingsJson = JsonSerializer.Serialize(command.QuestionnaireTemplate.Settings);

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                UPDATE questionnaire_templates
                SET name = @name,
                    description = @description,
                    category = @category,
                    is_active = @is_active,
                    is_published = @is_published,
                    published_date = @published_date,
                    last_published_date = @last_published_date,
                    published_by = @published_by,
                    sections = @sections,
                    settings = @settings,
                    updated_at = @updated_at
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", command.Id);
            cmd.Parameters.AddWithValue("@name", command.QuestionnaireTemplate.Name);
            cmd.Parameters.AddWithValue("@description", command.QuestionnaireTemplate.Description);
            cmd.Parameters.AddWithValue("@category", command.QuestionnaireTemplate.Category);
            cmd.Parameters.AddWithValue("@is_active", command.QuestionnaireTemplate.IsActive);
            cmd.Parameters.AddWithValue("@is_published", command.QuestionnaireTemplate.IsPublished);
            cmd.Parameters.AddWithValue("@published_date", (object?)command.QuestionnaireTemplate.PublishedDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@last_published_date", (object?)command.QuestionnaireTemplate.LastPublishedDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@published_by", command.QuestionnaireTemplate.PublishedBy);
            cmd.Parameters.Add("@sections", NpgsqlDbType.Jsonb).Value = sectionsJson;
            cmd.Parameters.Add("@settings", NpgsqlDbType.Jsonb).Value = settingsJson;
            cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", 400);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update questionnaire template: {ex.Message}", 400);
        }
    }

    public async Task<Result> HandleAsync(DeleteQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                DELETE FROM questionnaire_templates
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", command.Id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", 400);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete questionnaire template: {ex.Message}", 400);
        }
    }

    public async Task<Result> HandleAsync(PublishQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                UPDATE questionnaire_templates
                SET is_published = true,
                    published_date = COALESCE(published_date, @publish_date),
                    last_published_date = @publish_date,
                    published_by = @published_by,
                    updated_at = @updated_at
                WHERE id = @id AND is_active = true
                """;

            var publishDate = DateTime.UtcNow;
            cmd.Parameters.AddWithValue("@id", command.Id);
            cmd.Parameters.AddWithValue("@publish_date", publishDate);
            cmd.Parameters.AddWithValue("@published_by", command.PublishedBy);
            cmd.Parameters.AddWithValue("@updated_at", publishDate);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found or not active", 400);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to publish questionnaire template: {ex.Message}", 400);
        }
    }

    public async Task<Result> HandleAsync(UnpublishQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                UPDATE questionnaire_templates
                SET is_published = false,
                    updated_at = @updated_at
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", command.Id);
            cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", 400);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to unpublish questionnaire template: {ex.Message}", 400);
        }
    }

    public async Task<Result> HandleAsync(ActivateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                UPDATE questionnaire_templates
                SET is_active = true,
                    updated_at = @updated_at
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", command.Id);
            cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", 400);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to activate questionnaire template: {ex.Message}", 400);
        }
    }

    public async Task<Result> HandleAsync(DeactivateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                UPDATE questionnaire_templates
                SET is_active = false,
                    updated_at = @updated_at
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", command.Id);
            cmd.Parameters.AddWithValue("@updated_at", DateTime.UtcNow);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", 400);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to deactivate questionnaire template: {ex.Message}", 400);
        }
    }
}