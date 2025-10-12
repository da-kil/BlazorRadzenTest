namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for finishing a review meeting.
/// Manager uses this to complete the review meeting phase.
/// </summary>
public class FinishReviewMeetingDto
{
    public string FinishedBy { get; set; } = string.Empty;
    public string? ReviewSummary { get; set; }
    public int? ExpectedVersion { get; set; }
}
