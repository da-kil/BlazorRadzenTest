namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

public class SaveManagerResponseCommand : ICommand<Result<Guid>>
{
    public Guid ManagerId { get; set; }
    public Guid AssignmentId { get; set; }
    public Dictionary<Guid, object> SectionResponses { get; set; }

    public SaveManagerResponseCommand(Guid managerId, Guid assignmentId, Dictionary<Guid, object> sectionResponses)
    {
        ManagerId = managerId;
        AssignmentId = assignmentId;
        SectionResponses = sectionResponses;
    }
}
