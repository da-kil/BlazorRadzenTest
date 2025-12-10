using System.Text.Json;
using System.Text.Json.Serialization;

namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Custom JSON converter for IQuestionConfiguration interface to support polymorphic serialization.
/// Uses the "$type" property to determine which concrete type to deserialize.
/// </summary>
public class QuestionConfigurationJsonConverter : JsonConverter<IQuestionConfiguration>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(IQuestionConfiguration);
    }

    public override IQuestionConfiguration? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        // Read the JSON into a JsonDocument to inspect the "$type" property
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        QuestionType questionType;

        // Look for the "$type" discriminator property
        if (root.TryGetProperty("$type", out var typeElement))
        {
            if (!typeElement.TryGetInt32(out var typeValue))
            {
                throw new JsonException("Invalid $type discriminator value");
            }
            questionType = (QuestionType)typeValue;
        }
        else
        {
            // Fallback: Try to infer the type from the JSON properties for backward compatibility
            questionType = InferQuestionTypeFromProperties(root);
        }

        // Get the raw JSON text to deserialize to the concrete type
        var rawJson = root.GetRawText();

        // Deserialize to the appropriate concrete type based on the question type
        return questionType switch
        {
            QuestionType.Assessment => JsonSerializer.Deserialize<AssessmentConfiguration>(rawJson, options),
            QuestionType.TextQuestion => JsonSerializer.Deserialize<TextQuestionConfiguration>(rawJson, options),
            QuestionType.Goal => JsonSerializer.Deserialize<GoalConfiguration>(rawJson, options),
            _ => throw new JsonException($"Unknown question type: {questionType}")
        };
    }

    /// <summary>
    /// Infers the question type from JSON properties for backward compatibility with data that doesn't have $type discriminator.
    /// </summary>
    private static QuestionType InferQuestionTypeFromProperties(JsonElement root)
    {
        // Log the properties we find for debugging
        var properties = new List<string>();
        foreach (var prop in root.EnumerateObject())
        {
            properties.Add(prop.Name);
        }
        var propertiesDebug = string.Join(", ", properties);

        // AssessmentConfiguration has "Evaluations", "RatingScale", "ScaleLowLabel", "ScaleHighLabel"
        if (root.TryGetProperty("Evaluations", out _) &&
            root.TryGetProperty("RatingScale", out _) &&
            root.TryGetProperty("ScaleLowLabel", out _))
        {
            return QuestionType.Assessment;
        }

        // Also check for legacy "Competencies" property (old name before it was changed to Evaluations)
        if (root.TryGetProperty("Competencies", out _) &&
            root.TryGetProperty("RatingScale", out _) &&
            root.TryGetProperty("ScaleLowLabel", out _))
        {
            return QuestionType.Assessment;
        }

        // TextQuestionConfiguration has "TextSections"
        if (root.TryGetProperty("TextSections", out _))
        {
            return QuestionType.TextQuestion;
        }

        // GoalConfiguration has "ShowGoalSection"
        if (root.TryGetProperty("ShowGoalSection", out _))
        {
            return QuestionType.Goal;
        }

        // Last resort: Try to use the QuestionType property if it exists
        if (root.TryGetProperty("QuestionType", out var questionTypeElement) &&
            questionTypeElement.TryGetInt32(out var questionTypeValue))
        {
            return (QuestionType)questionTypeValue;
        }

        // If we can't infer the type, throw an exception with helpful guidance and debug info
        throw new JsonException($"Cannot determine question configuration type. JSON must either have a '$type' discriminator property or contain recognizable properties (Evaluations/RatingScale for Assessment, TextSections for TextQuestion, ShowGoalSection for Goal) or a 'QuestionType' property. Found properties: [{propertiesDebug}]");
    }

    public override void Write(Utf8JsonWriter writer, IQuestionConfiguration value, JsonSerializerOptions options)
    {
        // Add the "$type" discriminator property based on the question type
        writer.WriteStartObject();
        writer.WriteNumber("$type", (int)value.QuestionType);

        // Serialize the concrete type's properties
        switch (value)
        {
            case AssessmentConfiguration assessment:
                WriteAssessmentConfiguration(writer, assessment, options);
                break;
            case TextQuestionConfiguration textQuestion:
                WriteTextQuestionConfiguration(writer, textQuestion, options);
                break;
            case GoalConfiguration goal:
                WriteGoalConfiguration(writer, goal, options);
                break;
            default:
                throw new JsonException($"Unknown configuration type: {value.GetType()}");
        }

        writer.WriteEndObject();
    }

    private void WriteAssessmentConfiguration(Utf8JsonWriter writer, AssessmentConfiguration assessment, JsonSerializerOptions options)
    {
        writer.WritePropertyName("Evaluations");
        JsonSerializer.Serialize(writer, assessment.Evaluations, options);

        writer.WriteNumber("RatingScale", assessment.RatingScale);
        writer.WriteString("ScaleLowLabel", assessment.ScaleLowLabel);
        writer.WriteString("ScaleHighLabel", assessment.ScaleHighLabel);
    }

    private void WriteTextQuestionConfiguration(Utf8JsonWriter writer, TextQuestionConfiguration textQuestion, JsonSerializerOptions options)
    {
        writer.WritePropertyName("TextSections");
        JsonSerializer.Serialize(writer, textQuestion.TextSections, options);
    }

    private void WriteGoalConfiguration(Utf8JsonWriter writer, GoalConfiguration goal, JsonSerializerOptions options)
    {
        writer.WriteBoolean("ShowGoalSection", goal.ShowGoalSection);
    }
}