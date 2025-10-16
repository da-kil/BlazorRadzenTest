namespace ti8m.BeachBreak.Client.Models.Dto;

public class FinalizeQuestionnaireDto
{
    public string FinalizedBy { get; set; } = string.Empty;
    public string? ManagerFinalNotes { get; set; }
    public int? ExpectedVersion { get; set; }
}
