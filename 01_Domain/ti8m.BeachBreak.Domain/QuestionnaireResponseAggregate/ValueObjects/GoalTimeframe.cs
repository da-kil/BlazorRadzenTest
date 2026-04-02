using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

public class GoalTimeframe : ValueObject
{
    public DateTime From { get; private set; }
    public DateTime To { get; private set; }

    public GoalTimeframe(DateTime from, DateTime to)
    {
        if (from >= to)
            throw new ArgumentException("Timeframe 'from' must be before 'to'");
        From = from;
        To = to;
    }

    public TimeSpan Duration => To - From;

    public bool IsActiveAt(DateTime at) => at >= From && at <= To;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return From;
        yield return To;
    }
}
