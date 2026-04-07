namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for finishing a review meeting.
/// Manager uses this to complete the review meeting phase.
/// </summary>
public class FinishReviewMeetingDto
{
    public int? ExpectedVersion { get; set; }
}
