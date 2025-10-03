using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

public class SectionProgress : ValueObject
{
    public Guid SectionId { get; private set; }
    public bool IsEmployeeCompleted { get; private set; }
    public bool IsManagerCompleted { get; private set; }
    public DateTime? EmployeeCompletedDate { get; private set; }
    public DateTime? ManagerCompletedDate { get; private set; }

    private SectionProgress() { }

    public SectionProgress(Guid sectionId)
    {
        SectionId = sectionId;
        IsEmployeeCompleted = false;
        IsManagerCompleted = false;
    }

    public SectionProgress MarkEmployeeCompleted(DateTime completedDate)
    {
        return new SectionProgress
        {
            SectionId = SectionId,
            IsEmployeeCompleted = true,
            IsManagerCompleted = IsManagerCompleted,
            EmployeeCompletedDate = completedDate,
            ManagerCompletedDate = ManagerCompletedDate
        };
    }

    public SectionProgress MarkManagerCompleted(DateTime completedDate)
    {
        return new SectionProgress
        {
            SectionId = SectionId,
            IsEmployeeCompleted = IsEmployeeCompleted,
            IsManagerCompleted = true,
            EmployeeCompletedDate = EmployeeCompletedDate,
            ManagerCompletedDate = completedDate
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SectionId;
        yield return IsEmployeeCompleted;
        yield return IsManagerCompleted;
        yield return EmployeeCompletedDate ?? DateTime.MinValue;
        yield return ManagerCompletedDate ?? DateTime.MinValue;
    }
}
