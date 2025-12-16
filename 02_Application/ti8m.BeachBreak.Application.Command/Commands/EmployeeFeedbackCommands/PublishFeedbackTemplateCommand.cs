namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to publish a feedback template.
/// Changes template status to Published, making it available for feedback recording.
/// </summary>
public record PublishFeedbackTemplateCommand(Guid TemplateId) : ICommand<Result>;
