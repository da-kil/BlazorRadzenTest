using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to add custom question sections to an assignment during initialization.
/// Custom sections are instance-specific and excluded from aggregate reports.
/// </summary>
public class AddCustomSectionsCommand : ICommand<Result>
{
    public Guid AssignmentId { get; init; }
    public List<CommandQuestionSection> Sections { get; init; }
    public Guid AddedByEmployeeId { get; init; }

    public AddCustomSectionsCommand(
        Guid assignmentId,
        List<CommandQuestionSection> sections,
        Guid addedByEmployeeId)
    {
        AssignmentId = assignmentId;
        Sections = sections ?? new List<CommandQuestionSection>();
        AddedByEmployeeId = addedByEmployeeId;
    }
}
