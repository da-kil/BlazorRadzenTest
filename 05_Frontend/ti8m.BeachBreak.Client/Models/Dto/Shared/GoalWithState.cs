namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// GoalDto enhanced with pending state tracking for UI operations
/// </summary>
public class GoalWithState : GoalDto
{
    /// <summary>
    /// True if this goal was added in the current session and hasn't been saved to backend yet
    /// </summary>
    public bool IsPendingAdd { get; set; }

    /// <summary>
    /// True if this goal has been modified in the current session and hasn't been saved yet
    /// </summary>
    public bool IsPendingModify { get; set; }

    /// <summary>
    /// True if this goal has been marked for deletion but not yet removed from backend
    /// </summary>
    public bool IsPendingDelete { get; set; }

    /// <summary>
    /// True if this goal has any pending changes
    /// </summary>
    public bool HasPendingChanges => IsPendingAdd || IsPendingModify || IsPendingDelete;

    /// <summary>
    /// True if this is a temporary goal created in the current session
    /// </summary>
    public bool IsTemporary => IsPendingAdd && (Id == Guid.Empty || Id == default);

    /// <summary>
    /// Creates a new GoalWithState from an existing GoalDto
    /// </summary>
    public static GoalWithState FromGoalDto(GoalDto goalDto)
    {
        return new GoalWithState
        {
            Id = goalDto.Id,
            QuestionId = goalDto.QuestionId,
            AddedByRole = goalDto.AddedByRole,
            TimeframeFrom = goalDto.TimeframeFrom,
            TimeframeTo = goalDto.TimeframeTo,
            ObjectiveDescription = goalDto.ObjectiveDescription,
            MeasurementMetric = goalDto.MeasurementMetric,
            WeightingPercentage = goalDto.WeightingPercentage,
            AddedAt = goalDto.AddedAt,
            AddedByEmployeeId = goalDto.AddedByEmployeeId,
            Modifications = goalDto.Modifications,
            // Pending flags default to false for persisted goals
            IsPendingAdd = false,
            IsPendingModify = false,
            IsPendingDelete = false
        };
    }

    /// <summary>
    /// Converts back to a plain GoalDto for API operations
    /// </summary>
    public GoalDto ToGoalDto()
    {
        return new GoalDto
        {
            Id = Id,
            QuestionId = QuestionId,
            AddedByRole = AddedByRole,
            TimeframeFrom = TimeframeFrom,
            TimeframeTo = TimeframeTo,
            ObjectiveDescription = ObjectiveDescription,
            MeasurementMetric = MeasurementMetric,
            WeightingPercentage = WeightingPercentage,
            AddedAt = AddedAt,
            AddedByEmployeeId = AddedByEmployeeId,
            Modifications = Modifications
        };
    }
}