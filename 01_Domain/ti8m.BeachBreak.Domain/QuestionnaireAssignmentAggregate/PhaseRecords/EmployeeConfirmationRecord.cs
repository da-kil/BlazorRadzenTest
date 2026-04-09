using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

public class EmployeeConfirmationRecord : ValueObject
{
    public DateTime Date { get; private set; }
    public Guid ByEmployeeId { get; private set; }
    public string? Comments { get; private set; }

    public EmployeeConfirmationRecord(DateTime date, Guid byEmployeeId, string? comments)
    {
        Date = date;
        ByEmployeeId = byEmployeeId;
        Comments = comments;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Date;
        yield return ByEmployeeId;
        yield return Comments;
    }
}
