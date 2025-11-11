namespace ti8m.BeachBreak.CommandApi.Dto;

public class RatePredecessorGoalDto
{
    public Guid QuestionId { get; set; }
    public Guid SourceAssignmentId { get; set; }
    public Guid SourceGoalId { get; set; }
    public string RatedByRole { get; set; } = string.Empty; // "Employee" or "Manager"
    public decimal DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;

    // Predecessor goal data (read from QuestionnaireResponse)
    public string PredecessorGoalObjectiveDescription { get; set; } = string.Empty;
    public DateTime PredecessorGoalTimeframeFrom { get; set; }
    public DateTime PredecessorGoalTimeframeTo { get; set; }
    public string PredecessorGoalMeasurementMetric { get; set; } = string.Empty;
    public string PredecessorGoalAddedByRole { get; set; } = string.Empty;
    public decimal PredecessorGoalWeightingPercentage { get; set; }
}
