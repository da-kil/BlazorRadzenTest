namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerDashboardQuery : IQuery<Result<ManagerDashboard?>>
{
    public Guid ManagerId { get; init; }

    public ManagerDashboardQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}
