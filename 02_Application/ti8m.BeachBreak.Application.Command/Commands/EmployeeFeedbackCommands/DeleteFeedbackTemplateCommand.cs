namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to delete a feedback template (soft delete).
/// Marks template as deleted without removing from event store.
/// </summary>
public record DeleteFeedbackTemplateCommand(Guid TemplateId) : ICommand<Result>;
