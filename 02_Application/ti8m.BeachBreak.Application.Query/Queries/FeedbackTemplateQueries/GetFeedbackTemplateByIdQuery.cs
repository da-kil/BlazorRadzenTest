using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Queries.FeedbackTemplateQueries;

/// <summary>
/// Query to retrieve a single feedback template by ID.
/// Returns null if template not found or is deleted.
/// </summary>
public record GetFeedbackTemplateByIdQuery(Guid TemplateId) : IQuery<FeedbackTemplateReadModel?>;
