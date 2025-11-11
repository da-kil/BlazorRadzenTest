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

        // Get predecessor goal ratings for this question
        // Note: Predecessor goals themselves are now stored in QuestionnaireResponse
        // Ratings are still stored in QuestionnaireAssignment.GoalRatingsByQuestion
        if (dto.PredecessorAssignmentId.HasValue)
        {
            // Get all existing ratings for this question
            var existingRatings = assignment.GoalRatingsByQuestion.TryGetValue(query.QuestionId, out var ratings)
                ? ratings.ToList()
                : new List<Projections.GoalRatingReadModel>();

            // Create rating DTOs from existing ratings
            var ratingDtos = new List<GoalRatingDto>();
            foreach (var rating in existingRatings)
            {
                ratingDtos.Add(new GoalRatingDto
                {
                    Id = rating.Id,
                    SourceAssignmentId = rating.SourceAssignmentId,
                    SourceGoalId = rating.SourceGoalId,
                    QuestionId = query.QuestionId,
                    RatedByRole = rating.RatedByRole.ToString(),
                    DegreeOfAchievement = rating.DegreeOfAchievement,
                    Justification = rating.Justification ?? "",
                    OriginalObjectiveDescription = rating.SnapshotObjectiveDescription,
                    OriginalTimeframeFrom = rating.SnapshotTimeframeFrom,
                    OriginalTimeframeTo = rating.SnapshotTimeframeTo,
                    OriginalMeasurementMetric = rating.SnapshotMeasurementMetric,
                    OriginalAddedByRole = rating.SnapshotAddedByRole.ToString(),
                    OriginalWeightingPercentage = rating.SnapshotWeightingPercentage
                });
            }

            dto.PredecessorGoalRatings = ratingDtos;
        }

        return Result<GoalQuestionDataDto>.Success(dto);
    }
}
