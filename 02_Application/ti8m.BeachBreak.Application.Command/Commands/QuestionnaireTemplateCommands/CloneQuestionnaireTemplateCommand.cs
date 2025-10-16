namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

/// <summary>
/// Command to clone an existing questionnaire template.
/// Creates a complete copy with new IDs in Draft status.
/// </summary>
public class CloneQuestionnaireTemplateCommand : ICommand<Result<Guid>>
{
    public Guid SourceTemplateId { get; init; }
    public string? NamePrefix { get; init; }

    public CloneQuestionnaireTemplateCommand(Guid sourceTemplateId, string? namePrefix = null)
    {
        SourceTemplateId = sourceTemplateId;
        NamePrefix = namePrefix;
    }
}
