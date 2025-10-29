namespace ti8m.BeachBreak.CommandApi.Dto;

public class RatePredecessorGoalDto
{
    public Guid QuestionId { get; set; }
    public Guid SourceAssignmentId { get; set; }
    public Guid SourceGoalId { get; set; }
    public string RatedByRole { get; set; } = string.Empty; // "Employee" or "Manager"
    public decimal DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;
}
