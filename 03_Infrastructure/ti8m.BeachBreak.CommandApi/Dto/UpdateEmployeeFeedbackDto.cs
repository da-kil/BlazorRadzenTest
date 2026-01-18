using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for updating existing employee feedback.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class UpdateEmployeeFeedbackDto
{
    /// <summary>
    /// Updated provider information.
    /// </summary>
    public FeedbackProviderInfoDto ProviderInfo { get; set; } = null!;

    /// <summary>
    /// Updated feedback date.
    /// </summary>
    public DateTime FeedbackDate { get; set; }

    /// <summary>
    /// Updated feedback data.
    /// </summary>
    public ConfigurableFeedbackDataDto FeedbackData { get; set; } = null!;

    /// <summary>
    /// Converts DTO to domain command.
    /// </summary>
    public Application.Command.Commands.EmployeeFeedbackCommands.UpdateEmployeeFeedbackCommand ToCommand(Guid feedbackId)
    {
        return new Application.Command.Commands.EmployeeFeedbackCommands.UpdateEmployeeFeedbackCommand(
            feedbackId,
            ProviderInfo.ToValueObject(),
            FeedbackDate,
            FeedbackData.ToValueObject());
    }
}