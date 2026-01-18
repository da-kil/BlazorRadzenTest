using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for initializing a questionnaire assignment.
/// Enables manager-only initialization phase with optional notes.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class InitializeAssignmentDto
{
    /// <summary>
    /// Optional notes about the initialization
    /// </summary>
    public string? InitializationNotes { get; set; }
}
