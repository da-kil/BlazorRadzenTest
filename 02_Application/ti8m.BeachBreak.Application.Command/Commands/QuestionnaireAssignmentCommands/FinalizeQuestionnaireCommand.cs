namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record FinalizeQuestionnaireCommand(
    Guid AssignmentId,
    string FinalizedBy) : ICommand<Result>;
