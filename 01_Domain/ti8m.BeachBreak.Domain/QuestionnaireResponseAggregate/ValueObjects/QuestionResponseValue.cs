namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

/// <summary>
/// Discriminated union representing different types of question responses.
/// Replaces Dictionary<string, object> with compile-time type safety.
/// </summary>
public abstract record QuestionResponseValue
{
    /// <summary>
    /// Response for text-based questions with one or more text sections.
    /// </summary>
    public sealed record TextResponse(
        IReadOnlyList<string> TextSections
    ) : QuestionResponseValue
    {
        public static TextResponse Single(string text) => new([text]);
        public static TextResponse Multiple(params string[] texts) => new(texts);
    }

    /// <summary>
    /// Response for assessment questions with competency ratings and comments.
    /// </summary>
    public sealed record AssessmentResponse(
        IReadOnlyDictionary<string, CompetencyRating> Competencies
    ) : QuestionResponseValue;

    /// <summary>
    /// Response for goal questions including goals and predecessor ratings.
    /// </summary>
    public sealed record GoalResponse(
        IReadOnlyList<GoalData> Goals,
        IReadOnlyList<PredecessorRating> PredecessorRatings,
        Guid? PredecessorAssignmentId = null
    ) : QuestionResponseValue;
}