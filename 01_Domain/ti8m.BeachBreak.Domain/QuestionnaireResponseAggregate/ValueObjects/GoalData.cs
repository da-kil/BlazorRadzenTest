using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

/// <summary>
/// Strongly-typed representation of goal data.
/// Replaces magic string keys with compile-time validated properties.
/// </summary>
public class GoalData : ValueObject
{
    public Guid GoalId { get; }
    public string ObjectiveDescription { get; }
    public GoalTimeframe Timeframe { get; }
    public string MeasurementMetric { get; }
    public decimal WeightingPercentage { get; }
    public ApplicationRole AddedByRole { get; }

    // Convenience passthroughs
    public DateTime TimeframeFrom => Timeframe.From;
    public DateTime TimeframeTo => Timeframe.To;

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

        if (weightingPercentage < 0 || weightingPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(weightingPercentage), "Weighting must be between 0 and 100");

        GoalId = goalId;
        ObjectiveDescription = objectiveDescription;
        Timeframe = new GoalTimeframe(timeframeFrom, timeframeTo); // validates From < To
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
        Timeframe.From < Timeframe.To &&
        WeightingPercentage >= 0 && WeightingPercentage <= 100;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GoalId;
        yield return ObjectiveDescription;
        yield return Timeframe;
        yield return MeasurementMetric;
        yield return WeightingPercentage;
        yield return AddedByRole;
    }
}
