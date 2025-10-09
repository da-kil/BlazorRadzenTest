namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record SubmitEmployeeQuestionnaireCommand(
    Guid AssignmentId,
    string SubmittedBy,
    int ExpectedVersion) : ICommand<Result>;
