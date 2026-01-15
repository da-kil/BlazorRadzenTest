using ti8m.BeachBreak.Core.Domain;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class CreateBulkAssignmentsCommand : ICommand<Result>
{
    public Guid TemplateId { get; init; }
    public QuestionnaireProcessType ProcessType { get; init; }
    public List<EmployeeAssignmentData> EmployeeAssignments { get; init; } = new();
    public DateTime? DueDate { get; init; }
    public string? AssignedBy { get; init; }
    public Guid? AssignedByEmployeeId { get; init; }
    public string? Notes { get; init; }

    public CreateBulkAssignmentsCommand(
        Guid templateId,
        QuestionnaireProcessType processType,
        List<EmployeeAssignmentData> employeeAssignments,
        DateTime? dueDate = null,
        string? assignedBy = null,
        Guid? assignedByEmployeeId = null,
        string? notes = null)
    {
        TemplateId = templateId;
        ProcessType = processType;
        EmployeeAssignments = employeeAssignments;
        DueDate = dueDate;
        AssignedBy = assignedBy;
        AssignedByEmployeeId = assignedByEmployeeId;
        Notes = notes;
    }
}