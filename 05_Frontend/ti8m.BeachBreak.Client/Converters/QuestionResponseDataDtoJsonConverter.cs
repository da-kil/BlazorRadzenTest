using System.Text.Json;
using System.Text.Json.Serialization;
using ti8m.BeachBreak.Client.Models.DTOs;

namespace ti8m.BeachBreak.Client.Converters;

/// <summary>
/// Custom JSON converter for QuestionResponseDataDto that handles both string and numeric discriminators
/// and provides detailed logging for debugging deserialization issues.
/// </summary>
public class QuestionResponseDataDtoJsonConverter : JsonConverter<QuestionResponseDataDto>
{
    public override QuestionResponseDataDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            Console.WriteLine($"DEBUG CONVERTER: Expected StartObject, got {reader.TokenType}");
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        Console.WriteLine($"DEBUG CONVERTER: Raw JSON: {root.GetRawText()}");

        // Try to get the $type discriminator
        string? typeDiscriminator = null;
        int? numericDiscriminator = null;

        if (root.TryGetProperty("$type", out var typeElement))
        {
            if (typeElement.ValueKind == JsonValueKind.String)
            {
                typeDiscriminator = typeElement.GetString();
                Console.WriteLine($"DEBUG CONVERTER: Found string discriminator: '{typeDiscriminator}'");
            }
            else if (typeElement.ValueKind == JsonValueKind.Number)
            {
                numericDiscriminator = typeElement.GetInt32();
                Console.WriteLine($"DEBUG CONVERTER: Found numeric discriminator: {numericDiscriminator}");
            }
        }
        else
        {
            Console.WriteLine("DEBUG CONVERTER: No $type discriminator found");
        }

        // Create the appropriate type based on discriminator
        QuestionResponseDataDto? result = null;

        if (numericDiscriminator.HasValue)
        {
            result = numericDiscriminator.Value switch
            {
                0 => JsonSerializer.Deserialize<AssessmentResponseDataDto>(root.GetRawText(), options),
                1 => JsonSerializer.Deserialize<TextResponseDataDto>(root.GetRawText(), options),
                2 => JsonSerializer.Deserialize<GoalResponseDataDto>(root.GetRawText(), options),
                _ => null
            };
            Console.WriteLine($"DEBUG CONVERTER: Created {result?.GetType().Name} from numeric discriminator {numericDiscriminator}");
        }
        else if (!string.IsNullOrEmpty(typeDiscriminator))
        {
            result = typeDiscriminator switch
            {
                "assessment" => JsonSerializer.Deserialize<AssessmentResponseDataDto>(root.GetRawText(), options),
                "text" => JsonSerializer.Deserialize<TextResponseDataDto>(root.GetRawText(), options),
                "goal" => JsonSerializer.Deserialize<GoalResponseDataDto>(root.GetRawText(), options),
                _ => null
            };
            Console.WriteLine($"DEBUG CONVERTER: Created {result?.GetType().Name} from string discriminator '{typeDiscriminator}'");
        }
        else
        {
            // Fallback: try to infer from properties
            if (root.TryGetProperty("Evaluations", out _))
            {
                result = JsonSerializer.Deserialize<AssessmentResponseDataDto>(root.GetRawText(), options);
                Console.WriteLine("DEBUG CONVERTER: Inferred AssessmentResponseDataDto from Evaluations property");
            }
            else if (root.TryGetProperty("TextSections", out _))
            {
                result = JsonSerializer.Deserialize<TextResponseDataDto>(root.GetRawText(), options);
                Console.WriteLine("DEBUG CONVERTER: Inferred TextResponseDataDto from TextSections property");
            }
            else if (root.TryGetProperty("Goals", out _))
            {
                result = JsonSerializer.Deserialize<GoalResponseDataDto>(root.GetRawText(), options);
                Console.WriteLine("DEBUG CONVERTER: Inferred GoalResponseDataDto from Goals property");
            }
        }

        if (result == null)
        {
            Console.WriteLine("DEBUG CONVERTER: Failed to create any response data type!");
        }
        else
        {
            Console.WriteLine($"DEBUG CONVERTER: Successfully created {result.GetType().Name}");
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, QuestionResponseDataDto value, JsonSerializerOptions options)
    {
        // Let the default serialization handle writing
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}