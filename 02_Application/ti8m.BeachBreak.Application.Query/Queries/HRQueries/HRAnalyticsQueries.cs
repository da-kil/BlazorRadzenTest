namespace ti8m.BeachBreak.Application.Query.Queries.HRQueries;

public class HROrganizationAnalyticsQuery : IQuery<Result<AnalyticsQueries.OrganizationAnalytics>>
{
    public int OrganizationNumber { get; set; }

    public HROrganizationAnalyticsQuery(int organizationNumber)
    {
        OrganizationNumber = organizationNumber;
    }
}

public class HRDepartmentAnalyticsQuery : IQuery<Result<DepartmentAnalytics>>
{
    public string DepartmentName { get; set; }

    public HRDepartmentAnalyticsQuery(string departmentName)
    {
        DepartmentName = departmentName;
    }
}

public class HRComplianceReportQuery : IQuery<Result<ComplianceReport>>
{
    public int OrganizationNumber { get; set; }

    public HRComplianceReportQuery(int organizationNumber)
    {
        OrganizationNumber = organizationNumber;
    }
}

public class HROrganizationReportQuery : IQuery<Result<OrganizationReport>>
{
    public int OrganizationNumber { get; set; }
    public string ReportPeriod { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Department { get; set; }
    public string? TemplateId { get; set; }

    public HROrganizationReportQuery(int organizationNumber, string reportPeriod, DateTime? fromDate = null, DateTime? toDate = null, string? department = null, string? templateId = null)
    {
        OrganizationNumber = organizationNumber;
        ReportPeriod = reportPeriod;
        FromDate = fromDate;
        ToDate = toDate;
        Department = department;
        TemplateId = templateId;
    }
}

public class HRQuestionnaireUsageStatsQuery : IQuery<Result<IEnumerable<QuestionnaireUsageStats>>>
{
    public int OrganizationNumber { get; set; }

    public HRQuestionnaireUsageStatsQuery(int organizationNumber)
    {
        OrganizationNumber = organizationNumber;
    }
}

public class HRAnalyticsTrendsQuery : IQuery<Result<IEnumerable<TrendData>>>
{
    public int Days { get; set; }

    public HRAnalyticsTrendsQuery(int days = 30)
    {
        Days = days;
    }
}