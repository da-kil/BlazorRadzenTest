using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.FeedbackTemplateQueries;

/// <summary>
/// Query to retrieve feedback templates filtered by source type.
/// Returns templates where AllowedSourceTypes contains the specified source type.
/// </summary>
public record GetFeedbackTemplatesBySourceTypeQuery(FeedbackSourceType SourceType) : IQuery<List<FeedbackTemplateReadModel>>;
