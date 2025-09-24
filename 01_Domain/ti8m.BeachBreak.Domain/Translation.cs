using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain;

public class Translation : ValueObject
{
    public string German { get; private set; }
    public string English { get; private set; }

    public Translation(string german, string english)
    {
        German = german;
        English = english;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return German;
        yield return English;
    }
}
