using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

/// <summary>
/// Type-safe command for saving manager responses.
/// </summary>
public class SaveManagerResponseCommand : ICommand<Result<Guid>>
{
    public Guid ManagerId { get; set; }
    public Guid AssignmentId { get; set; }

    // Type-safe section responses: SectionId -> Role -> QuestionId -> QuestionResponseValue
    public Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>> SectionResponses { get; set; }

    public SaveManagerResponseCommand(
        Guid managerId,
        Guid assignmentId,
        Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>> sectionResponses)
    {
        ManagerId = managerId;
        AssignmentId = assignmentId;
        SectionResponses = sectionResponses;
    }
}
