namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record CompleteSectionAsManagerCommand(
    Guid AssignmentId,
    Guid SectionId) : ICommand<Result>;
