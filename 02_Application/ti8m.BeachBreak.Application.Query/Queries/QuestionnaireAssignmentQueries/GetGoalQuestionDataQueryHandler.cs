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

        // Get goals for this question with role-based filtering
        if (assignment.GoalsByQuestion.TryGetValue(query.QuestionId, out var goals))
        {
            var filteredGoals = FilterGoalsByWorkflowState(goals, assignment.WorkflowState, query.CurrentUserRole);

            dto.Goals = filteredGoals.Select(g => new GoalDto
            {
                Id = g.Id,
                QuestionId = g.QuestionId,
                AddedByRole = g.AddedByRole,
                TimeframeFrom = g.TimeframeFrom,
                TimeframeTo = g.TimeframeTo,
                ObjectiveDescription = g.ObjectiveDescription,
                MeasurementMetric = g.MeasurementMetric,
                WeightingPercentage = g.WeightingPercentage,
                AddedAt = g.AddedAt,
                AddedByEmployeeId = g.AddedByEmployeeId,
                Modifications = g.Modifications.Select(m => new GoalModificationDto
                {
                    ModifiedByRole = m.ModifiedByRole,
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

        // Get predecessor goals and ratings for this question
        if (dto.PredecessorAssignmentId.HasValue)
        {
            // Load predecessor assignment from read model
            var predecessor = await _repository.GetAssignmentByIdAsync(dto.PredecessorAssignmentId.Value, cancellationToken);

            if (predecessor != null)
            {
                if (predecessor.GoalsByQuestion.TryGetValue(query.QuestionId, out var predecessorGoals) && predecessorGoals.Any())
                {
                    // Get all existing ratings for this question
                    var existingRatings = assignment.GoalRatingsByQuestion.TryGetValue(query.QuestionId, out var ratings)
                        ? ratings.ToList()
                        : new List<Projections.GoalRatingReadModel>();

                    // Create rating DTOs for all combinations of predecessor goals and roles
                    var ratingDtos = new List<GoalRatingDto>();

                    foreach (var goal in predecessorGoals)
                    {
                        // Find all ratings for this specific goal (could be Employee, Manager, or both)
                        var ratingsForGoal = existingRatings.Where(r => r.SourceGoalId == goal.Id).ToList();

                        if (ratingsForGoal.Any())
                        {
                            // Add DTOs for all existing ratings
                            foreach (var rating in ratingsForGoal)
                            {
                                ratingDtos.Add(new GoalRatingDto
                                {
                                    Id = rating.Id,
                                    SourceAssignmentId = dto.PredecessorAssignmentId.Value,
                                    SourceGoalId = goal.Id,
                                    QuestionId = query.QuestionId,
                                    RatedByRole = rating.RatedByRole.ToString(),
                                    DegreeOfAchievement = rating.DegreeOfAchievement,
                                    Justification = rating.Justification ?? "",
                                    OriginalObjectiveDescription = goal.ObjectiveDescription,
                                    OriginalTimeframeFrom = goal.TimeframeFrom,
                                    OriginalTimeframeTo = goal.TimeframeTo,
                                    OriginalMeasurementMetric = goal.MeasurementMetric,
                                    OriginalAddedByRole = goal.AddedByRole.ToString(),
                                    OriginalWeightingPercentage = goal.WeightingPercentage
                                });
                            }
                        }
                        else
                        {
                            // No ratings exist yet - create placeholder entries for Employee and Manager roles
                            // This allows the frontend to show empty assessment cards that can be filled in
                            ratingDtos.Add(new GoalRatingDto
                            {
                                Id = Guid.NewGuid(),
                                SourceAssignmentId = dto.PredecessorAssignmentId.Value,
                                SourceGoalId = goal.Id,
                                QuestionId = query.QuestionId,
                                RatedByRole = ApplicationRole.Employee.ToString(),
                                DegreeOfAchievement = 0m,
                                Justification = "",
                                OriginalObjectiveDescription = goal.ObjectiveDescription,
                                OriginalTimeframeFrom = goal.TimeframeFrom,
                                OriginalTimeframeTo = goal.TimeframeTo,
                                OriginalMeasurementMetric = goal.MeasurementMetric,
                                OriginalAddedByRole = goal.AddedByRole.ToString(),
                                OriginalWeightingPercentage = goal.WeightingPercentage
                            });
                        }
                    }

                    dto.PredecessorGoalRatings = ratingDtos;
                }
            }
        }

        return Result<GoalQuestionDataDto>.Success(dto);
    }

    /// <summary>
    /// Filters goals based on workflow state and user role.
    /// In-Progress (including BothInProgress): Each role sees only their own goals.
    /// InReview and later: Manager sees all, Employee sees only their own.
    /// Post-Review: Both see all.
    /// </summary>
    private IEnumerable<Projections.GoalReadModel> FilterGoalsByWorkflowState(
        IEnumerable<Projections.GoalReadModel> goals,
        WorkflowState workflowState,
        ApplicationRole currentUserRole)
    {
        // Post-review states: Both Employee and Manager see all goals
        if (workflowState is WorkflowState.ManagerReviewConfirmed or
                              WorkflowState.EmployeeReviewConfirmed or
                              WorkflowState.Finalized)
        {
            return goals;
        }

        // InReview state: Manager sees all, Employee sees only their own
        // This covers review meetings where managers need to see both perspectives
        if (workflowState >= WorkflowState.InReview)
        {
            if (currentUserRole is ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin)
            {
                return goals; // Manager/Admin sees all goals during review
            }
            else
            {
                return goals.Where(g => g.AddedByRole == currentUserRole); // Employee sees only their own
            }
        }

        // All In-Progress states (including BothInProgress): Each role sees only their own goals
        // This ensures privacy during active goal creation and editing phases
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
