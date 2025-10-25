using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Immutable snapshot of a goal from a predecessor questionnaire.
/// Captured at linking time to protect against historical changes.
/// </summary>
public class GoalSnapshot : ValueObject
{
    public string ObjectiveDescription { get; private set; } = string.Empty;
    public DateTime TimeframeFrom { get; private set; }
    public DateTime TimeframeTo { get; private set; }
    public string MeasurementMetric { get; private set; } = string.Empty;
    public CompletionRole AddedByRole { get; private set; }
    public decimal WeightingPercentage { get; private set; }

    private GoalSnapshot() { }

    public GoalSnapshot(
        string objectiveDescription,
        DateTime timeframeFrom,
        DateTime timeframeTo,
        string measurementMetric,
        CompletionRole addedByRole,
        decimal weightingPercentage)
    {
        ObjectiveDescription = objectiveDescription ?? string.Empty;
        TimeframeFrom = timeframeFrom;
        TimeframeTo = timeframeTo;
        MeasurementMetric = measurementMetric ?? string.Empty;
        AddedByRole = addedByRole;
        WeightingPercentage = weightingPercentage;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ObjectiveDescription;
        yield return TimeframeFrom;
        yield return TimeframeTo;
        yield return MeasurementMetric;
        yield return AddedByRole;
        yield return WeightingPercentage;
    }
}
