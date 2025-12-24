namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to archive a feedback template.
/// Changes template status to Archived, making it unavailable for use.
/// </summary>
public record ArchiveFeedbackTemplateCommand(Guid TemplateId) : ICommand<Result>;
