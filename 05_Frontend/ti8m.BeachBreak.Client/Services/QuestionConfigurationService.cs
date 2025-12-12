using System.Text.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service to handle question configuration deserialization and management.
/// Centralizes logic for handling different data formats (List, JsonElement, string).
/// </summary>
public class QuestionConfigurationService
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    /// <summary>
    /// Generic method to extract and deserialize configuration lists from question configuration dictionary.
    /// Handles three formats: direct List cast, JsonElement deserialization, and string deserialization.
    /// </summary>
    /// <typeparam name="T">The type of items in the list</typeparam>
    /// <param name="configuration">The configuration dictionary</param>
    /// <param name="key">The key to look up in the dictionary</param>
    /// <returns>List of items, or empty list if not found or deserialization fails</returns>
    public List<T> GetConfigurationList<T>(Dictionary<string, object> configuration, string key)
    {
        if (!configuration.TryGetValue(key, out var configValue))
        {
            return new List<T>();
        }

        // Handle direct cast (when set in editor)
        if (configValue is List<T> directList)
        {
            return directList;
        }

        // Handle JSON deserialization (when loaded from API)
        if (configValue is JsonElement jsonElement)
        {
            try
            {
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(jsonElement.GetRawText(), jsonOptions)
                        ?? new List<T>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing {key} from JsonElement: {ex.Message}");
            }
        }

        // Handle string representation (backup case)
        if (configValue is string jsonString)
        {
            try
            {
                return JsonSerializer.Deserialize<List<T>>(jsonString, jsonOptions)
                    ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing {key} from string: {ex.Message}");
            }
        }

        return new List<T>();
    }

    /// <summary>
    /// Gets evaluations from question configuration
    /// </summary>
    public List<EvaluationItem> GetEvaluations(QuestionSection question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            return config.Evaluations;
        }
        return new List<EvaluationItem>();
    }


    /// <summary>
    /// Gets text sections from question configuration
    /// </summary>
    public List<TextSection> GetTextSections(QuestionSection question)
    {
        if (question.Configuration is TextQuestionConfiguration config)
        {
            return config.TextSections;
        }
        return new List<TextSection>();
    }

    /// <summary>
    /// Gets the rating scale value from a question's configuration.
    /// Returns 4 (default) if not configured.
    /// </summary>
    public int GetRatingScale(QuestionSection question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            return config.RatingScale;
        }
        return 4; // Default rating scale
    }

    /// <summary>
    /// Gets the scale low label from a question's configuration.
    /// Returns "Poor" (default) if not configured.
    /// </summary>
    public string GetScaleLowLabel(QuestionSection question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            return config.ScaleLowLabel ?? "Poor";
        }
        return "Poor";
    }

    /// <summary>
    /// Gets the scale high label from a question's configuration.
    /// Returns "Excellent" (default) if not configured.
    /// </summary>
    public string GetScaleHighLabel(QuestionSection question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            return config.ScaleHighLabel ?? "Excellent";
        }
        return "Excellent";
    }

    /// <summary>
    /// Updates evaluations in question configuration
    /// </summary>
    public void SetEvaluations(QuestionSection question, List<EvaluationItem> evaluations)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            config.Evaluations = evaluations;
        }
        else
        {
            question.Configuration = new AssessmentConfiguration
            {
                Evaluations = evaluations,
                RatingScale = 4,
                ScaleLowLabel = "Poor",
                ScaleHighLabel = "Excellent"
            };
        }
    }


    /// <summary>
    /// Updates text sections in question configuration
    /// </summary>
    public void SetTextSections(QuestionSection question, List<TextSection> textSections)
    {
        if (question.Configuration is TextQuestionConfiguration config)
        {
            config.TextSections = textSections;
        }
        else
        {
            question.Configuration = new TextQuestionConfiguration
            {
                TextSections = textSections
            };
        }
    }

    /// <summary>
    /// Validates if a question has valid configuration based on its type
    /// </summary>
    public bool HasValidConfiguration(QuestionSection question)
    {
        return question.Type switch
        {
            QuestionType.Assessment => GetEvaluations(question).Any(),
            QuestionType.Goal => true, // Goal questions don't require template items - items added dynamically during in-progress
            QuestionType.TextQuestion => GetTextSections(question).Any(),
            _ => false
        };
    }

    /// <summary>
    /// Gets configuration item count for a question
    /// </summary>
    public int GetConfigurationItemCount(QuestionSection question)
    {
        return question.Type switch
        {
            QuestionType.Assessment => GetEvaluations(question).Count,
            QuestionType.Goal => 0, // Goal questions don't have template items - items added dynamically during in-progress
            QuestionType.TextQuestion => GetTextSections(question).Count,
            _ => 0
        };
    }
}
