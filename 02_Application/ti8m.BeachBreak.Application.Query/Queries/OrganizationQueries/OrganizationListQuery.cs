namespace ti8m.BeachBreak.Application.Query.Queries.OrganizationQueries;

public class OrganizationListQuery : IQuery<Result<IEnumerable<Organization>>>
{
    public bool IncludeDeleted { get; set; } = false;
    public bool IncludeIgnored { get; set; } = false;
    public Guid? ParentId { get; set; }
    public string? ManagerId { get; set; }
}