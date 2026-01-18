using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.QueryApi.Serialization;

namespace ti8m.BeachBreak.QueryApi.Dto;

[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
public class AssignmentProgressDto
{
    public Guid AssignmentId { get; set; }
    public int ProgressPercentage { get; set; }
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan? TimeSpent { get; set; }
}