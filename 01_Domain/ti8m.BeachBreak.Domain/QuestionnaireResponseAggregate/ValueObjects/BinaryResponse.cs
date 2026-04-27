namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

public abstract partial class QuestionResponseValue
{
    /// <summary>
    /// Response for binary (Yes/No) questions. Stores per-item selections: itemKey → "A", "B", or null.
    /// </summary>
    public sealed class BinaryResponse : QuestionResponseValue
    {
        public IReadOnlyDictionary<string, string?> Selections { get; }

        public BinaryResponse(IReadOnlyDictionary<string, string?> selections)
        {
            Selections = selections;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            foreach (var kvp in Selections.OrderBy(k => k.Key))
            {
                yield return kvp.Key;
                yield return kvp.Value;
            }
        }
    }
}
