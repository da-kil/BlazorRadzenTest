namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

public abstract partial class QuestionResponseValue
{
    /// <summary>
    /// Response for text-based questions with one or more text sections.
    /// </summary>
    public sealed class TextResponse : QuestionResponseValue
    {
        public IReadOnlyList<string> TextSections { get; }

        public TextResponse(IReadOnlyList<string> textSections)
        {
            TextSections = textSections;
        }

        public static TextResponse Single(string text) => new([text]);
        public static TextResponse Multiple(params string[] texts) => new(texts);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            foreach (var section in TextSections)
                yield return section;
        }
    }
}
