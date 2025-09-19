using Npgsql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeQueryHandler :
    IQueryHandler<EmployeeListQuery, Result<IEnumerable<Employee>>>,
    IQueryHandler<EmployeeQuery, Result<Employee?>>,
    IQueryHandler<EmployeeAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>,
    IQueryHandler<EmployeeResponseQuery, Result<ResponseQueries.QuestionnaireResponse>>,
    IQueryHandler<EmployeeProgressQuery, Result<IEnumerable<ProgressQueries.AssignmentProgress>>>
{
    private readonly NpgsqlDataSource dataSource;
    private readonly ILogger<EmployeeQueryHandler> logger;

    public EmployeeQueryHandler(NpgsqlDataSource dataSource, ILogger<EmployeeQueryHandler> logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<Employee>>> HandleAsync(EmployeeListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting employee list query with filters - IncludeDeleted: {IncludeDeleted}, OrganizationNumber: {OrganizationNumber}, Role: {Role}, ManagerId: {ManagerId}",
            query.IncludeDeleted, query.OrganizationNumber, query.Role, query.ManagerId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            logger.LogDebug("Database connection established for employee list query");
            await using var cmd = connection.CreateCommand();

            var sql = new StringBuilder();
            sql.AppendLine("SELECT id, first_name, last_name, role, email, start_date, end_date,");
            sql.AppendLine("       last_start_date, manager_id, manager, login_name, employee_number,");
            sql.AppendLine("       organization_number, organization, is_deleted");
            sql.AppendLine("FROM employees");
            sql.AppendLine("WHERE 1=1");

            var parameters = new List<NpgsqlParameter>();

            if (!query.IncludeDeleted)
            {
                sql.AppendLine("AND is_deleted = false");
            }

            if (query.OrganizationNumber.HasValue)
            {
                sql.AppendLine("AND organization_number = @organization_number");
                parameters.Add(new NpgsqlParameter("@organization_number", query.OrganizationNumber.Value));
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                sql.AppendLine("AND role = @role");
                parameters.Add(new NpgsqlParameter("@role", query.Role));
            }

            if (query.ManagerId.HasValue)
            {
                sql.AppendLine("AND manager_id = @manager_id");
                parameters.Add(new NpgsqlParameter("@manager_id", query.ManagerId.Value));
            }

            sql.AppendLine("ORDER BY last_name, first_name");

            cmd.CommandText = sql.ToString();
            if (parameters.Any())
            {
                cmd.Parameters.AddRange(parameters.ToArray());
            }

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var employees = new List<Employee>();

            while (await reader.ReadAsync(cancellationToken))
            {
                employees.Add(new Employee
                {
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Role = reader.GetString(3),
                    EMail = reader.GetString(4),
                    StartDate = DateOnly.FromDateTime(reader.GetDateTime(5)),
                    EndDate = reader.IsDBNull(6) ? null : DateOnly.FromDateTime(reader.GetDateTime(6)),
                    LastStartDate = reader.IsDBNull(7) ? null : DateOnly.FromDateTime(reader.GetDateTime(7)),
                    ManagerId = reader.IsDBNull(8) ? null : reader.GetGuid(8),
                    Manager = reader.GetString(9),
                    LoginName = reader.GetString(10),
                    EmployeeNumber = reader.GetString(11),
                    OrganizationNumber = reader.GetInt32(12),
                    Organization = reader.GetString(13),
                    IsDeleted = reader.GetBoolean(14)
                });
            }

            logger.LogInformation("Employee list query completed successfully, returned {EmployeeCount} employees", employees.Count);
            return Result<IEnumerable<Employee>>.Success(employees);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute employee list query");
            return Result<IEnumerable<Employee>>.Fail($"Failed to retrieve employees: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<Employee?>> HandleAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting single employee query for EmployeeId: {EmployeeId}", query.EmployeeId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            logger.LogDebug("Database connection established for single employee query");
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT id, first_name, last_name, role, email, start_date, end_date,
                       last_start_date, manager_id, manager, login_name, employee_number,
                       organization_number, organization, is_deleted
                FROM employees
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", query.EmployeeId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var employee = new Employee
                {
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Role = reader.GetString(3),
                    EMail = reader.GetString(4),
                    StartDate = DateOnly.FromDateTime(reader.GetDateTime(5)),
                    EndDate = reader.IsDBNull(6) ? null : DateOnly.FromDateTime(reader.GetDateTime(6)),
                    LastStartDate = reader.IsDBNull(7) ? null : DateOnly.FromDateTime(reader.GetDateTime(7)),
                    ManagerId = reader.IsDBNull(8) ? null : reader.GetGuid(8),
                    Manager = reader.GetString(9),
                    LoginName = reader.GetString(10),
                    EmployeeNumber = reader.GetString(11),
                    OrganizationNumber = reader.GetInt32(12),
                    Organization = reader.GetString(13),
                    IsDeleted = reader.GetBoolean(14)
                };

                logger.LogInformation("Single employee query completed successfully for EmployeeId: {EmployeeId}", query.EmployeeId);
                return Result<Employee?>.Success(employee);
            }

            logger.LogInformation("Employee not found for EmployeeId: {EmployeeId}", query.EmployeeId);
            return Result<Employee?>.Success(null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute single employee query for EmployeeId: {EmployeeId}", query.EmployeeId);
            return Result<Employee?>.Fail($"Failed to retrieve employee: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
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

    public async Task<Result<ResponseQueries.QuestionnaireResponse>> HandleAsync(EmployeeResponseQuery query, CancellationToken cancellationToken = default)
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
                var response = new ResponseQueries.QuestionnaireResponse
                {
                    Id = reader.GetGuid(0),
                    TemplateId = reader.GetGuid(1),
                    EmployeeId = reader.GetGuid(2),
                    Status = Enum.Parse<ResponseQueries.ResponseStatus>(reader.GetString(3)),
                    SectionResponses = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, object>>(reader.GetString(4)) ?? new(),
                    SubmittedDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    LastModified = reader.GetDateTime(6)
                };

                logger.LogInformation("Employee response query completed successfully");
                return Result<ResponseQueries.QuestionnaireResponse>.Success(response);
            }

            logger.LogInformation("No response found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", query.EmployeeId, query.AssignmentId);
            return Result<ResponseQueries.QuestionnaireResponse>.Fail($"Response not found", StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute employee response query for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", query.EmployeeId, query.AssignmentId);
            return Result<ResponseQueries.QuestionnaireResponse>.Fail($"Failed to retrieve employee response: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<ProgressQueries.AssignmentProgress>>> HandleAsync(EmployeeProgressQuery query, CancellationToken cancellationToken = default)
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
            var progressList = new List<ProgressQueries.AssignmentProgress>();

            while (await reader.ReadAsync(cancellationToken))
            {
                progressList.Add(new ProgressQueries.AssignmentProgress
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
            return Result<IEnumerable<ProgressQueries.AssignmentProgress>>.Success(progressList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute employee progress query for EmployeeId: {EmployeeId}", query.EmployeeId);
            return Result<IEnumerable<ProgressQueries.AssignmentProgress>>.Fail($"Failed to retrieve employee progress: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}