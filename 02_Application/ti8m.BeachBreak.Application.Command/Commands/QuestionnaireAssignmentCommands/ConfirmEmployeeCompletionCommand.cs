namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record ConfirmEmployeeCompletionCommand(
    Guid AssignmentId,
    string ConfirmedBy) : ICommand<Result>;
