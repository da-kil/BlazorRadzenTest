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
        if (question.Configuration?.ContainsKey("Evaluations") != true)
        {
            return new List<EvaluationItem>();
        }

        var evaluationsValue = question.Configuration["Evaluations"];

        // Handle direct List<EvaluationItem>
        if (evaluationsValue is List<EvaluationItem> evals)
        {
            return evals;
        }

        // Handle JsonElement (from API deserialization)
        if (evaluationsValue is System.Text.Json.JsonElement jsonElement)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                var evaluations = System.Text.Json.JsonSerializer.Deserialize<List<EvaluationItem>>(
                    jsonElement.GetRawText(),
                    options
                );

                if (evaluations != null && evaluations.Any())
                {
                    return evaluations;
                }
            }
            catch { /* Skip deserialization errors */ }
        }

        // Handle IEnumerable<object> or other list types
        if (evaluationsValue is IEnumerable<object> enumerable)
        {
            var result = new List<EvaluationItem>();
            foreach (var item in enumerable)
            {
                if (item is EvaluationItem evalItem)
                {
                    result.Add(evalItem);
                }
                else if (item is System.Text.Json.JsonElement itemJson)
                {
                    try
                    {
                        var deserializedEval = System.Text.Json.JsonSerializer.Deserialize<EvaluationItem>(
                            itemJson.GetRawText()
                        );
                        if (deserializedEval != null) result.Add(deserializedEval);
                    }
                    catch { /* Skip invalid items */ }
                }
            }
            if (result.Any()) return result;
        }

        return new List<EvaluationItem>();
    }

    public static int GetRatingScaleFromConfiguration(QuestionItem question)
    {
        if (question.Configuration?.ContainsKey("RatingScale") == true)
        {
            var value = question.Configuration["RatingScale"];

            if (value == null)
            {
                return 4;
            }

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                if (value is System.Text.Json.JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        return jsonElement.GetInt32();
                    }
                    if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        if (int.TryParse(jsonElement.GetString(), out int parsed))
                        {
                            return parsed;
                        }
                    }
                }

                if (int.TryParse(value.ToString(), out int scale))
                {
                    return scale;
                }
            }
        }

        return 4;
    }

    public static string GetScaleLowLabelFromConfiguration(QuestionItem question)
    {
        if (question.Configuration?.TryGetValue("ScaleLowLabel", out var value) == true)
        {
            if (value is System.Text.Json.JsonElement jsonElement)
            {
                return jsonElement.ValueKind == System.Text.Json.JsonValueKind.String
                    ? jsonElement.GetString() ?? "Poor"
                    : "Poor";
            }
            return value.ToString() ?? "Poor";
        }
        return "Poor";
    }

    public static string GetScaleHighLabelFromConfiguration(QuestionItem question)
    {
        if (question.Configuration?.TryGetValue("ScaleHighLabel", out var value) == true)
        {
            if (value is System.Text.Json.JsonElement jsonElement)
            {
                return jsonElement.ValueKind == System.Text.Json.JsonValueKind.String
                    ? jsonElement.GetString() ?? "Excellent"
                    : "Excellent";
            }
            return value.ToString() ?? "Excellent";
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
