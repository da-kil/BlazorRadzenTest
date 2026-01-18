using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;
using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for recording new employee feedback from external sources.
/// Supports Customer, Peer, and Project Colleague feedback with configurable criteria.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class RecordEmployeeFeedbackDto
{
    /// <summary>
    /// ID of the employee the feedback is about.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Type of feedback source (0=Customer, 1=Peer, 2=ProjectColleague).
    /// </summary>
    public int SourceType { get; set; }

    /// <summary>
    /// Information about the person providing the feedback.
    /// </summary>
    public FeedbackProviderInfoDto ProviderInfo { get; set; } = null!;

    /// <summary>
    /// Date when the feedback was originally provided.
    /// </summary>
    public DateTime FeedbackDate { get; set; }

    /// <summary>
    /// The actual feedback data (ratings and comments).
    /// </summary>
    public ConfigurableFeedbackDataDto FeedbackData { get; set; } = null!;

    /// <summary>
    /// Converts DTO to domain command.
    /// </summary>
    public Application.Command.Commands.EmployeeFeedbackCommands.RecordEmployeeFeedbackCommand ToCommand()
    {
        return new Application.Command.Commands.EmployeeFeedbackCommands.RecordEmployeeFeedbackCommand(
            EmployeeId,
            (FeedbackSourceType)SourceType,
            ProviderInfo.ToValueObject(),
            FeedbackDate,
            FeedbackData.ToValueObject());
    }
}