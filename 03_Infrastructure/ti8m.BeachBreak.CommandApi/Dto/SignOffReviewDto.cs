using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for employee to sign-off on review outcome.
/// This is the intermediate step after manager finishes review meeting
/// but before final employee confirmation.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class SignOffReviewDto
{
    /// <summary>
    /// Optional comments from employee about the sign-off
    /// </summary>
    public string? SignOffComments { get; set; }

    /// <summary>
    /// Optional version for optimistic concurrency control
    /// </summary>
    public int? ExpectedVersion { get; set; }
}