using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

public class ReviewRecord : ValueObject
{
    public DateTime InitiatedDate { get; private set; }
    public Guid InitiatedBy { get; private set; }
    public DateTime? FinishedDate { get; private set; }
    public Guid? FinishedBy { get; private set; }
    public string? Summary { get; private set; }

    public ReviewRecord(DateTime initiatedDate, Guid initiatedBy, DateTime? finishedDate, Guid? finishedBy, string? summary)
    {
        InitiatedDate = initiatedDate;
        InitiatedBy = initiatedBy;
        FinishedDate = finishedDate;
        FinishedBy = finishedBy;
        Summary = summary;
    }

    public ReviewRecord WithFinished(DateTime finishedDate, Guid finishedBy, string? summary) =>
        new(InitiatedDate, InitiatedBy, finishedDate, finishedBy, summary);

    public ReviewRecord ResetFinished() =>
        new(InitiatedDate, InitiatedBy, null, null, Summary);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return InitiatedDate;
        yield return InitiatedBy;
        yield return FinishedDate;
        yield return FinishedBy;
        yield return Summary;
    }
}
