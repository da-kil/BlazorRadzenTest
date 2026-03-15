using System.Text.Json.Serialization;

namespace ti8m.BeachBreak.Client.Models.Dto;

public class BulkPdfExportRequestDto
{
    [JsonPropertyName("AssignmentIds")]
    public List<Guid> AssignmentIds { get; set; } = new();

    [JsonPropertyName("Language")]
    public int Language { get; set; } = 0;
}
