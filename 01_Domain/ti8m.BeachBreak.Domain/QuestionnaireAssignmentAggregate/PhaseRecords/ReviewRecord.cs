using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

public class ReviewRecord : ValueObject
{
    public DateTime InitiatedDate { get; private set; }
    public Guid InitiatedBy { get; private set; }
    public DateTime? FinishedDate { get; private set; }
    public Guid? FinishedBy { get; private set; }

    public ReviewRecord(DateTime initiatedDate, Guid initiatedBy, DateTime? finishedDate, Guid? finishedBy)
    {
        InitiatedDate = initiatedDate;
        InitiatedBy = initiatedBy;
        FinishedDate = finishedDate;
        FinishedBy = finishedBy;
    }

    public ReviewRecord WithFinished(DateTime finishedDate, Guid finishedBy) =>
        new(InitiatedDate, InitiatedBy, finishedDate, finishedBy);

    public ReviewRecord ResetFinished() =>
        new(InitiatedDate, InitiatedBy, null, null);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return InitiatedDate;
        yield return InitiatedBy;
        yield return FinishedDate;
        yield return FinishedBy;
    }
}
