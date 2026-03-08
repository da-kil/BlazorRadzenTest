namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Value object representing a viewer who has read-only access to a questionnaire assignment.
/// Viewers are internal employees who can access assignment data for collaboration, mentoring, or oversight.
/// </summary>
public record AssignmentViewer(
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeEmail,
    DateTime AddedDate,
    Guid AddedByEmployeeId);