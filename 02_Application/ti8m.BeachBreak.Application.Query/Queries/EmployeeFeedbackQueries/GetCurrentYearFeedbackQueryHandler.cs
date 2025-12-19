using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Handler for GetCurrentYearFeedbackQuery that retrieves current fiscal year feedback for an employee.
/// Used for questionnaire review integration to show feedback context during reviews.
/// </summary>
public class GetCurrentYearFeedbackQueryHandler : IQueryHandler<GetCurrentYearFeedbackQuery, Result<List<EmployeeFeedbackReadModel>>>
{
    private readonly IEmployeeFeedbackRepository repository;

    public GetCurrentYearFeedbackQueryHandler(IEmployeeFeedbackRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Result<List<EmployeeFeedbackReadModel>>> HandleAsync(GetCurrentYearFeedbackQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.EmployeeId == Guid.Empty)
            {
                return Result<List<EmployeeFeedbackReadModel>>.Fail("EmployeeId is required", 400);
            }

            // Query feedback for the specified employee
            // Filtered to current fiscal year only
            // Includes all source types (Customer, Peer, Project Colleague)
            // Ordered by source type and feedback date for better organization
            // Excludes deleted feedback
            var feedbackList = await repository.GetCurrentYearFeedbackAsync(
                request.EmployeeId,
                cancellationToken);

            return Result<List<EmployeeFeedbackReadModel>>.Success(feedbackList);
        }
        catch (Exception ex)
        {
            return Result<List<EmployeeFeedbackReadModel>>.Fail($"Failed to get current year feedback: {ex.Message}", 500);
        }
    }
}