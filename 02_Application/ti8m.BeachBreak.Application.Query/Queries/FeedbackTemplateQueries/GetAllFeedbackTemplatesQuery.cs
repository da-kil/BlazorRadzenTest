using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Queries.FeedbackTemplateQueries;

/// <summary>
/// Query to retrieve all non-deleted feedback templates.
/// Returns all templates regardless of status (Draft, Published, Archived).
/// </summary>
public record GetAllFeedbackTemplatesQuery() : IQuery<List<FeedbackTemplateReadModel>>;
