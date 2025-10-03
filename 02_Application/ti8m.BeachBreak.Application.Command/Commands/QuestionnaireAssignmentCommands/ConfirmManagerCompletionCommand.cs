namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record ConfirmManagerCompletionCommand(
    Guid AssignmentId,
    string ConfirmedBy) : ICommand<Result>;
