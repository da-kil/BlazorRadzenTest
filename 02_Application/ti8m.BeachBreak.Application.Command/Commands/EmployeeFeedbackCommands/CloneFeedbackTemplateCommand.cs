namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to clone an existing feedback template.
/// Creates a new template with copied configuration and "Copy of" prefix.
/// </summary>
public record CloneFeedbackTemplateCommand(
    Guid SourceTemplateId,
    Guid NewTemplateId,
    string NamePrefix) : ICommand<Result>;
