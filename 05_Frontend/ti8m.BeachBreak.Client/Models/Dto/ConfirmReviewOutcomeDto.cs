namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for employee to confirm the review outcome.
/// Employee cannot reject but can add comments about the review.
/// </summary>
public class ConfirmReviewOutcomeDto
{
    public string? EmployeeComments { get; set; }
    public int? ExpectedVersion { get; set; }
}
