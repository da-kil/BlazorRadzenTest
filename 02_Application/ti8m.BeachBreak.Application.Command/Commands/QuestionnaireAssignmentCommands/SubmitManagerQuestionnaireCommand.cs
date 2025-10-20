namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record SubmitManagerQuestionnaireCommand(
    Guid AssignmentId,
    Guid SubmittedByEmployeeId,
    int ExpectedVersion) : ICommand<Result>;
