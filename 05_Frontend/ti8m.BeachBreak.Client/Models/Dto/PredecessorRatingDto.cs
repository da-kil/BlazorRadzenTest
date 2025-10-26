namespace ti8m.BeachBreak.Client.Models.Dto;

public class PredecessorRatingDto
{
    public Guid SourceGoalId { get; set; }
    public string OriginalObjectiveDescription { get; set; } = string.Empty;
    public decimal DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string RatedByRole { get; set; } = string.Empty;
}
