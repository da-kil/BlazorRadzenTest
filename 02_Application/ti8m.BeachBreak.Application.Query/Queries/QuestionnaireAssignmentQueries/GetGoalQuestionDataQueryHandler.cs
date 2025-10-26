using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query handler to retrieve goal data for a specific question within an assignment.
/// Reads from the event-sourced QuestionnaireAssignment aggregate.
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
        // Load the aggregate from event stream
        var assignment = await _repository.LoadAggregateAsync(query.AssignmentId, cancellationToken);

        if (assignment == null)
        {
            return Result<GoalQuestionDataDto>.Fail($"Assignment {query.AssignmentId} not found", 404);
        }

        // Build DTO from aggregate state
        var dto = new GoalQuestionDataDto
        {
            QuestionId = query.QuestionId
        };

        // Get predecessor link if exists
        if (assignment.PredecessorLinks.TryGetValue(query.QuestionId, out var predecessorId))
        {
            dto.PredecessorAssignmentId = predecessorId;
        }

        // Get goals for this question
        if (assignment.GoalsByQuestion.TryGetValue(query.QuestionId, out var goals))
        {
            dto.Goals = goals.Select(g => new GoalDto
            {
                Id = g.Id,
                QuestionId = g.QuestionId,
                AddedByRole = g.AddedByRole.ToString(), // Convert enum to string
                TimeframeFrom = g.TimeframeFrom,
                TimeframeTo = g.TimeframeTo,
                ObjectiveDescription = g.ObjectiveDescription,
                MeasurementMetric = g.MeasurementMetric,
                WeightingPercentage = g.WeightingPercentage,
                AddedAt = g.AddedAt,
                AddedByEmployeeId = g.AddedByEmployeeId,
                Modifications = g.Modifications.Select(m => new GoalModificationDto
                {
                    ModifiedByRole = m.ModifiedByRole.ToString(), // Convert enum to string
                    ChangeReason = m.ChangeReason,
                    ModifiedAt = m.ModifiedAt,
                    ModifiedByEmployeeId = m.ModifiedByEmployeeId,
                    TimeframeFrom = m.TimeframeFrom,
                    TimeframeTo = m.TimeframeTo,
                    ObjectiveDescription = m.ObjectiveDescription,
                    MeasurementMetric = m.MeasurementMetric,
                    WeightingPercentage = m.WeightingPercentage
                }).ToList()
            }).ToList();
        }

        // Get predecessor goal ratings for this question
        if (assignment.GoalRatingsByQuestion.TryGetValue(query.QuestionId, out var ratings))
        {
            dto.PredecessorGoalRatings = ratings.Select(r => new GoalRatingDto
            {
                Id = r.Id,
                SourceAssignmentId = r.SourceAssignmentId,
                SourceGoalId = r.SourceGoalId,
                QuestionId = r.QuestionId,
                RatedByRole = r.RatedByRole,
                DegreeOfAchievement = r.DegreeOfAchievement,
                Justification = r.Justification,
                OriginalObjectiveDescription = r.Snapshot.ObjectiveDescription,
                OriginalTimeframeFrom = r.Snapshot.TimeframeFrom,
                OriginalTimeframeTo = r.Snapshot.TimeframeTo,
                OriginalMeasurementMetric = r.Snapshot.MeasurementMetric,
                OriginalAddedByRole = r.Snapshot.AddedByRole,
                OriginalWeightingPercentage = r.Snapshot.WeightingPercentage
            }).ToList();
        }

        return Result<GoalQuestionDataDto>.Success(dto);
    }
}
