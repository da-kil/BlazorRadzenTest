namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class CreateAssignmentCommand : ICommand<Result>
{
    public Guid TemplateId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeEmail { get; init; } = string.Empty;
    public DateTime? DueDate { get; init; }
    public string? AssignedBy { get; init; }
    public string? Notes { get; init; }

    public CreateAssignmentCommand(
        Guid templateId,
        Guid employeeId,
        string employeeName,
        string employeeEmail,
        DateTime? dueDate = null,
        string? assignedBy = null,
        string? notes = null)
    {
        TemplateId = templateId;
        EmployeeId = employeeId;
        EmployeeName = employeeName;
        EmployeeEmail = employeeEmail;
        DueDate = dueDate;
        AssignedBy = assignedBy;
        Notes = notes;
    }
}