using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

/// <summary>
/// Represents a rating of a goal from a previous questionnaire.
/// Provides type safety for predecessor goal evaluation.
/// </summary>
public class PredecessorRating : ValueObject
{
    public Guid SourceGoalId { get; }
    public int DegreeOfAchievement { get; }
    public string Justification { get; }
    public ApplicationRole RatedByRole { get; }
    public string OriginalObjective { get; }
    public ApplicationRole OriginalAddedByRole { get; }

    public PredecessorRating(
        Guid sourceGoalId,
        int degreeOfAchievement,
        string justification,
        ApplicationRole ratedByRole,
        string originalObjective,
        ApplicationRole originalAddedByRole)
    {
        if (degreeOfAchievement < 0 || degreeOfAchievement > 100)
            throw new ArgumentOutOfRangeException(nameof(degreeOfAchievement), "Degree of achievement must be between 0 and 100");

        if (string.IsNullOrWhiteSpace(originalObjective))
            throw new ArgumentException("Original objective cannot be empty", nameof(originalObjective));

        SourceGoalId = sourceGoalId;
        DegreeOfAchievement = degreeOfAchievement;
        Justification = justification;
        RatedByRole = ratedByRole;
        OriginalObjective = originalObjective;
        OriginalAddedByRole = originalAddedByRole;
    }

    /// <summary>
    /// Validates that this rating has all required data.
    /// </summary>
    public bool IsValid =>
        SourceGoalId != Guid.Empty &&
        DegreeOfAchievement >= 0 && DegreeOfAchievement <= 100 &&
        !string.IsNullOrWhiteSpace(OriginalObjective);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SourceGoalId;
        yield return DegreeOfAchievement;
        yield return Justification;
        yield return RatedByRole;
        yield return OriginalObjective;
        yield return OriginalAddedByRole;
    }
}
