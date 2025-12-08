namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Configuration for assessment questions with evaluation items and rating scales.
/// Users rate multiple evaluation criteria (e.g., skills, behaviors) on a numeric scale.
/// </summary>
public sealed class AssessmentConfiguration : IQuestionConfiguration
{
    /// <summary>
    /// Gets the question type.
    /// </summary>
    public QuestionType QuestionType => QuestionType.Assessment;

    /// <summary>
    /// List of evaluation items (criteria) to be rated.
    /// Each item represents a distinct aspect being assessed.
    /// </summary>
    public List<EvaluationItem> Evaluations { get; set; } = new();

    /// <summary>
    /// Maximum value on the rating scale (e.g., 4 for a 1-4 scale).
    /// Minimum is always 1.
    /// </summary>
    public int RatingScale { get; set; } = 4;

    /// <summary>
    /// Label for the lowest value on the scale (e.g., "Poor", "Needs Improvement").
    /// </summary>
    public string ScaleLowLabel { get; set; } = "Poor";

    /// <summary>
    /// Label for the highest value on the scale (e.g., "Excellent", "Outstanding").
    /// </summary>
    public string ScaleHighLabel { get; set; } = "Excellent";

    /// <summary>
    /// Gets a formatted description of the rating scale (e.g., "1 (Poor) - 4 (Excellent)").
    /// </summary>
    public string GetRatingScaleDescription()
    {
        return $"1 ({ScaleLowLabel}) - {RatingScale} ({ScaleHighLabel})";
    }
}
