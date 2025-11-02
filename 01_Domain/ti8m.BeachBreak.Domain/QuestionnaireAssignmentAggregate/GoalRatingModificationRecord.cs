using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Value object tracking a modification to a goal rating during review.
/// </summary>
public class GoalRatingModificationRecord : ValueObject
{
    public decimal? DegreeOfAchievement { get; private set; }
    public string? Justification { get; private set; }
    public ApplicationRole ModifiedByRole { get; private set; }
    public string ChangeReason { get; private set; } = string.Empty;
    public DateTime ModifiedAt { get; private set; }
    public Guid ModifiedByEmployeeId { get; private set; }

    private GoalRatingModificationRecord() { }

    public GoalRatingModificationRecord(
        decimal? degreeOfAchievement,
        string? justification,
        ApplicationRole modifiedByRole,
        string changeReason,
        DateTime modifiedAt,
        Guid modifiedByEmployeeId)
    {
        DegreeOfAchievement = degreeOfAchievement;
        Justification = justification;
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
