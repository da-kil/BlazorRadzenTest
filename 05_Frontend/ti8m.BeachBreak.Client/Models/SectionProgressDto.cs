namespace ti8m.BeachBreak.Client.Models;

public class SectionProgressDto
{
    public Guid SectionId { get; set; }
    public bool IsEmployeeCompleted { get; set; }
    public bool IsManagerCompleted { get; set; }
    public DateTime? EmployeeCompletedDate { get; set; }
    public DateTime? ManagerCompletedDate { get; set; }
}
