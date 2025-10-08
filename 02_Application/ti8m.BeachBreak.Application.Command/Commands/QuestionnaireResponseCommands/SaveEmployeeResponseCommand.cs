namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

public class SaveEmployeeResponseCommand : ICommand<Result<Guid>>
{
    public Guid EmployeeId { get; set; }
    public Guid AssignmentId { get; set; }
    public Dictionary<Guid, object> SectionResponses { get; set; }

    public SaveEmployeeResponseCommand(Guid employeeId, Guid assignmentId, Dictionary<Guid, object> sectionResponses)
    {
        EmployeeId = employeeId;
        AssignmentId = assignmentId;
        SectionResponses = sectionResponses;
    }
}