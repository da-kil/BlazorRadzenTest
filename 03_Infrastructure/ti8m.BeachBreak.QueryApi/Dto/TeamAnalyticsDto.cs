using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.QueryApi.Serialization;

namespace ti8m.BeachBreak.QueryApi.Dto;

[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
public class TeamAnalyticsDto
{
    public int TotalTeamMembers { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double AverageCompletionTime { get; set; }
    public double OnTimeCompletionRate { get; set; }
    public List<CategoryPerformanceDto> CategoryPerformance { get; set; } = new();
    public List<EmployeePerformanceDto> EmployeePerformance { get; set; } = new();
}
