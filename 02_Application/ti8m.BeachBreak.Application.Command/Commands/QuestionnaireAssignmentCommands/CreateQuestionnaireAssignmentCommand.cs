namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class CreateQuestionnaireAssignmentCommand : ICommand<Result>
{
    public QuestionnaireAssignment QuestionnaireAssignment { get; init; }

    public CreateQuestionnaireAssignmentCommand(QuestionnaireAssignment questionnaireAssignment)
    {
        QuestionnaireAssignment = questionnaireAssignment;
    }
}
