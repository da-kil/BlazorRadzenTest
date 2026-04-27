namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

public abstract partial class QuestionResponseValue
{
    /// <summary>
    /// Response for multiple choice questions. Stores per-question selections: questionKey → [selectedChoiceKeys].
    /// </summary>
    public sealed class MultipleChoiceResponse : QuestionResponseValue
    {
        public IReadOnlyDictionary<string, IReadOnlyList<string>> SelectionsByQuestion { get; }

        public MultipleChoiceResponse(IReadOnlyDictionary<string, IReadOnlyList<string>> selectionsByQuestion)
        {
            SelectionsByQuestion = selectionsByQuestion;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            foreach (var kvp in SelectionsByQuestion.OrderBy(k => k.Key))
            {
                yield return kvp.Key;
                foreach (var key in kvp.Value)
                    yield return key;
            }
        }
    }
}
