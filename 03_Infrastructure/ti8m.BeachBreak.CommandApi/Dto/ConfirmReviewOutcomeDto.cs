using System.ComponentModel.DataAnnotations;
using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for employee to confirm the review outcome.
/// Employee cannot reject but can add comments about the review.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class ConfirmReviewOutcomeDto
{
    /// <summary>
    /// Optional comments from employee about the review
    /// </summary>
    public string? EmployeeComments { get; set; }

    /// <summary>
    /// Optional version for optimistic concurrency control
    /// </summary>
    public int? ExpectedVersion { get; set; }
}
