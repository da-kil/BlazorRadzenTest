using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

public class FinalizationRecord : ValueObject
{
    public DateTime Date { get; private set; }
    public Guid ByEmployeeId { get; private set; }
    public string? Notes { get; private set; }

    public FinalizationRecord(DateTime date, Guid byEmployeeId, string? notes)
    {
        Date = date;
        ByEmployeeId = byEmployeeId;
        Notes = notes;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Date;
        yield return ByEmployeeId;
        yield return Notes;
    }
}
