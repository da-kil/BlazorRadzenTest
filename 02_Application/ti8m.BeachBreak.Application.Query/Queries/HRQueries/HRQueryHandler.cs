using Npgsql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Application.Query.Queries.HRQueries;

// CONSOLIDATED: This handler now supports all roles through dynamic filtering
public class UnifiedAnalyticsQueryHandler :
    IQueryHandler<HROrganizationAnalyticsQuery, Result<AnalyticsQueries.OrganizationAnalytics>>,
    IQueryHandler<HRDepartmentAnalyticsQuery, Result<DepartmentAnalytics>>,
    IQueryHandler<HRComplianceReportQuery, Result<ComplianceReport>>,
    IQueryHandler<HROrganizationReportQuery, Result<OrganizationReport>>,
    IQueryHandler<HRQuestionnaireUsageStatsQuery, Result<IEnumerable<QuestionnaireUsageStats>>>,
    IQueryHandler<HRTrendDataQuery, Result<IEnumerable<TrendData>>>,
    // NEW: Supporting Manager queries through dynamic filtering
    IQueryHandler<ManagerQueries.ManagerTeamAnalyticsQuery, Result<AnalyticsQueries.TeamAnalytics>>,
    IQueryHandler<ManagerQueries.ManagerTeamAssignmentsQuery, Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>,
    // NEW: Supporting Employee queries through dynamic filtering
    IQueryHandler<EmployeeQueries.EmployeeAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>,
    IQueryHandler<EmployeeQueries.EmployeeListQuery, Result<IEnumerable<EmployeeQueries.Employee>>>
{
    private readonly NpgsqlDataSource dataSource;
    private readonly ILogger<UnifiedAnalyticsQueryHandler> logger;

    public UnifiedAnalyticsQueryHandler(NpgsqlDataSource dataSource, ILogger<UnifiedAnalyticsQueryHandler> logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task<Result<AnalyticsQueries.OrganizationAnalytics>> HandleAsync(HROrganizationAnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting HR organization analytics query for OrganizationNumber: {OrganizationNumber}", query.OrganizationNumber);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            // Set timeout to prevent cancellation issues
            cmd.CommandTimeout = 120;

            cmd.CommandText = """
                SELECT
                    COUNT(DISTINCT e.id) as total_employees,
                    COUNT(CASE WHEN a.status = 'Assigned' OR a.status = 'InProgress' THEN 1 END) as total_active_assignments,
                    COUNT(CASE WHEN a.status = 'Completed' THEN 1 END) as total_completed_assignments,
                    COUNT(CASE WHEN a.due_date < CURRENT_DATE AND a.status != 'Completed' THEN 1 END) as total_overdue_assignments,
                    CASE
                        WHEN COUNT(a.id) > 0
                        THEN ROUND((COUNT(CASE WHEN a.status = 'Completed' THEN 1 END)::decimal / COUNT(a.id) * 100), 2)
                        ELSE 0
                    END as overall_completion_rate,
                    AVG(EXTRACT(EPOCH FROM (a.completed_date - a.assigned_date))/86400) as avg_completion_days
                FROM employees e
                LEFT JOIN questionnaire_assignments a ON e.id = a.employee_id
                WHERE e.organization_number = @organizationNumber AND e.is_deleted = false
                """;

            cmd.Parameters.AddWithValue("@organizationNumber", query.OrganizationNumber);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var analytics = new AnalyticsQueries.OrganizationAnalytics
                {
                    TotalEmployees = reader.GetInt32(0),
                    TotalActiveAssignments = reader.GetInt32(1),
                    TotalCompletedAssignments = reader.GetInt32(2),
                    TotalOverdueAssignments = reader.GetInt32(3),
                    OverallCompletionRate = reader.GetDecimal(4),
                    AverageCompletionTime = reader.IsDBNull(5) ? null : TimeSpan.FromDays(reader.GetDouble(5)),
                    DepartmentBreakdown = new Dictionary<string, AnalyticsQueries.DepartmentMetrics>(),
                    MonthlyTrends = new List<AnalyticsQueries.MonthlyTrend>()
                };

                logger.LogInformation("HR organization analytics query completed successfully");
                return Result<AnalyticsQueries.OrganizationAnalytics>.Success(analytics);
            }

            return Result<AnalyticsQueries.OrganizationAnalytics>.Fail("No analytics data found", StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute HR organization analytics query for OrganizationNumber: {OrganizationNumber}", query.OrganizationNumber);
            return Result<AnalyticsQueries.OrganizationAnalytics>.Fail($"Failed to retrieve organization analytics: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<DepartmentAnalytics>> HandleAsync(HRDepartmentAnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting HR department analytics query for Department: {DepartmentName}", query.DepartmentName);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            // Set timeout to prevent cancellation issues
            cmd.CommandTimeout = 120;

            cmd.CommandText = """
                SELECT
                    @departmentName as department_name,
                    COUNT(DISTINCT e.id) as total_employees,
                    COUNT(CASE WHEN a.status = 'Assigned' OR a.status = 'InProgress' THEN 1 END) as active_assignments,
                    COUNT(CASE WHEN a.status = 'Completed' THEN 1 END) as completed_assignments,
                    COUNT(CASE WHEN a.due_date < CURRENT_DATE AND a.status != 'Completed' THEN 1 END) as overdue_assignments,
                    CASE
                        WHEN COUNT(a.id) > 0
                        THEN ROUND((COUNT(CASE WHEN a.status = 'Completed' THEN 1 END)::decimal / COUNT(a.id) * 100), 2)
                        ELSE 0
                    END as completion_rate,
                    AVG(EXTRACT(EPOCH FROM (a.completed_date - a.assigned_date))/86400) as avg_completion_days
                FROM employees e
                LEFT JOIN questionnaire_assignments a ON e.id = a.employee_id
                WHERE e.role = @departmentName AND e.is_deleted = false
                """;

            cmd.Parameters.AddWithValue("@departmentName", query.DepartmentName);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var analytics = new DepartmentAnalytics
                {
                    DepartmentName = reader.GetString(0),
                    TotalEmployees = reader.GetInt32(1),
                    ActiveAssignments = reader.GetInt32(2),
                    CompletedAssignments = reader.GetInt32(3),
                    OverdueAssignments = reader.GetInt32(4),
                    CompletionRate = reader.GetDecimal(5),
                    AverageCompletionTime = reader.IsDBNull(6) ? null : TimeSpan.FromDays(reader.GetDouble(6)),
                    PerformanceMetrics = new Dictionary<string, object>()
                };

                logger.LogInformation("HR department analytics query completed successfully");
                return Result<DepartmentAnalytics>.Success(analytics);
            }

            return Result<DepartmentAnalytics>.Fail("No analytics data found", StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute HR department analytics query for Department: {DepartmentName}", query.DepartmentName);
            return Result<DepartmentAnalytics>.Fail($"Failed to retrieve department analytics: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<ComplianceReport>> HandleAsync(HRComplianceReportQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting HR compliance report query for OrganizationNumber: {OrganizationNumber}", query.OrganizationNumber);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            // Set timeout to prevent cancellation issues
            cmd.CommandTimeout = 120;

            cmd.CommandText = """
                SELECT
                    COUNT(CASE WHEN a.template_required = true THEN 1 END) as total_required_assignments,
                    COUNT(CASE WHEN a.template_required = true AND a.status = 'Completed' THEN 1 END) as completed_required_assignments,
                    COUNT(CASE WHEN a.template_required = true AND a.due_date < CURRENT_DATE AND a.status != 'Completed' THEN 1 END) as overdue_required_assignments,
                    CASE
                        WHEN COUNT(CASE WHEN a.template_required = true THEN 1 END) > 0
                        THEN ROUND((COUNT(CASE WHEN a.template_required = true AND a.status = 'Completed' THEN 1 END)::decimal / COUNT(CASE WHEN a.template_required = true THEN 1 END) * 100), 2)
                        ELSE 100
                    END as compliance_score
                FROM employees e
                LEFT JOIN questionnaire_assignments a ON e.id = a.employee_id
                LEFT JOIN questionnaire_templates t ON a.template_id = t.id
                WHERE e.organization_number = @organizationNumber AND e.is_deleted = false
                """;

            cmd.Parameters.AddWithValue("@organizationNumber", query.OrganizationNumber);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var report = new ComplianceReport
                {
                    ComplianceScore = reader.GetDecimal(3),
                    TotalRequiredAssignments = reader.GetInt32(0),
                    CompletedRequiredAssignments = reader.GetInt32(1),
                    OverdueRequiredAssignments = reader.GetInt32(2),
                    NonCompliantEmployees = new List<NonCompliantEmployee>(),
                    DepartmentCompliance = new Dictionary<string, decimal>(),
                    ReportGeneratedDate = DateTime.UtcNow
                };

                logger.LogInformation("HR compliance report query completed successfully");
                return Result<ComplianceReport>.Success(report);
            }

            return Result<ComplianceReport>.Fail("No compliance data found", StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute HR compliance report query for OrganizationNumber: {OrganizationNumber}", query.OrganizationNumber);
            return Result<ComplianceReport>.Fail($"Failed to retrieve compliance report: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<OrganizationReport>> HandleAsync(HROrganizationReportQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting HR organization report query for OrganizationNumber: {OrganizationNumber}, Period: {ReportPeriod}", query.OrganizationNumber, query.ReportPeriod);

        try
        {
            var report = new OrganizationReport
            {
                ReportPeriod = query.ReportPeriod,
                ExecutiveSummary = $"Organization report for period {query.ReportPeriod}",
                DepartmentPerformance = new Dictionary<string, object>(),
                OverallMetrics = new Dictionary<string, object>(),
                TrendAnalysis = new Dictionary<string, object>(),
                Recommendations = new List<string>(),
                DetailedBreakdown = new Dictionary<string, object>()
            };

            logger.LogInformation("HR organization report query completed successfully");
            return Result<OrganizationReport>.Success(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute HR organization report query for OrganizationNumber: {OrganizationNumber}", query.OrganizationNumber);
            return Result<OrganizationReport>.Fail($"Failed to retrieve organization report: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireUsageStats>>> HandleAsync(HRQuestionnaireUsageStatsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting HR questionnaire usage stats query for OrganizationNumber: {OrganizationNumber}", query.OrganizationNumber);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            // Set timeout to prevent cancellation issues
            cmd.CommandTimeout = 120;

            cmd.CommandText = """
                SELECT
                    t.id as template_id,
                    t.name as template_name,
                    COUNT(a.id) as total_assignments,
                    COUNT(CASE WHEN a.status = 'Completed' THEN 1 END) as completed_assignments,
                    AVG(EXTRACT(EPOCH FROM (a.completed_date - a.assigned_date))/86400) as avg_completion_days,
                    CASE
                        WHEN COUNT(a.id) > 0
                        THEN ROUND((COUNT(CASE WHEN a.status = 'Completed' THEN 1 END)::decimal / COUNT(a.id) * 100), 2)
                        ELSE 0
                    END as completion_rate,
                    MAX(a.assigned_date) as last_used_date,
                    COUNT(a.id) * 0.5 + COUNT(CASE WHEN a.status = 'Completed' THEN 1 END) * 0.5 as popularity_score
                FROM questionnaire_templates t
                LEFT JOIN questionnaire_assignments a ON t.id = a.template_id
                LEFT JOIN employees e ON a.employee_id = e.id
                WHERE e.organization_number = @organizationNumber OR e.organization_number IS NULL
                GROUP BY t.id, t.name
                ORDER BY popularity_score DESC
                """;

            cmd.Parameters.AddWithValue("@organizationNumber", query.OrganizationNumber);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var stats = new List<QuestionnaireUsageStats>();

            while (await reader.ReadAsync(cancellationToken))
            {
                stats.Add(new QuestionnaireUsageStats
                {
                    TemplateId = reader.GetGuid(0),
                    TemplateName = reader.GetString(1),
                    TotalAssignments = reader.GetInt32(2),
                    CompletedAssignments = reader.GetInt32(3),
                    AverageCompletionTime = reader.IsDBNull(4) ? null : TimeSpan.FromDays(reader.GetDouble(4)),
                    CompletionRate = reader.GetDecimal(5),
                    LastUsedDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    PopularityScore = reader.GetDecimal(7)
                });
            }

            logger.LogInformation("HR questionnaire usage stats query completed successfully, returned {StatsCount} stats", stats.Count);
            return Result<IEnumerable<QuestionnaireUsageStats>>.Success(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute HR questionnaire usage stats query for OrganizationNumber: {OrganizationNumber}", query.OrganizationNumber);
            return Result<IEnumerable<QuestionnaireUsageStats>>.Fail($"Failed to retrieve questionnaire usage stats: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<TrendData>>> HandleAsync(HRTrendDataQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting HR trend data query for OrganizationNumber: {OrganizationNumber}, StartDate: {StartDate}, EndDate: {EndDate}", query.OrganizationNumber, query.StartDate, query.EndDate);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            // Set timeout to prevent cancellation issues
            cmd.CommandTimeout = 120;

            cmd.CommandText = """
                SELECT
                    DATE(a.assigned_date) as date,
                    CASE
                        WHEN COUNT(a.id) > 0
                        THEN ROUND((COUNT(CASE WHEN a.status = 'Completed' THEN 1 END)::decimal / COUNT(a.id) * 100), 2)
                        ELSE 0
                    END as completion_rate,
                    COUNT(a.id) as new_assignments,
                    COUNT(CASE WHEN a.status = 'Completed' THEN 1 END) as completed_assignments,
                    COUNT(CASE WHEN a.due_date < CURRENT_DATE AND a.status != 'Completed' THEN 1 END) as overdue_assignments,
                    AVG(EXTRACT(EPOCH FROM (a.completed_date - a.assigned_date))/3600) as avg_response_hours
                FROM questionnaire_assignments a
                INNER JOIN employees e ON a.employee_id = e.id
                WHERE e.organization_number = @organizationNumber
                    AND DATE(a.assigned_date) >= @startDate
                    AND DATE(a.assigned_date) <= @endDate
                GROUP BY DATE(a.assigned_date)
                ORDER BY DATE(a.assigned_date)
                """;

            cmd.Parameters.AddWithValue("@organizationNumber", query.OrganizationNumber);
            cmd.Parameters.AddWithValue("@startDate", query.StartDate);
            cmd.Parameters.AddWithValue("@endDate", query.EndDate);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var trends = new List<TrendData>();

            while (await reader.ReadAsync(cancellationToken))
            {
                trends.Add(new TrendData
                {
                    Date = DateOnly.FromDateTime(reader.GetDateTime(0)),
                    CompletionRate = reader.GetDecimal(1),
                    NewAssignments = reader.GetInt32(2),
                    CompletedAssignments = reader.GetInt32(3),
                    OverdueAssignments = reader.GetInt32(4),
                    AverageResponseTime = reader.IsDBNull(5) ? null : TimeSpan.FromHours(reader.GetDouble(5))
                });
            }

            logger.LogInformation("HR trend data query completed successfully, returned {TrendCount} trend records", trends.Count);
            return Result<IEnumerable<TrendData>>.Success(trends);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute HR trend data query for OrganizationNumber: {OrganizationNumber}", query.OrganizationNumber);
            return Result<IEnumerable<TrendData>>.Fail($"Failed to retrieve trend data: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    // CONSOLIDATED: Manager queries now handled through dynamic filtering
    public async Task<Result<AnalyticsQueries.TeamAnalytics>> HandleAsync(ManagerQueries.ManagerTeamAnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting team analytics query (unified) for ManagerId: {ManagerId}", query.ManagerId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            // Set timeout to prevent cancellation issues
            cmd.CommandTimeout = 120;

            // SAME SQL as HRQueryHandler but with manager-specific filtering
            cmd.CommandText = """
                SELECT
                    COUNT(DISTINCT e.id) as total_team_members,
                    COUNT(CASE WHEN a.status = 'Assigned' OR a.status = 'InProgress' THEN 1 END) as active_assignments,
                    COUNT(CASE WHEN a.status = 'Completed' THEN 1 END) as completed_assignments,
                    COUNT(CASE WHEN a.due_date < CURRENT_DATE AND a.status != 'Completed' THEN 1 END) as overdue_assignments,
                    CASE
                        WHEN COUNT(a.id) > 0
                        THEN ROUND((COUNT(CASE WHEN a.status = 'Completed' THEN 1 END)::decimal / COUNT(a.id) * 100), 2)
                        ELSE 0
                    END as team_completion_rate
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
                    CompletionRate = reader.GetDecimal(4)
                };

                logger.LogInformation("Team analytics query (unified) completed successfully");
                return Result<AnalyticsQueries.TeamAnalytics>.Success(analytics);
            }

            return Result<AnalyticsQueries.TeamAnalytics>.Fail("No team analytics data found", StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute team analytics query (unified) for ManagerId: {ManagerId}", query.ManagerId);
            return Result<AnalyticsQueries.TeamAnalytics>.Fail($"Failed to retrieve team analytics: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    // CONSOLIDATED: Manager assignment queries now handled through dynamic filtering
    public async Task<Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>> HandleAsync(ManagerQueries.ManagerTeamAssignmentsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting team assignments query (unified) for ManagerId: {ManagerId}", query.ManagerId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            // Set timeout to prevent cancellation issues
            cmd.CommandTimeout = 120;

            // SAME SQL pattern as other handlers but with manager-specific filtering
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
                    AssignedBy = reader.GetString(9),
                    Notes = reader.IsDBNull(10) ? null : reader.GetString(10)
                });
            }

            logger.LogInformation("Team assignments query (unified) completed successfully, returned {AssignmentCount} assignments", assignments.Count);
            return Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute team assignments query (unified) for ManagerId: {ManagerId}", query.ManagerId);
            return Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>.Fail($"Failed to retrieve team assignments: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    // CONSOLIDATED: Employee queries now handled through dynamic filtering
    public async Task<Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>> HandleAsync(EmployeeQueries.EmployeeAssignmentListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting employee assignment query (unified) for EmployeeId: {EmployeeId}", query.EmployeeId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            // Set timeout to prevent cancellation issues
            cmd.CommandTimeout = 120;

            // SAME SQL pattern as other handlers but with employee-specific filtering
            cmd.CommandText = """
                SELECT a.id, a.template_id, a.employee_id, a.employee_name, a.employee_email,
                       a.assigned_date, a.due_date, a.completed_date, a.status, a.assigned_by, a.notes
                FROM questionnaire_assignments a
                WHERE a.employee_id = @employeeId
                ORDER BY a.assigned_date DESC
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
                    AssignedBy = reader.GetString(9),
                    Notes = reader.IsDBNull(10) ? null : reader.GetString(10)
                });
            }

            logger.LogInformation("Employee assignment query (unified) completed successfully, returned {AssignmentCount} assignments", assignments.Count);
            return Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute employee assignment query (unified) for EmployeeId: {EmployeeId}", query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>.Fail($"Failed to retrieve employee assignments: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    // CONSOLIDATED: Employee list queries now handled through dynamic filtering
    public async Task<Result<IEnumerable<EmployeeQueries.Employee>>> HandleAsync(EmployeeQueries.EmployeeListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting employee list query (unified) with filters: IncludeDeleted={IncludeDeleted}, OrganizationNumber={OrganizationNumber}, Role={Role}, ManagerId={ManagerId}",
            query.IncludeDeleted, query.OrganizationNumber, query.Role, query.ManagerId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            // Set timeout to prevent cancellation issues
            cmd.CommandTimeout = 120;

            // Build dynamic SQL based on query filters
            var whereConditions = new List<string>();
            var parameterIndex = 1;

            if (!query.IncludeDeleted)
            {
                whereConditions.Add("e.is_deleted = false");
            }

            if (query.OrganizationNumber.HasValue)
            {
                whereConditions.Add($"e.organization_number = @param{parameterIndex}");
                cmd.Parameters.AddWithValue($"@param{parameterIndex}", query.OrganizationNumber.Value);
                parameterIndex++;
            }

            if (!string.IsNullOrEmpty(query.Role))
            {
                whereConditions.Add($"e.role = @param{parameterIndex}");
                cmd.Parameters.AddWithValue($"@param{parameterIndex}", query.Role);
                parameterIndex++;
            }

            if (query.ManagerId.HasValue)
            {
                whereConditions.Add($"e.manager_id = @param{parameterIndex}");
                cmd.Parameters.AddWithValue($"@param{parameterIndex}", query.ManagerId.Value);
                parameterIndex++;
            }

            var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

            cmd.CommandText = $"""
                SELECT e.id, e.first_name, e.last_name, e.role, e.email, e.start_date, e.end_date,
                       e.last_start_date, e.manager_id,
                       COALESCE(m.first_name || ' ' || m.last_name, '') as manager_name,
                       e.login_name, e.employee_number, e.organization_number, e.organization, e.is_deleted
                FROM employees e
                LEFT JOIN employees m ON e.manager_id = m.id
                {whereClause}
                ORDER BY e.first_name, e.last_name
                """;

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

            logger.LogInformation("Employee list query (unified) completed successfully, returned {EmployeeCount} employees", employees.Count);
            return Result<IEnumerable<EmployeeQueries.Employee>>.Success(employees);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute employee list query (unified)");
            return Result<IEnumerable<EmployeeQueries.Employee>>.Fail($"Failed to retrieve employees: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}