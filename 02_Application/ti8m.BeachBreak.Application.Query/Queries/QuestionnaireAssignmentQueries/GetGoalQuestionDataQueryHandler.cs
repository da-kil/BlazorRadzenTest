using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query handler to retrieve goal data for a specific question within an assignment.
/// Reads from the QuestionnaireAssignmentReadModel projection (proper CQRS).
/// </summary>
public class GetGoalQuestionDataQueryHandler : IQueryHandler<GetGoalQuestionDataQuery, Result<GoalQuestionDataDto>>
{
    private readonly IQuestionnaireAssignmentRepository _repository;

    public GetGoalQuestionDataQueryHandler(IQuestionnaireAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GoalQuestionDataDto>> HandleAsync(GetGoalQuestionDataQuery query, CancellationToken cancellationToken = default)
    {
        // Load from read model projection instead of replaying event stream
        var assignment = await _repository.GetAssignmentByIdAsync(query.AssignmentId, cancellationToken);

        if (assignment == null)
        {
            return Result<GoalQuestionDataDto>.Fail($"Assignment {query.AssignmentId} not found", 404);
        }

        // Build DTO from aggregate state
        var dto = new GoalQuestionDataDto
        {
            QuestionId = query.QuestionId,
            WorkflowState = assignment.WorkflowState
        };

        // Get predecessor link if exists
        if (assignment.PredecessorLinksByQuestion.TryGetValue(query.QuestionId, out var predecessorId))
        {
            dto.PredecessorAssignmentId = predecessorId;
        }

        // Goals are now stored in QuestionnaireResponse.SectionResponses, not in QuestionnaireAssignment
        // Goals should be read from the main questionnaire response via GetQuestionnaireResponseQuery
        // This query handler now returns empty goals list - goals are accessed through the response data
        dto.Goals = new List<GoalDto>();

        // Predecessor goal ratings are now stored in QuestionnaireResponse.SectionResponses alongside goals
        // No longer stored in QuestionnaireAssignment aggregate
        dto.PredecessorGoalRatings = new List<GoalRatingDto>();

        return Result<GoalQuestionDataDto>.Success(dto);
    }
}
