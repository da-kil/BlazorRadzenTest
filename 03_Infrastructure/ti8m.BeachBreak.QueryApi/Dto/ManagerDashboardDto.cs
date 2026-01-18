using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.QueryApi.Serialization;

namespace ti8m.BeachBreak.QueryApi.Dto;

[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
public class ManagerDashboardDto
{
    public Guid ManagerId { get; set; }
    public string ManagerFullName { get; set; } = string.Empty;
    public string ManagerEmail { get; set; } = string.Empty;

    // Team-wide Aggregate Metrics
    public int TeamPendingCount { get; set; } = 0;
    public int TeamInProgressCount { get; set; } = 0;
    public int TeamCompletedCount { get; set; } = 0;
    public int TeamMemberCount { get; set; } = 0;

    // Team Members with their individual metrics
    public List<TeamMemberMetricsDto> TeamMembers { get; set; } = new();

    // Urgent assignments across the team
    public List<TeamUrgentAssignmentDto> UrgentAssignments { get; set; } = new();

    public DateTime LastUpdated { get; set; }
}

[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
public class TeamMemberMetricsDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public int PendingCount { get; set; } = 0;
    public int InProgressCount { get; set; } = 0;
    public int CompletedCount { get; set; } = 0;
    public int UrgentCount { get; set; } = 0;
    public bool HasOverdueItems { get; set; } = false;
}

[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
public class TeamUrgentAssignmentDto
{
    public Guid AssignmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string QuestionnaireTemplateName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string WorkflowState { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public int DaysUntilDue { get; set; }
}
