using System.Text.Json;
using System.Text.Json.Serialization;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.Infrastructure.Marten.JsonSerialization;

/// <summary>
/// Custom JSON converter for polymorphic serialization/deserialization of QuestionResponseValue discriminated union.
/// This enables System.Text.Json to properly handle the abstract QuestionResponseValue type and its concrete
/// implementations: TextResponse, AssessmentResponse, and GoalResponse.
///
/// Uses a type discriminator property "$type" to identify which concrete type to instantiate during deserialization.
/// </summary>
public class QuestionResponseValueJsonConverter : JsonConverter<QuestionResponseValue>
{
    private const string TypeDiscriminatorPropertyName = "$type";
    private const string TextResponseTypeValue = "text";
    private const string AssessmentResponseTypeValue = "assessment";
    private const string GoalResponseTypeValue = "goal";

    public override QuestionResponseValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var rawJson = root.GetRawText();

        // Check for type discriminator property (new format)
        if (root.TryGetProperty(TypeDiscriminatorPropertyName, out var typeProperty))
        {
            var typeValue = typeProperty.GetString();
            return typeValue switch
            {
                TextResponseTypeValue => JsonSerializer.Deserialize<QuestionResponseValue.TextResponse>(rawJson, options)!,
                AssessmentResponseTypeValue => JsonSerializer.Deserialize<QuestionResponseValue.AssessmentResponse>(rawJson, options)!,
                GoalResponseTypeValue => JsonSerializer.Deserialize<QuestionResponseValue.GoalResponse>(rawJson, options)!,
                _ => throw new JsonException($"Unknown QuestionResponseValue type discriminator: '{typeValue}'")
            };
        }

        // Legacy format - infer type from JSON structure
        var inferredType = InferTypeFromJsonStructure(root);
        return inferredType switch
        {
            TextResponseTypeValue => JsonSerializer.Deserialize<QuestionResponseValue.TextResponse>(rawJson, options)!,
            AssessmentResponseTypeValue => JsonSerializer.Deserialize<QuestionResponseValue.AssessmentResponse>(rawJson, options)!,
            GoalResponseTypeValue => JsonSerializer.Deserialize<QuestionResponseValue.GoalResponse>(rawJson, options)!,
            _ => throw new JsonException($"Could not infer QuestionResponseValue type from JSON structure: {rawJson}")
        };
    }

    /// <summary>
    /// Infers the QuestionResponseValue type from the JSON structure for legacy data without type discriminators.
    /// </summary>
    private static string InferTypeFromJsonStructure(JsonElement root)
    {
        // Check for TextResponse structure: has "TextSections" property
        if (root.TryGetProperty("TextSections", out _))
        {
            return TextResponseTypeValue;
        }

        // Check for AssessmentResponse structure: has "Competencies" property
        if (root.TryGetProperty("Competencies", out _))
        {
            return AssessmentResponseTypeValue;
        }

        // Check for GoalResponse structure: has "Goals", "PredecessorRatings", or "PredecessorAssignmentId" properties
        if (root.TryGetProperty("Goals", out _) ||
            root.TryGetProperty("PredecessorRatings", out _) ||
            root.TryGetProperty("PredecessorAssignmentId", out _))
        {
            return GoalResponseTypeValue;
        }

        // Default fallback - could indicate corrupted or unknown data format
        return TextResponseTypeValue;
    }

    public override void Write(Utf8JsonWriter writer, QuestionResponseValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write type discriminator first
        var typeValue = value switch
        {
            QuestionResponseValue.TextResponse => TextResponseTypeValue,
            QuestionResponseValue.AssessmentResponse => AssessmentResponseTypeValue,
            QuestionResponseValue.GoalResponse => GoalResponseTypeValue,
            _ => throw new JsonException($"Unknown QuestionResponseValue type: {value.GetType()}")
        };

        writer.WriteString(TypeDiscriminatorPropertyName, typeValue);

        // Serialize the object properties (excluding the type discriminator we just added)
        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        using var document = JsonDocument.Parse(json);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Name != TypeDiscriminatorPropertyName) // Avoid duplicate type property
            {
                property.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }
}