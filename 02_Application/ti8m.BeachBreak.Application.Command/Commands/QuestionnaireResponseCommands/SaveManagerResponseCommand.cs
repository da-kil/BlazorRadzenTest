using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

/// <summary>
/// Type-safe command for saving manager responses.
/// </summary>
public class SaveManagerResponseCommand : ICommand<Result<Guid>>
{
    public Guid ManagerId { get; set; }
    public Guid AssignmentId { get; set; }

    /// <summary>
    /// Section responses: SectionId -> CompletionRole -> QuestionResponseValue
    /// Section IS the question (2-level dictionary, no nested questionId).
    /// </summary>
    public Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> SectionResponses { get; set; }

    public SaveManagerResponseCommand(
        Guid managerId,
        Guid assignmentId,
        Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> sectionResponses)
    {
        ManagerId = managerId;
        AssignmentId = assignmentId;
        SectionResponses = sectionResponses;
    }
}
