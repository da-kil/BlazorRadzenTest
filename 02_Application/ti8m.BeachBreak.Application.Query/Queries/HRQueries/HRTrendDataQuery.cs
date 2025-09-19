namespace ti8m.BeachBreak.Application.Query.Queries.HRQueries;

public class HRTrendDataQuery : IQuery<Result<IEnumerable<TrendData>>>
{
    public int OrganizationNumber { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public HRTrendDataQuery(int organizationNumber, DateOnly startDate, DateOnly endDate)
    {
        OrganizationNumber = organizationNumber;
        StartDate = startDate;
        EndDate = endDate;
    }
}