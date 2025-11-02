using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Value object tracking a modification to a goal during review.
/// </summary>
public class GoalModificationRecord : ValueObject
{
    public DateTime? TimeframeFrom { get; private set; }
    public DateTime? TimeframeTo { get; private set; }
    public string? ObjectiveDescription { get; private set; }
    public string? MeasurementMetric { get; private set; }
    public decimal? WeightingPercentage { get; private set; }
    public ApplicationRole ModifiedByRole { get; private set; }
    public string ChangeReason { get; private set; } = string.Empty;
    public DateTime ModifiedAt { get; private set; }
    public Guid ModifiedByEmployeeId { get; private set; }

    private GoalModificationRecord() { }

    public GoalModificationRecord(
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
        TimeframeFrom = timeframeFrom;
        TimeframeTo = timeframeTo;
        ObjectiveDescription = objectiveDescription;
        MeasurementMetric = measurementMetric;
        WeightingPercentage = weightingPercentage;
        ModifiedByRole = modifiedByRole;
        ChangeReason = changeReason ?? string.Empty;
        ModifiedAt = modifiedAt;
        ModifiedByEmployeeId = modifiedByEmployeeId;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ModifiedAt;
        yield return ModifiedByEmployeeId;
        yield return ChangeReason;
    }
}
