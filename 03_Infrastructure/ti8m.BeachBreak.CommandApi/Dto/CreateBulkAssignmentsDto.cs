using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class CreateBulkAssignmentsDto
{
    public Guid TemplateId { get; set; }
    public List<EmployeeAssignmentDto> EmployeeAssignments { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
}

public class EmployeeAssignmentDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
}