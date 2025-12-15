namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for updating existing employee feedback.
/// </summary>
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
}