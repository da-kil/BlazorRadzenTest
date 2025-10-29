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

        // Get goals for this question with role-based filtering
        if (assignment.GoalsByQuestion.TryGetValue(query.QuestionId, out var goals))
        {
            var filteredGoals = FilterGoalsByWorkflowState(goals, assignment.WorkflowState, query.CurrentUserRole);

            dto.Goals = filteredGoals.Select(g => new GoalDto
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
        // Only show ratings from InReview onwards
        if (ShouldShowPredecessorRatings(assignment.WorkflowState) &&
            assignment.GoalRatingsByQuestion.TryGetValue(query.QuestionId, out var ratings))
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
                OriginalObjectiveDescription = r.SnapshotObjectiveDescription,
                OriginalTimeframeFrom = r.SnapshotTimeframeFrom,
                OriginalTimeframeTo = r.SnapshotTimeframeTo,
                OriginalMeasurementMetric = r.SnapshotMeasurementMetric,
                OriginalAddedByRole = r.SnapshotAddedByRole,
                OriginalWeightingPercentage = r.SnapshotWeightingPercentage
            }).ToList();
        }

        return Result<GoalQuestionDataDto>.Success(dto);
    }

    /// <summary>
    /// Filters goals based on workflow state and user role.
    /// In-Progress: User sees only their own goals.
    /// InReview: Manager sees all, Employee sees only their own.
    /// Post-Review: Both see all.
    /// </summary>
    private IEnumerable<Projections.GoalReadModel> FilterGoalsByWorkflowState(
        IEnumerable<Projections.GoalReadModel> goals,
        WorkflowState workflowState,
        CompletionRole currentUserRole)
    {
        // Post-review states: Both Employee and Manager see all goals
        if (workflowState >= WorkflowState.ManagerReviewConfirmed)
        {
            return goals;
        }

        // InReview: Manager sees all, Employee sees only their own
        if (workflowState == WorkflowState.InReview)
        {
            if (currentUserRole == CompletionRole.Manager)
            {
                return goals; // Manager sees all
            }
            else
            {
                return goals.Where(g => g.AddedByRole == currentUserRole); // Employee sees only their own
            }
        }

        // In-Progress states: Each role sees only their own goals
        return goals.Where(g => g.AddedByRole == currentUserRole);
    }

    /// <summary>
    /// Determines if predecessor ratings should be shown based on workflow state.
    /// Ratings are visible from InReview onwards.
    /// </summary>
    private bool ShouldShowPredecessorRatings(WorkflowState workflowState)
    {
        return workflowState >= WorkflowState.InReview;
    }
}
