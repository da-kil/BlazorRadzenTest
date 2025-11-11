namespace ti8m.BeachBreak.Client.Models.Dto;

public class RatePredecessorGoalDto
{
    public Guid QuestionId { get; set; }
    public Guid SourceAssignmentId { get; set; }
    public Guid SourceGoalId { get; set; }
    public decimal DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string RatedByRole { get; set; } = string.Empty; // "Employee" or "Manager"
}
