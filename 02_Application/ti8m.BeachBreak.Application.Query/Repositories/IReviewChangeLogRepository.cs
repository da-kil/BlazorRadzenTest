using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

/// <summary>
/// Repository for querying ReviewChangeLog read models.
/// Provides access to review change logs generated during manager review meetings.
/// </summary>
public interface IReviewChangeLogRepository : IRepository
{
    /// <summary>
    /// Gets all review changes for a specific questionnaire assignment, ordered by change timestamp.
    /// </summary>
    Task<List<ReviewChangeLogReadModel>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}