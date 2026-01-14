using ti8m.BeachBreak.Application.Query.Projections.Models;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Handler for GetAvailableEmployeeFeedbackQuery that retrieves all non-deleted feedback
/// for the employee associated with the assignment.
/// </summary>
public class GetAvailableEmployeeFeedbackQueryHandler
    : IQueryHandler<GetAvailableEmployeeFeedbackQuery, Result<List<LinkedEmployeeFeedbackDto>>>
{
    private readonly IEmployeeFeedbackRepository feedbackRepository;
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;

    public GetAvailableEmployeeFeedbackQueryHandler(
        IEmployeeFeedbackRepository feedbackRepository,
        IQuestionnaireAssignmentRepository assignmentRepository)
    {
        this.feedbackRepository = feedbackRepository;
        this.assignmentRepository = assignmentRepository;
    }

    public async Task<Result<List<LinkedEmployeeFeedbackDto>>> HandleAsync(
        GetAvailableEmployeeFeedbackQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get assignment to find employee
            var assignment = await assignmentRepository.GetAssignmentByIdAsync(query.AssignmentId, cancellationToken);
            if (assignment == null)
                return Result<List<LinkedEmployeeFeedbackDto>>.Fail($"Assignment {query.AssignmentId} not found", 404);

            // Get all non-deleted feedback for this employee
            var feedback = await feedbackRepository.GetEmployeeFeedbackAsync(
                employeeId: assignment.EmployeeId,
                includeDeleted: false,
                pageSize: 1000, // Get all feedback records
                cancellationToken: cancellationToken);

            var feedbackDtos = feedback
                .OrderByDescending(f => f.FeedbackDate)
                .Select(f => new LinkedEmployeeFeedbackDto
                {
                    FeedbackId = f.Id,
                    EmployeeId = f.EmployeeId,
                    SourceType = f.SourceType,
                    ProviderName = f.ProviderName,
                    FeedbackDate = f.FeedbackDate,
                    FeedbackData = System.Text.Json.JsonSerializer.Deserialize<Domain.EmployeeFeedbackAggregate.ValueObjects.ConfigurableFeedbackData>(f.FeedbackDataJson)!,
                    ProjectName = f.ProjectName,
                    ProjectRole = f.ProjectContext,
                    AverageRating = f.AverageRating,
                    RatedItemsCount = f.RatedCriteriaCount,
                    HasComments = f.HasUnstructuredFeedback
                })
                .ToList();

            return Result<List<LinkedEmployeeFeedbackDto>>.Success(feedbackDtos);
        }
        catch (Exception ex)
        {
            return Result<List<LinkedEmployeeFeedbackDto>>.Fail($"Failed to get available feedback: {ex.Message}", 500);
        }
    }
}
