namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class CreateBulkAssignmentsCommand : ICommand<Result>
{
    public Guid TemplateId { get; init; }
    public bool RequiresManagerReview { get; init; }
    public List<EmployeeAssignmentData> EmployeeAssignments { get; init; } = new();
    public DateTime? DueDate { get; init; }
    public string? AssignedBy { get; init; }
    public Guid? AssignedByEmployeeId { get; init; }
    public string? Notes { get; init; }

    public CreateBulkAssignmentsCommand(
        Guid templateId,
        bool requiresManagerReview,
        List<EmployeeAssignmentData> employeeAssignments,
        DateTime? dueDate = null,
        string? assignedBy = null,
        Guid? assignedByEmployeeId = null,
        string? notes = null)
    {
        TemplateId = templateId;
        RequiresManagerReview = requiresManagerReview;
        EmployeeAssignments = employeeAssignments;
        DueDate = dueDate;
        AssignedBy = assignedBy;
        AssignedByEmployeeId = assignedByEmployeeId;
        Notes = notes;
    }
}