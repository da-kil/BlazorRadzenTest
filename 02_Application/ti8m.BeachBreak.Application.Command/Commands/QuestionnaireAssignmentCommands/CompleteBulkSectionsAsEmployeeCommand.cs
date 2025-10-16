namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record CompleteBulkSectionsAsEmployeeCommand(
    Guid AssignmentId,
    List<Guid> SectionIds) : ICommand<Result>;
