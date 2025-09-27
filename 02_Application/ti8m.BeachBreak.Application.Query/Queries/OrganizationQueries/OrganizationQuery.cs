namespace ti8m.BeachBreak.Application.Query.Queries.OrganizationQueries;

public class OrganizationQuery : IQuery<Result<Organization?>>
{
    public Guid Id { get; set; }

    public OrganizationQuery(Guid id)
    {
        Id = id;
    }
}