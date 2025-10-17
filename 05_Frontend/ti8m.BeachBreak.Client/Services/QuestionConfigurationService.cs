using System.Text.Json;
using ti8m.BeachBreak.Client.Models;
using QuestionCardTypes = ti8m.BeachBreak.Client.Components.QuestionnaireBuilder.QuestionCard;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service to handle question configuration deserialization and management.
/// Centralizes logic for handling different data formats (List, JsonElement, string).
/// </summary>
public class QuestionConfigurationService
{
    private readonly JsonSerializerOptions jsonOptions;

    public QuestionConfigurationService()
    {
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

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
    /// Gets competencies from question configuration
    /// </summary>
    public List<CompetencyDefinition> GetCompetencies(QuestionItem question)
    {
        return GetConfigurationList<CompetencyDefinition>(question.Configuration, "Competencies");
    }

    /// <summary>
    /// Gets goal categories from question configuration
    /// Note: Uses nested GoalCategory type from QuestionCard for compatibility with existing code
    /// </summary>
    public List<QuestionCardTypes.GoalCategory> GetGoalCategories(QuestionItem question)
    {
        return GetConfigurationList<QuestionCardTypes.GoalCategory>(question.Configuration, "GoalCategories");
    }

    /// <summary>
    /// Gets text sections from question configuration
    /// Note: Uses nested TextSection type from QuestionCard for compatibility with existing code
    /// </summary>
    public List<QuestionCardTypes.TextSection> GetTextSections(QuestionItem question)
    {
        return GetConfigurationList<QuestionCardTypes.TextSection>(question.Configuration, "TextSections");
    }

    /// <summary>
    /// Updates competencies in question configuration
    /// </summary>
    public void SetCompetencies(QuestionItem question, List<CompetencyDefinition> competencies)
    {
        question.Configuration["Competencies"] = competencies;
    }

    /// <summary>
    /// Updates goal categories in question configuration
    /// </summary>
    public void SetGoalCategories(QuestionItem question, List<QuestionCardTypes.GoalCategory> goalCategories)
    {
        question.Configuration["GoalCategories"] = goalCategories;
    }

    /// <summary>
    /// Updates text sections in question configuration
    /// </summary>
    public void SetTextSections(QuestionItem question, List<QuestionCardTypes.TextSection> textSections)
    {
        question.Configuration["TextSections"] = textSections;
    }

    /// <summary>
    /// Validates if a question has valid configuration based on its type
    /// </summary>
    public bool HasValidConfiguration(QuestionItem question)
    {
        return question.Type switch
        {
            QuestionType.Assessment => GetCompetencies(question).Any(),
            QuestionType.GoalAchievement => GetGoalCategories(question).Any(),
            QuestionType.TextQuestion => GetTextSections(question).Any(),
            _ => false
        };
    }

    /// <summary>
    /// Gets configuration item count for a question
    /// </summary>
    public int GetConfigurationItemCount(QuestionItem question)
    {
        return question.Type switch
        {
            QuestionType.Assessment => GetCompetencies(question).Count,
            QuestionType.GoalAchievement => GetGoalCategories(question).Count,
            QuestionType.TextQuestion => GetTextSections(question).Count,
            _ => 0
        };
    }
}
