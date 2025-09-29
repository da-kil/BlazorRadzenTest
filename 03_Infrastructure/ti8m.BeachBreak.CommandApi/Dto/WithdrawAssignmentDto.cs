namespace ti8m.BeachBreak.CommandApi.Dto;

public class WithdrawAssignmentDto
{
    public Guid AssignmentId { get; set; }
    public string WithdrawnBy { get; set; } = string.Empty;
    public string? WithdrawalReason { get; set; }
}