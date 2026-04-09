using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

public class WithdrawalRecord : ValueObject
{
    public DateTime Date { get; private set; }
    public Guid ByEmployeeId { get; private set; }
    public string? Reason { get; private set; }

    public WithdrawalRecord(DateTime date, Guid byEmployeeId, string? reason)
    {
        Date = date;
        ByEmployeeId = byEmployeeId;
        Reason = reason;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Date;
        yield return ByEmployeeId;
        yield return Reason;
    }
}
