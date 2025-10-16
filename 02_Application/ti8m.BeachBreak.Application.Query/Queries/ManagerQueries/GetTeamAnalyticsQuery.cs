namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamAnalyticsQuery : IQuery<Result<TeamAnalytics>>
{
    public Guid ManagerId { get; }

    public GetTeamAnalyticsQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}
