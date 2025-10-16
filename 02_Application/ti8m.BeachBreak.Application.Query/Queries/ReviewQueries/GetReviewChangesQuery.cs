namespace ti8m.BeachBreak.Application.Query.Queries.ReviewQueries;

/// <summary>
/// Query to retrieve all changes made during a review meeting for a specific assignment.
/// Returns a list of ReviewChangeLogReadModel entries showing what was edited,
/// by whom, and when during the review meeting.
/// </summary>
public record GetReviewChangesQuery(Guid AssignmentId) : IQuery<List<Projections.ReviewChangeLogReadModel>>;
