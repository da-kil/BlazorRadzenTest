namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for feedback provider information including project context.
/// </summary>
public class FeedbackProviderInfoDto
{
    /// <summary>
    /// Name of the person providing the feedback.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Role or title of the person providing the feedback.
    /// </summary>
    public string ProviderRole { get; set; } = string.Empty;

    /// <summary>
    /// Project name (required for Project Colleague feedback).
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Additional project context or description.
    /// </summary>
    public string? ProjectContext { get; set; }
}