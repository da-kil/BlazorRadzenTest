namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerTeamProgressQuery : IQuery<Result<IEnumerable<ProgressQueries.AssignmentProgress>>>
{
    public Guid ManagerId { get; set; }

    public ManagerTeamProgressQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}