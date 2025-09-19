using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.HRQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/hr")]
public class HRController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<HRController> logger;

    public HRController(
        IQueryDispatcher queryDispatcher,
        ILogger<HRController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    // HR Employee Management
    [HttpGet("employees")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEmployees()
    {
        logger.LogInformation("Received HR GetAllEmployees request");

        try
        {
            var result = await queryDispatcher.QueryAsync(new HREmployeeListQuery());

            return CreateResponse(result, employees =>
            {
                return employees.Select(employee => new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Role = employee.Role,
                    EMail = employee.EMail,
                    StartDate = employee.StartDate,
                    EndDate = employee.EndDate,
                    LastStartDate = employee.LastStartDate,
                    ManagerId = employee.ManagerId,
                    Manager = employee.Manager,
                    LoginName = employee.LoginName,
                    EmployeeNumber = employee.EmployeeNumber,
                    OrganizationNumber = employee.OrganizationNumber,
                    Organization = employee.Organization,
                    IsDeleted = employee.IsDeleted
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all employees for HR");
            return StatusCode(500, "An error occurred while retrieving employees");
        }
    }

    // HR Assignment Management
    [HttpGet("assignments")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAssignments()
    {
        logger.LogInformation("Received HR GetAllAssignments request");

        try
        {
            var result = await queryDispatcher.QueryAsync(new HRAssignmentListQuery());

            return CreateResponse(result, assignments =>
            {
                return assignments.Select(assignment => new QuestionnaireAssignmentDto
                {
                    Id = assignment.Id,
                    TemplateId = assignment.TemplateId,
                    EmployeeId = assignment.EmployeeId,
                    EmployeeName = assignment.EmployeeName,
                    EmployeeEmail = assignment.EmployeeEmail,
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    CompletedDate = assignment.CompletedDate,
                    Status = MapAssignmentStatus(assignment.Status),
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all assignments for HR");
            return StatusCode(500, "An error occurred while retrieving assignments");
        }
    }

    [HttpGet("assignments/department/{department}")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignmentsByDepartment(string department)
    {
        logger.LogInformation("Received HR GetAssignmentsByDepartment request for Department: {Department}", department);

        try
        {
            var result = await queryDispatcher.QueryAsync(new HRDepartmentAssignmentListQuery(department));

            return CreateResponse(result, assignments =>
            {
                return assignments.Select(assignment => new QuestionnaireAssignmentDto
                {
                    Id = assignment.Id,
                    TemplateId = assignment.TemplateId,
                    EmployeeId = assignment.EmployeeId,
                    EmployeeName = assignment.EmployeeName,
                    EmployeeEmail = assignment.EmployeeEmail,
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    CompletedDate = assignment.CompletedDate,
                    Status = MapAssignmentStatus(assignment.Status),
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments by department {Department} for HR", department);
            return StatusCode(500, "An error occurred while retrieving department assignments");
        }
    }

    // HR Analytics
    [HttpGet("analytics/organization")]
    [ProducesResponseType(typeof(OrganizationAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationAnalytics()
    {
        logger.LogInformation("Received HR GetOrganizationAnalytics request");

        try
        {
            var result = await queryDispatcher.QueryAsync(new HROrganizationAnalyticsQuery(1));

            return CreateResponse(result, analytics => new OrganizationAnalyticsDto
            {
                TotalEmployees = analytics.TotalEmployees,
                TotalActiveAssignments = analytics.TotalActiveAssignments,
                TotalCompletedAssignments = analytics.TotalCompletedAssignments,
                TotalOverdueAssignments = analytics.TotalOverdueAssignments,
                OverallCompletionRate = analytics.OverallCompletionRate,
                AverageCompletionTime = analytics.AverageCompletionTime,
                DepartmentBreakdown = analytics.DepartmentBreakdown?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new DepartmentMetricsDto
                    {
                        DepartmentName = kvp.Key,
                        EmployeeCount = kvp.Value.EmployeeCount,
                        AssignmentCount = kvp.Value.AssignmentCount,
                        CompletionRate = kvp.Value.CompletionRate
                    }) ?? new Dictionary<string, DepartmentMetricsDto>(),
                MonthlyTrends = analytics.MonthlyTrends?.Select(trend => new MonthlyTrendDto
                {
                    Month = trend.Month,
                    AssignmentsCreated = trend.AssignmentsCreated,
                    AssignmentsCompleted = trend.AssignmentsCompleted,
                    CompletionRate = trend.CompletionRate
                }).ToList() ?? new List<MonthlyTrendDto>()
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving organization analytics for HR");
            return StatusCode(500, "An error occurred while retrieving organization analytics");
        }
    }

    [HttpGet("analytics/departments")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentAnalytics()
    {
        logger.LogInformation("Received HR GetDepartmentAnalytics request");

        try
        {
            var result = await queryDispatcher.QueryAsync(new HRDepartmentAnalyticsQuery(""));

            return CreateResponse(result, department => new List<DepartmentAnalyticsDto>
            {
                new DepartmentAnalyticsDto
                {
                    DepartmentName = department.DepartmentName,
                    TotalEmployees = department.TotalEmployees,
                    ActiveAssignments = department.ActiveAssignments,
                    CompletedAssignments = department.CompletedAssignments,
                    OverdueAssignments = department.OverdueAssignments,
                    CompletionRate = department.CompletionRate,
                    AverageCompletionTime = department.AverageCompletionTime,
                    PerformanceMetrics = department.PerformanceMetrics
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving department analytics for HR");
            return StatusCode(500, "An error occurred while retrieving department analytics");
        }
    }

    [HttpGet("compliance")]
    [ProducesResponseType(typeof(ComplianceReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplianceReport()
    {
        logger.LogInformation("Received HR GetComplianceReport request");

        try
        {
            var result = await queryDispatcher.QueryAsync(new HRComplianceReportQuery(1));

            return CreateResponse(result, report => new ComplianceReportDto
            {
                ComplianceScore = report.ComplianceScore,
                TotalRequiredAssignments = report.TotalRequiredAssignments,
                CompletedRequiredAssignments = report.CompletedRequiredAssignments,
                OverdueRequiredAssignments = report.OverdueRequiredAssignments,
                NonCompliantEmployees = report.NonCompliantEmployees?.Select(emp => new NonCompliantEmployeeDto
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeName = emp.EmployeeName,
                    Department = emp.Department,
                    OverdueCount = emp.OverdueCount
                }).ToList() ?? new List<NonCompliantEmployeeDto>(),
                DepartmentCompliance = report.DepartmentCompliance,
                ReportGeneratedDate = report.ReportGeneratedDate
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving compliance report for HR");
            return StatusCode(500, "An error occurred while retrieving compliance report");
        }
    }

    // HR Reporting
    [HttpGet("reports/organization")]
    [ProducesResponseType(typeof(OrganizationReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? department = null,
        [FromQuery] string? templateId = null)
    {
        logger.LogInformation("Received HR GetOrganizationReport request");

        try
        {
            var result = await queryDispatcher.QueryAsync(new HROrganizationReportQuery(1, "monthly", fromDate, toDate, department, templateId));

            return CreateResponse(result, report => new OrganizationReportDto
            {
                ReportPeriod = report.ReportPeriod,
                ExecutiveSummary = report.ExecutiveSummary,
                DepartmentPerformance = report.DepartmentPerformance,
                OverallMetrics = report.OverallMetrics,
                TrendAnalysis = report.TrendAnalysis,
                Recommendations = report.Recommendations,
                DetailedBreakdown = report.DetailedBreakdown
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving organization report for HR");
            return StatusCode(500, "An error occurred while retrieving organization report");
        }
    }

    [HttpGet("questionnaires/usage-stats")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireUsageStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionnaireUsageStats()
    {
        logger.LogInformation("Received HR GetQuestionnaireUsageStats request");

        try
        {
            var result = await queryDispatcher.QueryAsync(new HRQuestionnaireUsageStatsQuery(1));

            return CreateResponse(result, statsList =>
            {
                return statsList.Select(stats => new QuestionnaireUsageStatsDto
                {
                    TemplateId = stats.TemplateId,
                    TemplateName = stats.TemplateName,
                    TotalAssignments = stats.TotalAssignments,
                    CompletedAssignments = stats.CompletedAssignments,
                    AverageCompletionTime = stats.AverageCompletionTime,
                    CompletionRate = stats.CompletionRate,
                    LastUsedDate = stats.LastUsedDate,
                    PopularityScore = stats.PopularityScore
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving questionnaire usage stats for HR");
            return StatusCode(500, "An error occurred while retrieving usage statistics");
        }
    }

    [HttpGet("analytics/trends")]
    [ProducesResponseType(typeof(IEnumerable<TrendDataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalyticsTrends([FromQuery] int days = 30)
    {
        logger.LogInformation("Received HR GetAnalyticsTrends request for {Days} days", days);

        try
        {
            var result = await queryDispatcher.QueryAsync(new HRAnalyticsTrendsQuery(days));

            return CreateResponse(result, trendsList =>
            {
                return trendsList.Select(trend => new TrendDataDto
                {
                    Date = trend.Date.ToDateTime(TimeOnly.MinValue),
                    CompletionRate = trend.CompletionRate,
                    NewAssignments = trend.NewAssignments,
                    CompletedAssignments = trend.CompletedAssignments,
                    OverdueAssignments = trend.OverdueAssignments,
                    AverageResponseTime = trend.AverageResponseTime
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving analytics trends for HR");
            return StatusCode(500, "An error occurred while retrieving analytics trends");
        }
    }

    private static AssignmentStatus MapAssignmentStatus(Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus status)
    {
        return status switch
        {
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Assigned => AssignmentStatus.Assigned,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.InProgress => AssignmentStatus.InProgress,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Completed => AssignmentStatus.Completed,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Overdue => AssignmentStatus.Overdue,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Cancelled => AssignmentStatus.Cancelled,
            _ => AssignmentStatus.Assigned
        };
    }
}