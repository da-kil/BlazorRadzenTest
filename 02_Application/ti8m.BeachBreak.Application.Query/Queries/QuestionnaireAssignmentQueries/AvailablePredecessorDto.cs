namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class AvailablePredecessorDto
{
    public Guid AssignmentId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int GoalCount { get; set; }
}
