using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Entity representing a goal defined in the current questionnaire.
/// Goals are added dynamically during in-progress states, not predefined in template.
/// Identity is based on unique Id (Guid).
/// </summary>
public class Goal : Entity<Guid>
{
    public Guid QuestionId { get; private set; }
    public ApplicationRole AddedByRole { get; private set; }
    public DateTime TimeframeFrom { get; private set; }
    public DateTime TimeframeTo { get; private set; }
    public string ObjectiveDescription { get; private set; } = string.Empty;
    public string MeasurementMetric { get; private set; } = string.Empty;
    public decimal WeightingPercentage { get; private set; }
    public DateTime AddedAt { get; private set; }
    public Guid AddedByEmployeeId { get; private set; }

    // Modification tracking
    public List<GoalModificationRecord> Modifications { get; private set; } = new();

    private Goal() : base() { }

    public Goal(
        Guid id,
        Guid questionId,
        ApplicationRole addedByRole,
        DateTime timeframeFrom,
        DateTime timeframeTo,
        string objectiveDescription,
        string measurementMetric,
        decimal weightingPercentage,
        DateTime addedAt,
        Guid addedByEmployeeId) : base(id)
    {
        if (timeframeFrom >= timeframeTo)
            throw new ArgumentException("Timeframe 'From' must be before 'To'");

        if (string.IsNullOrWhiteSpace(objectiveDescription))
            throw new ArgumentException("Objective description is required", nameof(objectiveDescription));

        if (string.IsNullOrWhiteSpace(measurementMetric))
            throw new ArgumentException("Measurement metric is required", nameof(measurementMetric));

        if (weightingPercentage < 0 || weightingPercentage > 100)
            throw new ArgumentException("Weighting must be between 0 and 100 (inclusive)", nameof(weightingPercentage));
        QuestionId = questionId;
        AddedByRole = addedByRole;
        TimeframeFrom = timeframeFrom;
        TimeframeTo = timeframeTo;
        ObjectiveDescription = objectiveDescription;
        MeasurementMetric = measurementMetric;
        WeightingPercentage = weightingPercentage;
        AddedAt = addedAt;
        AddedByEmployeeId = addedByEmployeeId;
        Modifications = new List<GoalModificationRecord>();
    }

    public Goal ApplyModification(
        DateTime? timeframeFrom,
        DateTime? timeframeTo,
        string? objectiveDescription,
        string? measurementMetric,
        decimal? weightingPercentage,
        ApplicationRole modifiedByRole,
        string changeReason,
        DateTime modifiedAt,
        Guid modifiedByEmployeeId)
    {
        var modified = new Goal
        {
            Id = Id,
            QuestionId = QuestionId,
            AddedByRole = AddedByRole,
            TimeframeFrom = timeframeFrom ?? TimeframeFrom,
            TimeframeTo = timeframeTo ?? TimeframeTo,
            ObjectiveDescription = objectiveDescription ?? ObjectiveDescription,
            MeasurementMetric = measurementMetric ?? MeasurementMetric,
            WeightingPercentage = weightingPercentage ?? WeightingPercentage,
            AddedAt = AddedAt,
            AddedByEmployeeId = AddedByEmployeeId,
            Modifications = new List<GoalModificationRecord>(Modifications)
        };

        modified.Modifications.Add(new GoalModificationRecord(
            timeframeFrom,
            timeframeTo,
            objectiveDescription,
            measurementMetric,
            weightingPercentage,
            modifiedByRole,
            changeReason,
            modifiedAt,
            modifiedByEmployeeId
        ));

        return modified;
    }
}
