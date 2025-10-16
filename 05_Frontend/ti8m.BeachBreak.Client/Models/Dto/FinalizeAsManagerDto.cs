namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for manager to finalize the questionnaire after employee confirmation.
/// This is the final step in the review process.
/// </summary>
public class FinalizeAsManagerDto
{
    public string FinalizedBy { get; set; } = string.Empty;
    public string? ManagerFinalNotes { get; set; }
    public int? ExpectedVersion { get; set; }
}
