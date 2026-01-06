namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// Client-side DTO for initializing a questionnaire assignment.
/// Enables manager-only initialization phase with optional notes.
/// Matches the CommandApi DTO for type-safe communication.
/// </summary>
public class InitializeAssignmentDto
{
    /// <summary>
    /// Optional notes about the initialization.
    /// Manager can provide context about custom questions or predecessor links.
    /// </summary>
    public string? InitializationNotes { get; set; }
}
