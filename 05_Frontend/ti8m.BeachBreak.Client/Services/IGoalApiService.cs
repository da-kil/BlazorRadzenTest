using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// API service for goal question operations.
/// Provides methods to add, modify, rate goals and link predecessors.
/// </summary>
public interface IGoalApiService
{
    /// <summary>
    /// Links a predecessor questionnaire to a goal question for rating previous goals.
    /// </summary>
    Task<Result> LinkPredecessorAsync(Guid assignmentId, LinkPredecessorQuestionnaireDto dto);

    /// <summary>
    /// Adds a new goal to a goal question.
    /// </summary>
    Task<Result<Guid>> AddGoalAsync(Guid assignmentId, AddGoalDto dto);

    /// <summary>
    /// Modifies an existing goal.
    /// </summary>
    Task<Result> ModifyGoalAsync(Guid assignmentId, Guid goalId, ModifyGoalDto dto);

    /// <summary>
    /// Deletes an existing goal.
    /// </summary>
    Task<Result> DeleteGoalAsync(Guid assignmentId, Guid goalId);

    /// <summary>
    /// Rates a predecessor goal (from a linked predecessor questionnaire).
    /// </summary>
    Task<Result> RatePredecessorGoalAsync(Guid assignmentId, RatePredecessorGoalDto dto);

    /// <summary>
    /// Modifies an existing predecessor goal rating.
    /// </summary>
    Task<Result> ModifyPredecessorGoalRatingAsync(Guid assignmentId, Guid sourceGoalId, ModifyPredecessorGoalRatingDto dto);

    /// <summary>
    /// Gets available predecessor questionnaires that can be linked for goal rating.
    /// Returns finalized questionnaires for same employee, same category, that have goals.
    /// </summary>
    Task<Result<IEnumerable<AvailablePredecessorDto>>> GetAvailablePredecessorsAsync(Guid assignmentId, Guid questionId);

    /// <summary>
    /// Gets all goal data for a specific question (current goals and predecessor ratings).
    /// </summary>
    Task<Result<GoalQuestionDataDto>> GetGoalQuestionDataAsync(Guid assignmentId, Guid questionId);
}
