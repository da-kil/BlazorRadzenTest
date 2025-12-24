namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model entity for section progress tracking within questionnaire assignments.
/// This is stored in the database as part of the QuestionnaireAssignmentReadModel.
/// NOT a DTO - this is the actual read model structure for persistence.
/// </summary>
public class SectionProgress
{
    public Guid SectionId { get; set; }
    public bool IsEmployeeCompleted { get; set; }
    public bool IsManagerCompleted { get; set; }
    public DateTime? EmployeeCompletedDate { get; set; }
    public DateTime? ManagerCompletedDate { get; set; }
}