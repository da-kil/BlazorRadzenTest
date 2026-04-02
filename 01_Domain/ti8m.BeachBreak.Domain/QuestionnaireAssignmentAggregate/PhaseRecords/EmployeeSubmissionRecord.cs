using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

public class EmployeeSubmissionRecord : ValueObject
{
    public DateTime Date { get; private set; }
    public Guid ByEmployeeId { get; private set; }

    public EmployeeSubmissionRecord(DateTime date, Guid byEmployeeId)
    {
        Date = date;
        ByEmployeeId = byEmployeeId;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Date;
        yield return ByEmployeeId;
    }
}
