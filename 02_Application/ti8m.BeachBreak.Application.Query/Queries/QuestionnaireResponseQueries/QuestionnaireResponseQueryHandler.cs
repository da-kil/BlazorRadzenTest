using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;
using ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireResponseQueries;

public class QuestionnaireResponseQueryHandler :
    IQueryHandler<EmployeeAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>,
    IQueryHandler<EmployeeResponseQuery, Result<QuestionnaireResponse>>,
    IQueryHandler<EmployeeProgressQuery, Result<IEnumerable<AssignmentProgress>>>
{
    private readonly NpgsqlDataSource dataSource;
    private readonly ILogger<QuestionnaireResponseQueryHandler> logger;

    public QuestionnaireResponseQueryHandler(NpgsqlDataSource dataSource, ILogger<QuestionnaireResponseQueryHandler> logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>> HandleAsync(EmployeeAssignmentListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting employee assignments query for EmployeeId: {EmployeeId}", query.EmployeeId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT id, template_id, employee_id, employee_name, employee_email,
                       assigned_date, due_date, completed_date, status, assigned_by, notes
                FROM questionnaire_assignments
                WHERE employee_id = @employeeId
                ORDER BY assigned_date DESC
                """;

            cmd.Parameters.AddWithValue("@employeeId", query.EmployeeId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var assignments = new List<QuestionnaireAssignmentQueries.QuestionnaireAssignment>();

            while (await reader.ReadAsync(cancellationToken))
            {
                assignments.Add(new QuestionnaireAssignmentQueries.QuestionnaireAssignment
                {
                    Id = reader.GetGuid(0),
                    TemplateId = reader.GetGuid(1),
                    EmployeeId = reader.GetGuid(2).ToString(),
                    EmployeeName = reader.GetString(3),
                    EmployeeEmail = reader.GetString(4),
                    AssignedDate = reader.GetDateTime(5),
                    DueDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    CompletedDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    Status = Enum.Parse<QuestionnaireAssignmentQueries.AssignmentStatus>(reader.GetString(8)),
                    AssignedBy = reader.IsDBNull(9) ? null : reader.GetString(9),
                    Notes = reader.IsDBNull(10) ? null : reader.GetString(10)
                });
            }

            logger.LogInformation("Employee assignments query completed successfully, returned {AssignmentCount} assignments", assignments.Count);
            return Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute employee assignments query for EmployeeId: {EmployeeId}", query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>.Fail($"Failed to retrieve employee assignments: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<QuestionnaireResponse>> HandleAsync(EmployeeResponseQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting employee response query for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", query.EmployeeId, query.AssignmentId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT r.id, r.template_id, r.employee_id, r.status, r.section_responses, r.submitted_date, r.last_modified
                FROM questionnaire_responses r
                INNER JOIN questionnaire_assignments a ON r.assignment_id = a.id
                WHERE r.employee_id = @employeeId AND a.id = @assignmentId
                """;

            cmd.Parameters.AddWithValue("@employeeId", query.EmployeeId);
            cmd.Parameters.AddWithValue("@assignmentId", query.AssignmentId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var response = new QuestionnaireResponse
                {
                    Id = reader.GetGuid(0),
                    TemplateId = reader.GetGuid(1),
                    EmployeeId = reader.GetGuid(2),
                    Status = Enum.Parse<ResponseStatus>(reader.GetString(3)),
                    SectionResponses = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, object>>(reader.GetString(4)) ?? new(),
                    SubmittedDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    LastModified = reader.GetDateTime(6)
                };

                logger.LogInformation("Employee response query completed successfully");
                return Result<QuestionnaireResponse>.Success(response);
            }

            logger.LogInformation("No response found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", query.EmployeeId, query.AssignmentId);
            return Result<QuestionnaireResponse>.Fail($"Response not found", StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute employee response query for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", query.EmployeeId, query.AssignmentId);
            return Result<QuestionnaireResponse>.Fail($"Failed to retrieve employee response: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<AssignmentProgress>>> HandleAsync(EmployeeProgressQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting employee progress query for EmployeeId: {EmployeeId}", query.EmployeeId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT a.id, a.template_id,
                       COALESCE(r.progress_percentage, 0) as progress_percentage,
                       COALESCE(r.answered_questions, 0) as answered_questions,
                       t.total_questions,
                       COALESCE(r.last_modified, a.assigned_date) as last_modified,
                       CASE WHEN a.status = 'Completed' THEN true ELSE false END as is_completed,
                       r.time_spent
                FROM questionnaire_assignments a
                LEFT JOIN questionnaire_responses r ON a.id = r.assignment_id
                LEFT JOIN (
                    SELECT template_id, COUNT(*) as total_questions
                    FROM questionnaire_template_questions
                    GROUP BY template_id
                ) t ON a.template_id = t.template_id
                WHERE a.employee_id = @employeeId
                ORDER BY a.assigned_date DESC
                """;

            cmd.Parameters.AddWithValue("@employeeId", query.EmployeeId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var progressList = new List<AssignmentProgress>();

            while (await reader.ReadAsync(cancellationToken))
            {
                progressList.Add(new AssignmentProgress
                {
                    AssignmentId = reader.GetGuid(0),
                    TemplateId = reader.GetGuid(1),
                    ProgressPercentage = reader.GetInt32(2),
                    AnsweredQuestions = reader.GetInt32(3),
                    TotalQuestions = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    LastModified = reader.GetDateTime(5),
                    IsCompleted = reader.GetBoolean(6),
                    TimeSpent = reader.IsDBNull(7) ? null : TimeSpan.FromSeconds(reader.GetInt32(7))
                });
            }

            logger.LogInformation("Employee progress query completed successfully, returned {ProgressCount} progress records", progressList.Count);
            return Result<IEnumerable<AssignmentProgress>>.Success(progressList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute employee progress query for EmployeeId: {EmployeeId}", query.EmployeeId);
            return Result<IEnumerable<AssignmentProgress>>.Fail($"Failed to retrieve employee progress: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}