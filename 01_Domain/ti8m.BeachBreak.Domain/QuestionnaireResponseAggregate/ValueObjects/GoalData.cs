using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

/// <summary>
/// Strongly-typed representation of goal data.
/// Replaces magic string keys with compile-time validated properties.
/// </summary>
public record GoalData
{
    public Guid GoalId { get; init; }
    public string ObjectiveDescription { get; init; }
    public DateTime TimeframeFrom { get; init; }
    public DateTime TimeframeTo { get; init; }
    public string MeasurementMetric { get; init; }
    public decimal WeightingPercentage { get; init; }
    public ApplicationRole AddedByRole { get; init; }

    public GoalData(
        Guid goalId,
        string objectiveDescription,
        DateTime timeframeFrom,
        DateTime timeframeTo,
        string measurementMetric,
        decimal weightingPercentage,
        ApplicationRole addedByRole)
    {
        if (string.IsNullOrWhiteSpace(objectiveDescription))
            throw new ArgumentException("Objective description cannot be empty", nameof(objectiveDescription));

        if (string.IsNullOrWhiteSpace(measurementMetric))
            throw new ArgumentException("Measurement metric cannot be empty", nameof(measurementMetric));

        if (timeframeFrom >= timeframeTo)
            throw new ArgumentException("Timeframe 'from' must be before 'to'");

        if (weightingPercentage < 0 || weightingPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(weightingPercentage), "Weighting must be between 0 and 100");

        GoalId = goalId;
        ObjectiveDescription = objectiveDescription;
        TimeframeFrom = timeframeFrom;
        TimeframeTo = timeframeTo;
        MeasurementMetric = measurementMetric;
        WeightingPercentage = weightingPercentage;
        AddedByRole = addedByRole;
    }

    /// <summary>
    /// Validates that this goal has all required data for completion.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(ObjectiveDescription) &&
        !string.IsNullOrWhiteSpace(MeasurementMetric) &&
        TimeframeFrom < TimeframeTo &&
        WeightingPercentage >= 0 && WeightingPercentage <= 100;
}