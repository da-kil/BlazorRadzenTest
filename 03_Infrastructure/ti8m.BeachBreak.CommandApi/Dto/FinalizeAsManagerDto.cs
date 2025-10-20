using System.ComponentModel.DataAnnotations;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for manager to finalize the questionnaire after employee confirmation.
/// This is the final step in the review process.
/// </summary>
public class FinalizeAsManagerDto
{
    /// <summary>
    /// Optional final notes from manager before archiving
    /// </summary>
    public string? ManagerFinalNotes { get; set; }

    /// <summary>
    /// Optional version for optimistic concurrency control
    /// </summary>
    public int? ExpectedVersion { get; set; }
}
