namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerTeamPerformanceReportQuery : IQuery<Result<ReportQueries.TeamPerformanceReport>>
{
    public Guid ManagerId { get; set; }
    public string ReportPeriod { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? TemplateId { get; set; }

    public ManagerTeamPerformanceReportQuery(Guid managerId, string reportPeriod, DateTime? fromDate = null, DateTime? toDate = null, string? templateId = null)
    {
        ManagerId = managerId;
        ReportPeriod = reportPeriod;
        FromDate = fromDate;
        ToDate = toDate;
        TemplateId = templateId;
    }
}