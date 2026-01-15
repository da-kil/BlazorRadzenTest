using ti8m.BeachBreak.Application.Query.Projections.Models;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Handler for GetFeedbackQuestionDataQuery that retrieves feedback data for a specific question.
/// Returns all linked feedback records with full details.
/// </summary>
public class GetFeedbackQuestionDataQueryHandler
    : IQueryHandler<GetFeedbackQuestionDataQuery, Result<FeedbackQuestionDataDto>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly IEmployeeFeedbackRepository feedbackRepository;

    public GetFeedbackQuestionDataQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IEmployeeFeedbackRepository feedbackRepository)
    {
        this.assignmentRepository = assignmentRepository;
        this.feedbackRepository = feedbackRepository;
    }

    public async Task<Result<FeedbackQuestionDataDto>> HandleAsync(
        GetFeedbackQuestionDataQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var assignment = await assignmentRepository.GetAssignmentByIdAsync(query.AssignmentId, cancellationToken);
            if (assignment == null)
                return Result<FeedbackQuestionDataDto>.Fail($"Assignment {query.AssignmentId} not found", 404);

            // Get linked feedback IDs for this question
            var linkedFeedbackIds = assignment.LinkedFeedbackByQuestion.ContainsKey(query.QuestionId)
                ? assignment.LinkedFeedbackByQuestion[query.QuestionId]
                : new List<Guid>();

            // Fetch full feedback data
            var linkedFeedback = new List<LinkedEmployeeFeedbackDto>();
            foreach (var feedbackId in linkedFeedbackIds)
            {
                var feedback = await feedbackRepository.GetFeedbackByIdAsync(feedbackId, false, cancellationToken);
                if (feedback != null && !feedback.IsDeleted)
                {
                    linkedFeedback.Add(new LinkedEmployeeFeedbackDto
                    {
                        FeedbackId = feedback.Id,
                        EmployeeId = feedback.EmployeeId,
                        SourceType = feedback.SourceType,
                        ProviderName = feedback.ProviderName,
                        FeedbackDate = feedback.FeedbackDate,
                        FeedbackData = System.Text.Json.JsonSerializer.Deserialize<Domain.EmployeeFeedbackAggregate.ValueObjects.ConfigurableFeedbackData>(feedback.FeedbackDataJson)!,
                        ProjectName = feedback.ProjectName,
                        ProjectRole = feedback.ProjectContext,
                        AverageRating = feedback.AverageRating,
                        RatedItemsCount = feedback.RatedCriteriaCount,
                        HasComments = feedback.HasUnstructuredFeedback
                    });
                }
            }

            var result = new FeedbackQuestionDataDto
            {
                QuestionId = query.QuestionId,
                WorkflowState = assignment.WorkflowState,
                LinkedFeedback = linkedFeedback.OrderByDescending(f => f.FeedbackDate).ToList()
            };

            return Result<FeedbackQuestionDataDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<FeedbackQuestionDataDto>.Fail($"Failed to get feedback question data: {ex.Message}", 500);
        }
    }
}
