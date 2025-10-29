using System.Text.Json;
using System.Text.Json.Serialization;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Converters;

/// <summary>
/// JSON converter for ResponseRole enum that serializes as string instead of integer.
/// This ensures API compatibility and human-readable JSON (e.g., "Employee" instead of 0).
/// </summary>
public class ResponseRoleJsonConverter : JsonConverter<ResponseRole>
{
    public override ResponseRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return value switch
            {
                "Employee" => ResponseRole.Employee,
                "Manager" => ResponseRole.Manager,
                _ => throw new JsonException($"Invalid ResponseRole value: {value}")
            };
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Support reading as integer for backwards compatibility
            var value = reader.GetInt32();
            return value switch
            {
                0 => ResponseRole.Employee,
                1 => ResponseRole.Manager,
                _ => throw new JsonException($"Invalid ResponseRole value: {value}")
            };
        }

        throw new JsonException($"Unexpected token type for ResponseRole: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, ResponseRole value, JsonSerializerOptions options)
    {
        // Always write as string for human readability
        writer.WriteStringValue(value.ToString());
    }
}
