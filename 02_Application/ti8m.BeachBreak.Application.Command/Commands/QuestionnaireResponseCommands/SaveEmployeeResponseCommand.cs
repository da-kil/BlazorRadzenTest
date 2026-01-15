using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

/// <summary>
/// Type-safe command for saving employee responses.
/// </summary>
public class SaveEmployeeResponseCommand : ICommand<Result<Guid>>
{
    public Guid EmployeeId { get; set; }
    public Guid AssignmentId { get; set; }

    /// <summary>
    /// Section responses: SectionId -> CompletionRole -> QuestionResponseValue
    /// Section IS the question (2-level dictionary, no nested questionId).
    /// </summary>
    public Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> SectionResponses { get; set; }

    public SaveEmployeeResponseCommand(
        Guid employeeId,
        Guid assignmentId,
        Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> sectionResponses)
    {
        EmployeeId = employeeId;
        AssignmentId = assignmentId;
        SectionResponses = sectionResponses;
    }
}