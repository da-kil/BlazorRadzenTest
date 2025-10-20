using System.ComponentModel.DataAnnotations;

namespace ti8m.BeachBreak.CommandApi.Dto;

public class SendReminderDto
{
    [Required]
    public Guid AssignmentId { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}
