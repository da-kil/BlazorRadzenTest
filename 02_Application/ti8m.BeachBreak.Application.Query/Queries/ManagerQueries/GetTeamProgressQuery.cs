using ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamProgressQuery : IQuery<Result<IEnumerable<AssignmentProgress>>>
{
    public Guid ManagerId { get; }

    public GetTeamProgressQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}
