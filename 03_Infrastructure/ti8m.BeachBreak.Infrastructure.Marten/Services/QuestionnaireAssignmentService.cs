using Npgsql;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;

namespace ti8m.BeachBreak.Infrastructure.Marten.Services;

public class QuestionnaireAssignmentService : IQuestionnaireAssignmentService
{
    private readonly NpgsqlDataSource dataSource;

    public QuestionnaireAssignmentService(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<bool> HasActiveAssignmentsAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        var count = await GetActiveAssignmentCountAsync(templateId, cancellationToken);
        return count > 0;
    }

    public async Task<int> GetActiveAssignmentCountAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = connection.CreateCommand();

        cmd.CommandText = """
            SELECT COUNT(*)
            FROM questionnaire_assignments
            WHERE template_id = @template_id
            AND status IN (@assigned_status, @in_progress_status, @overdue_status)
            """;

        cmd.Parameters.AddWithValue("@template_id", templateId);
        cmd.Parameters.AddWithValue("@assigned_status", 0); // Assigned
        cmd.Parameters.AddWithValue("@in_progress_status", 1); // InProgress
        cmd.Parameters.AddWithValue("@overdue_status", 3); // Overdue

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result ?? 0);
    }
}