namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

public abstract partial class QuestionResponseValue
{
    /// <summary>
    /// Response for binary (Yes/No) questions. SelectedOption is "A", "B", or null if not answered.
    /// </summary>
    public sealed class BinaryResponse : QuestionResponseValue
    {
        public string? SelectedOption { get; }

        public BinaryResponse(string? selectedOption)
        {
            SelectedOption = selectedOption;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return SelectedOption;
        }
    }
}
