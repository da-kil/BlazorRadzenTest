using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.DTOs;

namespace ti8m.BeachBreak.Client.Helpers;

/// <summary>
/// Shared helper for parsing assessment question configuration.
/// Eliminates code duplication across assessment-related components.
/// </summary>
public static class AssessmentConfigurationHelper
{
    public static List<EvaluationItem> GetEvaluationsFromConfiguration(QuestionItem question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            return config.Evaluations;
        }
        return new List<EvaluationItem>();
    }

    public static int GetRatingScaleFromConfiguration(QuestionItem question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            return config.RatingScale;
        }
        return 4;
    }

    public static string GetScaleLowLabelFromConfiguration(QuestionItem question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            return config.ScaleLowLabel ?? "Poor";
        }
        return "Poor";
    }

    public static string GetScaleHighLabelFromConfiguration(QuestionItem question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            return config.ScaleHighLabel ?? "Excellent";
        }
        return "Excellent";
    }

    public static string GetRatingScaleDescription(int ratingScale, string scaleLowLabel, string scaleHighLabel)
    {
        return $"1 ({scaleLowLabel}) - {ratingScale} ({scaleHighLabel})";
    }

    public static EvaluationRatingDto GetEvaluationRatingDto(QuestionResponse response, string evaluationKey)
    {
        if (response.ResponseData is AssessmentResponseDataDto assessmentData &&
            assessmentData.Evaluations.TryGetValue(evaluationKey, out var existingRating))
        {
            return existingRating;
        }

        return new EvaluationRatingDto();
    }
}
