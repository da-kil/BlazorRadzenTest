namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerTeamAnalyticsQuery : IQuery<Result<AnalyticsQueries.TeamAnalytics>>
{
    public Guid ManagerId { get; set; }

    public ManagerTeamAnalyticsQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}