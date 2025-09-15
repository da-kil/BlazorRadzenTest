namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeListQuery : IQuery<Result<IEnumerable<Employee>>>
{
    public bool IncludeDeleted { get; set; } = false;
    public int? OrganizationNumber { get; set; }
    public string? Role { get; set; }
    public Guid? ManagerId { get; set; }
}