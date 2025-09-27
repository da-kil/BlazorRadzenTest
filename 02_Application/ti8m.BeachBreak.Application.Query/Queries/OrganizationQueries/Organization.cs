namespace ti8m.BeachBreak.Application.Query.Queries.OrganizationQueries;

public class Organization
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? ManagerId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsIgnored { get; set; }
    public bool IsDeleted { get; set; }
}