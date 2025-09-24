namespace ti8m.BeachBreak.Application.Query.Projections;

public class TranslationReadModel
{
    public string German { get; private set; }
    public string English { get; private set; }

    public TranslationReadModel(string german, string english)
    {
        German = german;
        English = english;
    }
}
