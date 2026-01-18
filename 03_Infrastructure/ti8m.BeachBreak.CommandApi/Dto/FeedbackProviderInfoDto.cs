using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.ValueObjects;
using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for feedback provider information including project context.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
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

    /// <summary>
    /// Converts DTO to domain value object.
    /// </summary>
    public FeedbackProviderInfo ToValueObject()
    {
        return new FeedbackProviderInfo(
            ProviderName,
            ProviderRole,
            ProjectName,
            ProjectContext);
    }

    /// <summary>
    /// Creates DTO from domain value object.
    /// </summary>
    public static FeedbackProviderInfoDto FromValueObject(FeedbackProviderInfo providerInfo)
    {
        return new FeedbackProviderInfoDto
        {
            ProviderName = providerInfo.ProviderName,
            ProviderRole = providerInfo.ProviderRole,
            ProjectName = providerInfo.ProjectName,
            ProjectContext = providerInfo.ProjectContext
        };
    }
}