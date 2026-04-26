namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

public abstract partial class QuestionResponseValue
{
    /// <summary>
    /// Response for multiple choice questions. Stores the keys of selected options.
    /// </summary>
    public sealed class MultipleChoiceResponse : QuestionResponseValue
    {
        public IReadOnlyList<string> SelectedKeys { get; }

        public MultipleChoiceResponse(IReadOnlyList<string> selectedKeys)
        {
            SelectedKeys = selectedKeys;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            foreach (var key in SelectedKeys)
                yield return key;
        }
    }
}
