namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerTeamListQuery : IQuery<Result<IEnumerable<EmployeeQueries.Employee>>>
{
    public Guid ManagerId { get; set; }

    public ManagerTeamListQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}