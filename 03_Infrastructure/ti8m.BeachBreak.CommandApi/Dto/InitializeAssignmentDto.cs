namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for initializing a questionnaire assignment.
/// Enables manager-only initialization phase with optional notes.
/// </summary>
public class InitializeAssignmentDto
{
    /// <summary>
    /// Optional notes about the initialization
    /// </summary>
    public string? InitializationNotes { get; set; }
}
