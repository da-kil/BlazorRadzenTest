namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class CreateBulkAssignmentsCommand : ICommand<Result>
{
    public Guid TemplateId { get; init; }
    public List<EmployeeAssignmentData> EmployeeAssignments { get; init; } = new();
    public DateTime? DueDate { get; init; }
    public string? AssignedBy { get; init; }
    public string? Notes { get; init; }

    public CreateBulkAssignmentsCommand(
        Guid templateId,
        List<EmployeeAssignmentData> employeeAssignments,
        DateTime? dueDate = null,
        string? assignedBy = null,
        string? notes = null)
    {
        TemplateId = templateId;
        EmployeeAssignments = employeeAssignments;
        DueDate = dueDate;
        AssignedBy = assignedBy;
        Notes = notes;
    }
}