namespace ti8m.BeachBreak.Client.Models.Dto;

public class LinkPredecessorQuestionnaireDto
{
    public Guid QuestionId { get; set; }
    public Guid PredecessorAssignmentId { get; set; }
    public string LinkedByRole { get; set; } = string.Empty; // "Employee" or "Manager"
}
