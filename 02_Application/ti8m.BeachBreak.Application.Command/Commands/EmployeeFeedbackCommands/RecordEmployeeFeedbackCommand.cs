using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to record new employee feedback from external sources.
/// Supports Customer, Peer, and Project Colleague feedback types.
/// </summary>
public class RecordEmployeeFeedbackCommand : ICommand<Result<Guid>>
{
    /// <summary>
    /// Optional ID for the feedback record. If not provided, a new one will be generated.
    /// </summary>
    public Guid? FeedbackId { get; set; }

    /// <summary>
    /// ID of the employee the feedback is about.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Type of feedback source (Customer, Peer, Project Colleague).
    /// </summary>
    public FeedbackSourceType SourceType { get; set; }

    /// <summary>
    /// Information about the person providing the feedback.
    /// </summary>
    public FeedbackProviderInfo ProviderInfo { get; set; } = null!;

    /// <summary>
    /// Date when the feedback was originally provided.
    /// </summary>
    public DateTime FeedbackDate { get; set; }

    /// <summary>
    /// The actual feedback data (ratings and comments).
    /// </summary>
    public ConfigurableFeedbackData FeedbackData { get; set; } = null!;

    public RecordEmployeeFeedbackCommand() { }

    public RecordEmployeeFeedbackCommand(
        Guid employeeId,
        FeedbackSourceType sourceType,
        FeedbackProviderInfo providerInfo,
        DateTime feedbackDate,
        ConfigurableFeedbackData feedbackData,
        Guid? feedbackId = null)
    {
        FeedbackId = feedbackId;
        EmployeeId = employeeId;
        SourceType = sourceType;
        ProviderInfo = providerInfo;
        FeedbackDate = feedbackDate;
        FeedbackData = feedbackData;
    }
}