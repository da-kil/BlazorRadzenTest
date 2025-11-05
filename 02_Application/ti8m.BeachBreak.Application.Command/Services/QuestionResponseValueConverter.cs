using System.Text.Json;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Command.Services;

/// <summary>
/// Converts legacy Dictionary<Guid, object> question responses to type-safe QuestionResponseValue objects.
/// Handles the conversion from the old ComplexValue format to the new discriminated union.
/// </summary>
public static class QuestionResponseValueConverter
{
    /// <summary>
    /// Converts a legacy Dictionary<Guid, object> response to QuestionResponseValue.
    /// </summary>
    public static Dictionary<Guid, QuestionResponseValue> ConvertQuestionResponses(Dictionary<Guid, object> legacyResponses)
    {
        var result = new Dictionary<Guid, QuestionResponseValue>();

        foreach (var kvp in legacyResponses)
        {
            var questionId = kvp.Key;
            var responseData = kvp.Value;

            var convertedValue = ConvertSingleResponse(responseData);
            if (convertedValue != null)
            {
                result[questionId] = convertedValue;
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a single response object to QuestionResponseValue.
    /// </summary>
    private static QuestionResponseValue? ConvertSingleResponse(object responseData)
    {
        if (responseData == null)
            return null;

        try
        {
            // If it's already a QuestionResponseValue, return as-is
            if (responseData is QuestionResponseValue existing)
                return existing;

            // Try to deserialize from JSON if it's a string
            if (responseData is string jsonString)
            {
                return DeserializeFromJson(jsonString);
            }

            // Handle Dictionary<string, object> format (ComplexValue)
            if (responseData is Dictionary<string, object> complexValue)
            {
                return ConvertFromComplexValue(complexValue);
            }

            // Handle JsonElement (from System.Text.Json deserialization)
            if (responseData is JsonElement jsonElement)
            {
                return ConvertFromJsonElement(jsonElement);
            }

            // Fallback - try to serialize then deserialize
            var jsonFallback = JsonSerializer.Serialize(responseData);
            return DeserializeFromJson(jsonFallback);
        }
        catch (Exception)
        {
            // If all conversion attempts fail, return null
            // This ensures we don't crash the entire operation due to one bad response
            return null;
        }
    }

    /// <summary>
    /// Converts from Dictionary<string, object> ComplexValue format.
    /// </summary>
    private static QuestionResponseValue? ConvertFromComplexValue(Dictionary<string, object> complexValue)
    {
        // Detection logic based on keys in the dictionary
        var keys = complexValue.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Assessment Response Detection
        if (keys.Any(k => k.StartsWith("rating_", StringComparison.OrdinalIgnoreCase)) ||
            keys.Any(k => k.StartsWith("comment_", StringComparison.OrdinalIgnoreCase)))
        {
            return ConvertToAssessmentResponse(complexValue);
        }

        // Goal Response Detection
        if (keys.Contains("Description", StringComparer.OrdinalIgnoreCase) &&
            keys.Contains("AchievementPercentage", StringComparer.OrdinalIgnoreCase))
        {
            return ConvertToGoalResponse(complexValue);
        }

        // Text Response Detection (single or multiple sections)
        if (keys.Contains("value", StringComparer.OrdinalIgnoreCase) ||
            keys.Any(k => k.StartsWith("section_", StringComparison.OrdinalIgnoreCase)) ||
            keys.Any(k => k.StartsWith("text_", StringComparison.OrdinalIgnoreCase)))
        {
            return ConvertToTextResponse(complexValue);
        }

        return null;
    }

    /// <summary>
    /// Converts to AssessmentResponse.
    /// </summary>
    private static QuestionResponseValue ConvertToAssessmentResponse(Dictionary<string, object> complexValue)
    {
        var competencies = new Dictionary<string, CompetencyRating>();

        // Group ratings and comments by competency key
        var competencyKeys = new HashSet<string>();

        foreach (var kvp in complexValue)
        {
            var key = kvp.Key;
            if (key.StartsWith("rating_", StringComparison.OrdinalIgnoreCase))
            {
                var competencyKey = key.Substring(7); // Remove "rating_" prefix
                competencyKeys.Add(competencyKey);
            }
            else if (key.StartsWith("comment_", StringComparison.OrdinalIgnoreCase))
            {
                var competencyKey = key.Substring(8); // Remove "comment_" prefix
                competencyKeys.Add(competencyKey);
            }
        }

        foreach (var competencyKey in competencyKeys)
        {
            var ratingKey = $"rating_{competencyKey}";
            var commentKey = $"comment_{competencyKey}";

            var rating = 0;
            var comment = string.Empty;

            if (complexValue.TryGetValue(ratingKey, out var ratingObj) &&
                int.TryParse(ratingObj?.ToString(), out var parsedRating))
            {
                rating = parsedRating;
            }

            if (complexValue.TryGetValue(commentKey, out var commentObj))
            {
                comment = commentObj?.ToString() ?? string.Empty;
            }

            competencies[competencyKey] = new CompetencyRating(rating, comment);
        }

        return new QuestionResponseValue.AssessmentResponse(competencies);
    }

    /// <summary>
    /// Converts to GoalResponse.
    /// </summary>
    private static QuestionResponseValue ConvertToGoalResponse(Dictionary<string, object> complexValue)
    {
        var description = complexValue.TryGetValue("Description", out var descObj) ? descObj?.ToString() ?? string.Empty : string.Empty;

        var percentage = 0m;
        if (complexValue.TryGetValue("AchievementPercentage", out var percentObj) &&
            decimal.TryParse(percentObj?.ToString(), out var parsedPercent))
        {
            percentage = parsedPercent;
        }

        var justification = complexValue.TryGetValue("Justification", out var justObj) ? justObj?.ToString() ?? string.Empty : string.Empty;

        // Create a GoalData with required parameters (using defaults for missing values)
        var goalId = Guid.NewGuid(); // Generate new ID for converted data
        var timeframeFrom = DateTime.Today; // Default to current date
        var timeframeTo = DateTime.Today.AddMonths(12); // Default to 1 year from now
        var measurementMetric = !string.IsNullOrWhiteSpace(justification) ? justification : "Achievement percentage"; // Use justification as metric
        var addedByRole = ApplicationRole.Employee; // Default to Employee role

        var goalData = new GoalData(
            goalId,
            description,
            timeframeFrom,
            timeframeTo,
            measurementMetric,
            percentage,
            addedByRole);

        var goals = new List<GoalData> { goalData };

        // Handle predecessor ratings if present
        var predecessorRatings = new List<PredecessorRating>();
        // TODO: Add predecessor rating conversion logic if needed

        return new QuestionResponseValue.GoalResponse(goals, predecessorRatings);
    }

    /// <summary>
    /// Converts to TextResponse.
    /// </summary>
    private static QuestionResponseValue ConvertToTextResponse(Dictionary<string, object> complexValue)
    {
        var textSections = new List<string>();

        // Check for single value format
        if (complexValue.TryGetValue("value", out var singleValue))
        {
            textSections.Add(singleValue?.ToString() ?? string.Empty);
        }
        else
        {
            // Check for section-based format (section_0, section_1, etc.)
            var sectionKeys = complexValue.Keys
                .Where(k => k.StartsWith("section_", StringComparison.OrdinalIgnoreCase))
                .OrderBy(k => k)
                .ToList();

            if (sectionKeys.Any())
            {
                foreach (var key in sectionKeys)
                {
                    var text = complexValue[key]?.ToString() ?? string.Empty;
                    textSections.Add(text);
                }
            }
            else
            {
                // Check for legacy text_N format
                var textKeys = complexValue.Keys
                    .Where(k => k.StartsWith("text_", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(k => k)
                    .ToList();

                foreach (var key in textKeys)
                {
                    var text = complexValue[key]?.ToString() ?? string.Empty;
                    textSections.Add(text);
                }
            }
        }

        // Ensure we have at least one section
        if (textSections.Count == 0)
        {
            textSections.Add(string.Empty);
        }

        return new QuestionResponseValue.TextResponse(textSections);
    }

    /// <summary>
    /// Converts from JsonElement.
    /// </summary>
    private static QuestionResponseValue? ConvertFromJsonElement(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var property in jsonElement.EnumerateObject())
            {
                dictionary[property.Name] = property.Value.GetRawText();
            }
            return ConvertFromComplexValue(dictionary);
        }

        return null;
    }

    /// <summary>
    /// Deserializes QuestionResponseValue from JSON string.
    /// </summary>
    private static QuestionResponseValue? DeserializeFromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<QuestionResponseValue>(json);
        }
        catch
        {
            return null;
        }
    }
}