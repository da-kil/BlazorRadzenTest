using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

/// <summary>
/// Type-safe command for saving employee responses.
/// </summary>
public class SaveEmployeeResponseCommand : ICommand<Result<Guid>>
{
    public Guid EmployeeId { get; set; }
    public Guid AssignmentId { get; set; }

    // Type-safe section responses: SectionId -> Role -> QuestionId -> QuestionResponseValue
    public Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>> SectionResponses { get; set; }

    public SaveEmployeeResponseCommand(
        Guid employeeId,
        Guid assignmentId,
        Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>> sectionResponses)
    {
        EmployeeId = employeeId;
        AssignmentId = assignmentId;
        SectionResponses = sectionResponses;
    }
}