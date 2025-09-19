using Npgsql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerQueryHandler :
    IQueryHandler<ManagerTeamListQuery, Result<IEnumerable<EmployeeQueries.Employee>>>,
    IQueryHandler<ManagerTeamAssignmentsQuery, Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>,
    IQueryHandler<ManagerTeamProgressQuery, Result<IEnumerable<ProgressQueries.AssignmentProgress>>>,
    IQueryHandler<ManagerTeamAnalyticsQuery, Result<AnalyticsQueries.TeamAnalytics>>,
    IQueryHandler<ManagerTeamPerformanceReportQuery, Result<ReportQueries.TeamPerformanceReport>>
{
    private readonly NpgsqlDataSource dataSource;
    private readonly ILogger<ManagerQueryHandler> logger;

    public ManagerQueryHandler(NpgsqlDataSource dataSource, ILogger<ManagerQueryHandler> logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<EmployeeQueries.Employee>>> HandleAsync(ManagerTeamListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting manager team list query for ManagerId: {ManagerId}", query.ManagerId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT id, first_name, last_name, role, email, start_date, end_date,
                       last_start_date, manager_id, manager, login_name, employee_number,
                       organization_number, organization, is_deleted
                FROM employees
                WHERE manager_id = @managerId AND is_deleted = false
                ORDER BY last_name, first_name
                """;

            cmd.Parameters.AddWithValue("@managerId", query.ManagerId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var employees = new List<EmployeeQueries.Employee>();

            while (await reader.ReadAsync(cancellationToken))
            {
                employees.Add(new EmployeeQueries.Employee
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

            logger.LogInformation("Manager team list query completed successfully, returned {EmployeeCount} employees", employees.Count);
            return Result<IEnumerable<EmployeeQueries.Employee>>.Success(employees);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute manager team list query for ManagerId: {ManagerId}", query.ManagerId);
            return Result<IEnumerable<EmployeeQueries.Employee>>.Fail($"Failed to retrieve team members: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>> HandleAsync(ManagerTeamAssignmentsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting manager team assignments query for ManagerId: {ManagerId}", query.ManagerId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT a.id, a.template_id, a.employee_id, a.employee_name, a.employee_email,
                       a.assigned_date, a.due_date, a.completed_date, a.status, a.assigned_by, a.notes
                FROM questionnaire_assignments a
                INNER JOIN employees e ON a.employee_id = e.id
                WHERE e.manager_id = @managerId
                ORDER BY a.assigned_date DESC
                """;

            cmd.Parameters.AddWithValue("@managerId", query.ManagerId);

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

            logger.LogInformation("Manager team assignments query completed successfully, returned {AssignmentCount} assignments", assignments.Count);
            return Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute manager team assignments query for ManagerId: {ManagerId}", query.ManagerId);
            return Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>.Fail($"Failed to retrieve team assignments: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<ProgressQueries.AssignmentProgress>>> HandleAsync(ManagerTeamProgressQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting manager team progress query for ManagerId: {ManagerId}", query.ManagerId);

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
                INNER JOIN employees e ON a.employee_id = e.id
                LEFT JOIN questionnaire_responses r ON a.id = r.assignment_id
                LEFT JOIN (
                    SELECT template_id, COUNT(*) as total_questions
                    FROM questionnaire_template_questions
                    GROUP BY template_id
                ) t ON a.template_id = t.template_id
                WHERE e.manager_id = @managerId
                ORDER BY a.assigned_date DESC
                """;

            cmd.Parameters.AddWithValue("@managerId", query.ManagerId);

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

            logger.LogInformation("Manager team progress query completed successfully, returned {ProgressCount} progress records", progressList.Count);
            return Result<IEnumerable<ProgressQueries.AssignmentProgress>>.Success(progressList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute manager team progress query for ManagerId: {ManagerId}", query.ManagerId);
            return Result<IEnumerable<ProgressQueries.AssignmentProgress>>.Fail($"Failed to retrieve team progress: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<AnalyticsQueries.TeamAnalytics>> HandleAsync(ManagerTeamAnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting manager team analytics query for ManagerId: {ManagerId}", query.ManagerId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT
                    COUNT(DISTINCT e.id) as total_team_members,
                    COUNT(CASE WHEN a.status = 'Assigned' OR a.status = 'InProgress' THEN 1 END) as active_assignments,
                    COUNT(CASE WHEN a.status = 'Completed' THEN 1 END) as completed_assignments,
                    COUNT(CASE WHEN a.due_date < CURRENT_DATE AND a.status != 'Completed' THEN 1 END) as overdue_assignments,
                    AVG(EXTRACT(EPOCH FROM (a.completed_date - a.assigned_date))/86400) as avg_completion_days,
                    CASE
                        WHEN COUNT(a.id) > 0
                        THEN ROUND((COUNT(CASE WHEN a.status = 'Completed' THEN 1 END)::decimal / COUNT(a.id) * 100), 2)
                        ELSE 0
                    END as completion_rate
                FROM employees e
                LEFT JOIN questionnaire_assignments a ON e.id = a.employee_id
                WHERE e.manager_id = @managerId AND e.is_deleted = false
                """;

            cmd.Parameters.AddWithValue("@managerId", query.ManagerId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var analytics = new AnalyticsQueries.TeamAnalytics
                {
                    TotalTeamMembers = reader.GetInt32(0),
                    ActiveAssignments = reader.GetInt32(1),
                    CompletedAssignments = reader.GetInt32(2),
                    OverdueAssignments = reader.GetInt32(3),
                    AverageCompletionTime = reader.IsDBNull(4) ? null : TimeSpan.FromDays(reader.GetDouble(4)),
                    CompletionRate = reader.GetDecimal(5),
                    TeamPerformanceMetrics = new Dictionary<string, object>()
                };

                logger.LogInformation("Manager team analytics query completed successfully");
                return Result<AnalyticsQueries.TeamAnalytics>.Success(analytics);
            }

            return Result<AnalyticsQueries.TeamAnalytics>.Fail("No analytics data found", StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute manager team analytics query for ManagerId: {ManagerId}", query.ManagerId);
            return Result<AnalyticsQueries.TeamAnalytics>.Fail($"Failed to retrieve team analytics: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<ReportQueries.TeamPerformanceReport>> HandleAsync(ManagerTeamPerformanceReportQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting manager team performance report query for ManagerId: {ManagerId}, Period: {ReportPeriod}", query.ManagerId, query.ReportPeriod);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            var report = new ReportQueries.TeamPerformanceReport
            {
                ReportPeriod = query.ReportPeriod,
                TeamMetrics = new Dictionary<string, object>(),
                IndividualPerformances = new List<ReportQueries.IndividualPerformance>(),
                TrendAnalysis = new Dictionary<string, object>(),
                Recommendations = new List<string>()
            };

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT
                    e.id,
                    e.first_name || ' ' || e.last_name as employee_name,
                    COUNT(a.id) as total_assignments,
                    COUNT(CASE WHEN a.status = 'Completed' THEN 1 END) as completed_assignments,
                    CASE
                        WHEN COUNT(a.id) > 0
                        THEN ROUND((COUNT(CASE WHEN a.status = 'Completed' THEN 1 END)::decimal / COUNT(a.id) * 100), 2)
                        ELSE 0
                    END as completion_rate,
                    AVG(EXTRACT(EPOCH FROM (a.completed_date - a.assigned_date))/86400) as avg_completion_days
                FROM employees e
                LEFT JOIN questionnaire_assignments a ON e.id = a.employee_id
                    AND a.assigned_date >= @periodStart AND a.assigned_date <= @periodEnd
                WHERE e.manager_id = @managerId AND e.is_deleted = false
                GROUP BY e.id, e.first_name, e.last_name
                ORDER BY completion_rate DESC
                """;

            var periodStart = DateTime.UtcNow.AddDays(-30);
            var periodEnd = DateTime.UtcNow;

            cmd.Parameters.AddWithValue("@managerId", query.ManagerId);
            cmd.Parameters.AddWithValue("@periodStart", periodStart);
            cmd.Parameters.AddWithValue("@periodEnd", periodEnd);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                report.IndividualPerformances.Add(new ReportQueries.IndividualPerformance
                {
                    EmployeeId = reader.GetGuid(0),
                    EmployeeName = reader.GetString(1),
                    CompletedAssignments = reader.GetInt32(3),
                    CompletionRate = reader.GetDecimal(4),
                    AverageCompletionTime = reader.IsDBNull(5) ? null : TimeSpan.FromDays(reader.GetDouble(5)),
                    PerformanceMetrics = new Dictionary<string, object>()
                });
            }

            logger.LogInformation("Manager team performance report query completed successfully");
            return Result<ReportQueries.TeamPerformanceReport>.Success(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute manager team performance report query for ManagerId: {ManagerId}", query.ManagerId);
            return Result<ReportQueries.TeamPerformanceReport>.Fail($"Failed to retrieve team performance report: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}