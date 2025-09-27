namespace ti8m.BeachBreak.Application.Query.Queries.OrganizationQueries;

public class OrganizationByNumberQuery : IQuery<Result<Organization?>>
{
    public string Number { get; set; }

    public OrganizationByNumberQuery(string number)
    {
        Number = number;
    }
}