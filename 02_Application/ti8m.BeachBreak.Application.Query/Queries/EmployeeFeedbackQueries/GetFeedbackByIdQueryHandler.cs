using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Handler for GetFeedbackByIdQuery that retrieves a specific feedback record by ID.
/// Returns detailed feedback information including provider context and evaluation data.
/// </summary>
public class GetFeedbackByIdQueryHandler : IQueryHandler<GetFeedbackByIdQuery, Result<EmployeeFeedbackReadModel>>
{
    private readonly IEmployeeFeedbackRepository repository;

    public GetFeedbackByIdQueryHandler(IEmployeeFeedbackRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Result<EmployeeFeedbackReadModel>> HandleAsync(GetFeedbackByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.FeedbackId == Guid.Empty)
            {
                return Result<EmployeeFeedbackReadModel>.Fail("FeedbackId is required", 400);
            }

            // Query the read model database by ID
            // Check if feedback exists and is not deleted (unless IncludeDeleted is true)
            var feedback = await repository.GetFeedbackByIdAsync(
                request.FeedbackId,
                request.IncludeDeleted,
                cancellationToken);

            if (feedback == null)
            {
                return Result<EmployeeFeedbackReadModel>.Fail($"Feedback with ID {request.FeedbackId} not found", 404);
            }

            // Return the feedback record with full details including provider information, ratings, and comments
            return Result<EmployeeFeedbackReadModel>.Success(feedback);
        }
        catch (Exception ex)
        {
            return Result<EmployeeFeedbackReadModel>.Fail($"Failed to get feedback by ID: {ex.Message}", 500);
        }
    }
}