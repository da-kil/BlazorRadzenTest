using System.Text.Json;
using System.Text.Json.Serialization;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
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

        // All data must have type discriminator property
        if (!root.TryGetProperty(TypeDiscriminatorPropertyName, out var typeProperty))
        {
            throw new JsonException($"Missing required type discriminator property '{TypeDiscriminatorPropertyName}' in QuestionResponseValue JSON");
        }

        var typeValue = typeProperty.GetString();
        return typeValue switch
        {
            TextResponseTypeValue => DeserializeTextResponse(root),
            AssessmentResponseTypeValue => DeserializeAssessmentResponse(root),
            GoalResponseTypeValue => DeserializeGoalResponse(root),
            _ => throw new JsonException($"Unknown QuestionResponseValue type discriminator: '{typeValue}'")
        };
    }

    private static QuestionResponseValue.TextResponse DeserializeTextResponse(JsonElement root)
    {
        var dto = JsonSerializer.Deserialize<TextResponseDto>(root.GetRawText())
            ?? throw new JsonException("Failed to deserialize TextResponse");
        return new QuestionResponseValue.TextResponse(dto.TextSections);
    }

    private class TextResponseDto
    {
        public List<string> TextSections { get; set; } = new();
    }

    private static QuestionResponseValue.AssessmentResponse DeserializeAssessmentResponse(JsonElement root)
    {
        var dto = JsonSerializer.Deserialize<AssessmentResponseDto>(root.GetRawText())
            ?? throw new JsonException("Failed to deserialize AssessmentResponse");
        var competencies = dto.Competencies.ToDictionary(
            kvp => kvp.Key,
            kvp => new CompetencyRating(kvp.Value.Rating, kvp.Value.Comment ?? string.Empty)
        );
        return new QuestionResponseValue.AssessmentResponse(competencies);
    }

    private class AssessmentResponseDto
    {
        public Dictionary<string, CompetencyRatingDto> Competencies { get; set; } = new();
    }

    private class CompetencyRatingDto
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    private static QuestionResponseValue.GoalResponse DeserializeGoalResponse(JsonElement root)
    {
        var dto = JsonSerializer.Deserialize<GoalResponseDto>(root.GetRawText())
            ?? throw new JsonException("Failed to deserialize GoalResponse");

        var goals = dto.Goals.Select(g => new GoalData(
            g.GoalId,
            g.ObjectiveDescription,
            g.TimeframeFrom,
            g.TimeframeTo,
            g.MeasurementMetric,
            g.WeightingPercentage,
            g.AddedByRole
        )).ToList();

        var predecessorRatings = dto.PredecessorRatings.Select(pr => new PredecessorRating(
            pr.SourceGoalId,
            pr.DegreeOfAchievement,
            pr.Justification ?? string.Empty,
            pr.RatedByRole,
            pr.OriginalObjective,
            pr.OriginalAddedByRole
        )).ToList();

        return new QuestionResponseValue.GoalResponse(goals, predecessorRatings, dto.PredecessorAssignmentId);
    }

    private class GoalResponseDto
    {
        public List<GoalDataDto> Goals { get; set; } = new();
        public List<PredecessorRatingDto> PredecessorRatings { get; set; } = new();
        public Guid? PredecessorAssignmentId { get; set; }
    }

    private class GoalDataDto
    {
        public Guid GoalId { get; set; }
        public string ObjectiveDescription { get; set; } = string.Empty;
        public DateTime TimeframeFrom { get; set; }
        public DateTime TimeframeTo { get; set; }
        public string MeasurementMetric { get; set; } = string.Empty;
        public decimal WeightingPercentage { get; set; }
        public ApplicationRole AddedByRole { get; set; }
    }

    private class PredecessorRatingDto
    {
        public Guid SourceGoalId { get; set; }
        public string OriginalObjective { get; set; } = string.Empty;
        public int DegreeOfAchievement { get; set; }
        public string? Justification { get; set; }
        public ApplicationRole RatedByRole { get; set; }
        public ApplicationRole OriginalAddedByRole { get; set; }
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