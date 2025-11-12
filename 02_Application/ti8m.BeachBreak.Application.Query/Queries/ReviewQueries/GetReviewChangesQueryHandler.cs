using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.ReviewQueries;

/// <summary>
/// Query handler to retrieve all review changes for a specific questionnaire assignment.
/// Queries the ReviewChangeLog projection to return all edits made by the manager during the review meeting.
/// Note: Employee names are resolved in the controller/presentation layer, not here.
/// </summary>
public class GetReviewChangesQueryHandler : IQueryHandler<GetReviewChangesQuery, List<ReviewChangeLogReadModel>>
{
    private readonly IReviewChangeLogRepository reviewChangeLogRepository;

    public GetReviewChangesQueryHandler(IReviewChangeLogRepository reviewChangeLogRepository)
    {
        this.reviewChangeLogRepository = reviewChangeLogRepository;
    }

    public async Task<List<ReviewChangeLogReadModel>> HandleAsync(GetReviewChangesQuery query, CancellationToken cancellationToken = default)
    {
        return await reviewChangeLogRepository.GetByAssignmentIdAsync(query.AssignmentId, cancellationToken);
    }
}
