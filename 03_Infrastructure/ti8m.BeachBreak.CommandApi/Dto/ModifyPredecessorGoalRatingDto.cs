namespace ti8m.BeachBreak.CommandApi.Dto;

public class ModifyPredecessorGoalRatingDto
{
    public Guid SourceGoalId { get; set; }
    public decimal? DegreeOfAchievement { get; set; }
    public string? Justification { get; set; }
    public string ModifiedByRole { get; set; } = string.Empty; // "Employee" or "Manager"
    public string ChangeReason { get; set; } = string.Empty;
}
