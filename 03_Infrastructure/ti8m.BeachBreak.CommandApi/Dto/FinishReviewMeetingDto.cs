using System.ComponentModel.DataAnnotations;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for finishing a review meeting.
/// Manager uses this to complete the review meeting phase.
/// </summary>
public class FinishReviewMeetingDto
{
    [Required]
    public string FinishedBy { get; set; } = string.Empty;

    /// <summary>
    /// Optional summary of the review meeting discussion
    /// </summary>
    public string? ReviewSummary { get; set; }

    /// <summary>
    /// Optional version for optimistic concurrency control
    /// </summary>
    public int? ExpectedVersion { get; set; }
}
