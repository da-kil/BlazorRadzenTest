namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

public abstract partial class QuestionResponseValue
{
    /// <summary>
    /// Response for assessment questions with evaluation ratings and comments.
    /// </summary>
    public sealed class AssessmentResponse : QuestionResponseValue
    {
        public IReadOnlyDictionary<string, EvaluationRating> Evaluations { get; }

        public AssessmentResponse(IReadOnlyDictionary<string, EvaluationRating> evaluations)
        {
            Evaluations = evaluations;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            foreach (var kvp in Evaluations.OrderBy(e => e.Key))
            {
                yield return kvp.Key;
                yield return kvp.Value;
            }
        }
    }
}
