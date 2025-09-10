namespace ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;

public class TemplateAnalyticsListQuery : IQuery<Result<Dictionary<string, object>>>
{
    public Guid TemplateId { get; init; }

    public TemplateAnalyticsListQuery(Guid templateId)
    {
        TemplateId = templateId;
    }
}
