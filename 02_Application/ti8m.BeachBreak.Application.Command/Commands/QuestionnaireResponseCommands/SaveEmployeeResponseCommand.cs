namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

public class SaveEmployeeResponseCommand : ICommand<Result<Guid>>
{
    public Guid EmployeeId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid TemplateId { get; set; }
    public Dictionary<Guid, object> SectionResponses { get; set; }
    public ResponseStatus Status { get; set; }

    public SaveEmployeeResponseCommand(Guid employeeId, Guid assignmentId, Dictionary<Guid, object> sectionResponses, ResponseStatus status = ResponseStatus.InProgress) : this(employeeId, assignmentId, Guid.Empty, sectionResponses, status)
    {
    }

    public SaveEmployeeResponseCommand(Guid employeeId, Guid assignmentId, Guid templateId, Dictionary<Guid, object> sectionResponses, ResponseStatus status = ResponseStatus.InProgress)
    {
        EmployeeId = employeeId;
        AssignmentId = assignmentId;
        TemplateId = templateId;
        SectionResponses = sectionResponses;
        Status = status;
    }
}