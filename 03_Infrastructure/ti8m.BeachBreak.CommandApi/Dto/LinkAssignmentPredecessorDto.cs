namespace ti8m.BeachBreak.CommandApi.Dto;

public class LinkAssignmentPredecessorDto
{
    public Guid PredecessorAssignmentId { get; set; }
    public string LinkedByRole { get; set; } = string.Empty; // "Employee" or "Manager"
}