using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate;

public class EmploymentPeriod : ValueObject
{
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public DateOnly? LastStartDate { get; private set; }

    public EmploymentPeriod(DateOnly startDate, DateOnly? endDate, DateOnly? lastStartDate)
    {
        StartDate = startDate;
        EndDate = endDate;
        LastStartDate = lastStartDate;
    }

    public bool IsCurrentlyEmployed => EndDate == null;

    public DateOnly EffectiveStartDate => LastStartDate ?? StartDate;

    public bool IsEmployedAt(DateOnly date) =>
        date >= EffectiveStartDate && (EndDate == null || date <= EndDate.Value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
        yield return LastStartDate;
    }
}
