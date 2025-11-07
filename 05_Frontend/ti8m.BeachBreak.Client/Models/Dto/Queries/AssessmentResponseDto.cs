namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// DTO for assessment question responses with competency ratings.
/// Eliminates magic string keys for competency access.
/// </summary>
public class AssessmentResponseDto
{
    public Dictionary<string, CompetencyRatingDto> Competencies { get; set; } = new();

    /// <summary>
    /// Gets a competency rating by key with safe access.
    /// </summary>
    public CompetencyRatingDto? GetCompetency(string competencyKey) =>
        Competencies.TryGetValue(competencyKey, out var rating) ? rating : null;

    /// <summary>
    /// Sets a competency rating with type safety.
    /// </summary>
    public void SetCompetency(string competencyKey, int rating, string comment = "")
    {
        Competencies[competencyKey] = new CompetencyRatingDto
        {
            Rating = rating,
            Comment = comment
        };
    }

    /// <summary>
    /// Validates that all required competencies have been rated.
    /// </summary>
    public bool IsComplete(IEnumerable<string> requiredCompetencies) =>
        requiredCompetencies.All(key => Competencies.ContainsKey(key) && Competencies[key].Rating >= 0);
}