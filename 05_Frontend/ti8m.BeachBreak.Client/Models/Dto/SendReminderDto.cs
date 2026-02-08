using System.ComponentModel.DataAnnotations;

namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for sending reminders to questionnaire assignment recipients.
/// Maps to the backend SendReminderDto for assignment reminder functionality.
/// </summary>
public class SendReminderDto
{
    [Required]
    public Guid AssignmentId { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}