using System.ComponentModel.DataAnnotations;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for finishing a review meeting.
/// Manager uses this to complete the review meeting phase.
/// </summary>
public class FinishReviewMeetingDto
{
    /// <summary>
    /// Optional version for optimistic concurrency control
    /// </summary>
    public int? ExpectedVersion { get; set; }
}
