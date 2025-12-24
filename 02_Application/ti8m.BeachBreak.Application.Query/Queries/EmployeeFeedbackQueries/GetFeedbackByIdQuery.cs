using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Core;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Query to get a single feedback record by its ID.
/// Includes authorization check to ensure user has access to the feedback.
/// </summary>
public class GetFeedbackByIdQuery : IQuery<Result<EmployeeFeedbackReadModel>>
{
    /// <summary>
    /// ID of the feedback to retrieve.
    /// </summary>
    public Guid FeedbackId { get; set; }

    /// <summary>
    /// Whether to include deleted feedback (default: false).
    /// Only HR+ roles should have access to deleted feedback.
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    public GetFeedbackByIdQuery() { }

    public GetFeedbackByIdQuery(Guid feedbackId)
    {
        FeedbackId = feedbackId;
    }
}