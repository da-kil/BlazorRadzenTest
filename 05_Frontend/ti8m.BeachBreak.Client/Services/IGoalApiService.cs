using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;
using ti8m.BeachBreak.Client.Models.Dto.Shared;

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
    /// Rates a predecessor goal (from a linked predecessor questionnaire).
    /// </summary>
    Task<Result> RatePredecessorGoalAsync(Guid assignmentId, RatePredecessorGoalDto dto);

    /// <summary>
    /// Modifies an existing predecessor goal rating.
    /// </summary>
    Task<Result> ModifyPredecessorGoalRatingAsync(Guid assignmentId, Guid sourceGoalId, ModifyPredecessorGoalRatingDto dto);

    /// <summary>
    /// Gets available predecessor questionnaires that can be linked for goal rating.
    /// Returns finalized questionnaires for same employee, same category, that have ANY goals.
    /// Works across cloned templates with different question IDs.
    /// </summary>
    Task<Result<IEnumerable<AvailablePredecessorDto>>> GetAvailablePredecessorsAsync(Guid assignmentId);

    /// <summary>
    /// Gets all goal data for a specific question (current goals and predecessor ratings).
    /// </summary>
    Task<Result<GoalQuestionDataDto>> GetGoalQuestionDataAsync(Guid assignmentId, Guid questionId);
}
