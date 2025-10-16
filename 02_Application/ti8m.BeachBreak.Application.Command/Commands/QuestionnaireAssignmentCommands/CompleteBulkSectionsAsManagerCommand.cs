namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record CompleteBulkSectionsAsManagerCommand(
    Guid AssignmentId,
    List<Guid> SectionIds) : ICommand<Result>;
