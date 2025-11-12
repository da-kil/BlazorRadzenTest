using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

/// <summary>
/// Represents a rating of a goal from a previous questionnaire.
/// Provides type safety for predecessor goal evaluation.
/// </summary>
public record PredecessorRating
{
    public Guid SourceGoalId { get; init; }
    public int DegreeOfAchievement { get; init; }
    public string Justification { get; init; }
    public ApplicationRole RatedByRole { get; init; }
    public string OriginalObjective { get; init; }
    public ApplicationRole OriginalAddedByRole { get; init; }

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

        if (string.IsNullOrWhiteSpace(justification))
            throw new ArgumentException("Justification cannot be empty", nameof(justification));

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
        !string.IsNullOrWhiteSpace(Justification) &&
        !string.IsNullOrWhiteSpace(OriginalObjective);
}