namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for withdrawing/canceling questionnaire assignments.
/// Maps to the backend WithdrawAssignmentDto for assignment cancellation functionality.
/// </summary>
public class WithdrawAssignmentDto
{
    public Guid AssignmentId { get; set; }
    public string? WithdrawalReason { get; set; }
}