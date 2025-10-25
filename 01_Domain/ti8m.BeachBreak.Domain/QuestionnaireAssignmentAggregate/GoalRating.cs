using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Entity representing a rating of a goal from a predecessor questionnaire.
/// Uses hybrid approach: maintains reference + snapshot for historical accuracy.
/// Identity is based on unique Id (Guid).
/// </summary>
public class GoalRating : Entity<Guid>
{
    // Reference to source
    public Guid SourceAssignmentId { get; private set; }
    public Guid SourceGoalId { get; private set; }
    public Guid QuestionId { get; private set; }

    // Snapshot (immutable copy at linking time)
    public GoalSnapshot Snapshot { get; private set; } = null!;

    // Rating data
    public CompletionRole RatedByRole { get; private set; }
    public decimal DegreeOfAchievement { get; private set; }
    public string Justification { get; private set; } = string.Empty;
    public DateTime RatedAt { get; private set; }
    public Guid RatedByEmployeeId { get; private set; }

    // Modification tracking
    public List<GoalRatingModificationRecord> Modifications { get; private set; } = new();

    private GoalRating() : base() { }

    public GoalRating(
        Guid id,
        Guid sourceAssignmentId,
        Guid sourceGoalId,
        Guid questionId,
        GoalSnapshot snapshot,
        CompletionRole ratedByRole,
        decimal degreeOfAchievement,
        string justification,
        DateTime ratedAt,
        Guid ratedByEmployeeId) : base(id)
    {
        if (degreeOfAchievement < 0 || degreeOfAchievement > 100)
            throw new ArgumentException("Degree of achievement must be between 0 and 100", nameof(degreeOfAchievement));

        SourceAssignmentId = sourceAssignmentId;
        SourceGoalId = sourceGoalId;
        QuestionId = questionId;
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        RatedByRole = ratedByRole;
        DegreeOfAchievement = degreeOfAchievement;
        Justification = justification ?? string.Empty;
        RatedAt = ratedAt;
        RatedByEmployeeId = ratedByEmployeeId;
        Modifications = new List<GoalRatingModificationRecord>();
    }

    public GoalRating ApplyModification(
        decimal? degreeOfAchievement,
        string? justification,
        CompletionRole modifiedByRole,
        string changeReason,
        DateTime modifiedAt,
        Guid modifiedByEmployeeId)
    {
        var modified = new GoalRating
        {
            SourceAssignmentId = SourceAssignmentId,
            SourceGoalId = SourceGoalId,
            QuestionId = QuestionId,
            Snapshot = Snapshot,
            RatedByRole = RatedByRole,
            DegreeOfAchievement = degreeOfAchievement ?? DegreeOfAchievement,
            Justification = justification ?? Justification,
            RatedAt = RatedAt,
            RatedByEmployeeId = RatedByEmployeeId,
            Modifications = new List<GoalRatingModificationRecord>(Modifications)
        };

        modified.Modifications.Add(new GoalRatingModificationRecord(
            degreeOfAchievement,
            justification,
            modifiedByRole,
            changeReason,
            modifiedAt,
            modifiedByEmployeeId
        ));

        return modified;
    }
}
