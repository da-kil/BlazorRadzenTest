using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.DTOs;

namespace ti8m.BeachBreak.Client.Helpers;

/// <summary>
/// Shared helper for parsing assessment question configuration.
/// Eliminates code duplication across assessment-related components.
/// </summary>
public static class AssessmentConfigurationHelper
{
    public static List<CompetencyDefinition> GetCompetenciesFromConfiguration(QuestionItem question)
    {
        if (question.Configuration?.ContainsKey("Competencies") != true)
        {
            return new List<CompetencyDefinition>();
        }

        var competenciesValue = question.Configuration["Competencies"];

        // Handle direct List<CompetencyDefinition>
        if (competenciesValue is List<CompetencyDefinition> comps)
        {
            return comps;
        }

        // Handle JsonElement (from API deserialization)
        if (competenciesValue is System.Text.Json.JsonElement jsonElement)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                var competencies = System.Text.Json.JsonSerializer.Deserialize<List<CompetencyDefinition>>(
                    jsonElement.GetRawText(),
                    options
                );

                if (competencies != null && competencies.Any())
                {
                    return competencies;
                }
            }
            catch { /* Skip deserialization errors */ }
        }

        // Handle IEnumerable<object> or other list types
        if (competenciesValue is IEnumerable<object> enumerable)
        {
            var result = new List<CompetencyDefinition>();
            foreach (var item in enumerable)
            {
                if (item is CompetencyDefinition compDef)
                {
                    result.Add(compDef);
                }
                else if (item is System.Text.Json.JsonElement itemJson)
                {
                    try
                    {
                        var deserializedComp = System.Text.Json.JsonSerializer.Deserialize<CompetencyDefinition>(
                            itemJson.GetRawText()
                        );
                        if (deserializedComp != null) result.Add(deserializedComp);
                    }
                    catch { /* Skip invalid items */ }
                }
            }
            if (result.Any()) return result;
        }

        return new List<CompetencyDefinition>();
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

    public static CompetencyRatingDto GetCompetencyRatingDto(QuestionResponse response, string competencyKey)
    {
        if (response.ResponseData is AssessmentResponseDataDto assessmentData &&
            assessmentData.Competencies.TryGetValue(competencyKey, out var existingRating))
        {
            return existingRating;
        }

        return new CompetencyRatingDto();
    }
}
