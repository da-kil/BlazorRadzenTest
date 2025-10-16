using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamMembersQuery : IQuery<Result<IEnumerable<Employee>>>
{
    public Guid ManagerId { get; }

    public GetTeamMembersQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}
