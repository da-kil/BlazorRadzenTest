namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record CompleteSectionAsEmployeeCommand(
    Guid AssignmentId,
    Guid SectionId) : ICommand<Result>;
