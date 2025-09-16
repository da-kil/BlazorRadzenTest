using Microsoft.Extensions.Logging;
using Npgsql;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class QuestionnaireAssignmentQueryHandler :
    IQueryHandler<QuestionnaireAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignment>>>,
    IQueryHandler<QuestionnaireAssignmentQuery, Result<QuestionnaireAssignment>>,
    IQueryHandler<QuestionnaireEmployeeAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignment>>>
{
    private readonly ILogger<QuestionnaireAssignmentQueryHandler> logger;
    private readonly NpgsqlDataSource dataSource;

    public QuestionnaireAssignmentQueryHandler(ILogger<QuestionnaireAssignmentQueryHandler> logger, NpgsqlDataSource dataSource)
    {
        this.logger = logger;
        this.dataSource = dataSource;
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignment>>> HandleAsync(QuestionnaireAssignmentListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving all questionnaire assignments");

            const string sql = """
                SELECT id, template_id, employee_id, employee_name, employee_email,
                       assigned_date, due_date, completed_date, status, assigned_by, notes
                FROM questionnaire_assignments
                ORDER BY assigned_date DESC
                """;

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var assignments = new List<QuestionnaireAssignment>();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                assignments.Add(MapToQuestionnaireAssignment(reader));
            }

            logger.LogInformation("Retrieved {AssignmentCount} assignments", assignments.Count);
            return Result<IEnumerable<QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all assignments");
            return Result<IEnumerable<QuestionnaireAssignment>>.Fail("Failed to retrieve assignments: " + ex.Message, 500);
        }
    }

    public async Task<Result<QuestionnaireAssignment>> HandleAsync(QuestionnaireAssignmentQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving assignment {AssignmentId}", query.Id);

            const string sql = """
                SELECT id, template_id, employee_id, employee_name, employee_email,
                       assigned_date, due_date, completed_date, status, assigned_by, notes
                FROM questionnaire_assignments
                WHERE id = @id
                """;

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@id", query.Id);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var assignment = MapToQuestionnaireAssignment(reader);
                logger.LogInformation("Retrieved assignment {AssignmentId}", assignment.Id);
                return Result<QuestionnaireAssignment>.Success(assignment);
            }

            logger.LogWarning("Assignment {AssignmentId} not found", query.Id);
            return Result<QuestionnaireAssignment>.Fail($"Assignment {query.Id} not found", 404);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment {AssignmentId}", query.Id);
            return Result<QuestionnaireAssignment>.Fail("Failed to retrieve assignment: " + ex.Message, 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignment>>> HandleAsync(QuestionnaireEmployeeAssignmentListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving assignments for employee {EmployeeId}", query.EmployeeId);

            const string sql = """
                SELECT id, template_id, employee_id, employee_name, employee_email,
                       assigned_date, due_date, completed_date, status, assigned_by, notes
                FROM questionnaire_assignments
                WHERE employee_id = @employeeId
                ORDER BY assigned_date DESC
                """;

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@employeeId", query.EmployeeId);

            var assignments = new List<QuestionnaireAssignment>();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                assignments.Add(MapToQuestionnaireAssignment(reader));
            }

            logger.LogInformation("Retrieved {AssignmentCount} assignments for employee {EmployeeId}", assignments.Count, query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Fail("Failed to retrieve employee assignments: " + ex.Message, 500);
        }
    }

    private static QuestionnaireAssignment MapToQuestionnaireAssignment(NpgsqlDataReader reader)
    {
        return new QuestionnaireAssignment
        {
            Id = reader.GetGuid(0),
            TemplateId = reader.GetGuid(1),
            EmployeeId = reader.GetGuid(2).ToString(),
            EmployeeName = reader.GetString(3),
            EmployeeEmail = reader.GetString(4),
            AssignedDate = reader.GetDateTime(5),
            DueDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            CompletedDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
            Status = Enum.Parse<AssignmentStatus>(reader.GetString(8)),
            AssignedBy = reader.IsDBNull(9) ? null : reader.GetString(9),
            Notes = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }
}
