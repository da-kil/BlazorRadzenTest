namespace ti8m.BeachBreak.Client.Models;

public class ParticipantInfo
{
    public string EmployeeName { get; set; } = string.Empty;
    public string Function { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; } = DateTime.Now;
    public string Location { get; set; } = string.Empty;
}