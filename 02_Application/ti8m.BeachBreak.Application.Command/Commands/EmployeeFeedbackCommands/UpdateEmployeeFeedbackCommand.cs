using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to update existing employee feedback.
/// Allows modification of provider info, feedback date, and feedback data.
/// </summary>
public class UpdateEmployeeFeedbackCommand : ICommand<Result>
{
    /// <summary>
    /// ID of the feedback to update.
    /// </summary>
    public Guid FeedbackId { get; set; }

    /// <summary>
    /// Updated information about the person providing the feedback.
    /// </summary>
    public FeedbackProviderInfo ProviderInfo { get; set; } = null!;

    /// <summary>
    /// Updated date when the feedback was provided.
    /// </summary>
    public DateTime FeedbackDate { get; set; }

    /// <summary>
    /// Updated feedback data (ratings and comments).
    /// </summary>
    public ConfigurableFeedbackData FeedbackData { get; set; } = null!;

    public UpdateEmployeeFeedbackCommand() { }

    public UpdateEmployeeFeedbackCommand(
        Guid feedbackId,
        FeedbackProviderInfo providerInfo,
        DateTime feedbackDate,
        ConfigurableFeedbackData feedbackData)
    {
        FeedbackId = feedbackId;
        ProviderInfo = providerInfo;
        FeedbackDate = feedbackDate;
        FeedbackData = feedbackData;
    }
}