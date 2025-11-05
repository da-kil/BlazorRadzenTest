using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.CommandDTOs;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Utility class for converting between frontend questionnaire response formats.
/// Handles conversion from section-based responses to question-based DTOs expected by the backend.
/// </summary>
public static class QuestionnaireResponseConverter
{
    /// <summary>
    /// Converts section-based responses to question-based DTO format expected by the backend.
    /// Flattens section/role structure into direct question responses and adds templateId optimization.
    /// </summary>
    /// <param name="sectionResponses">Section-based responses from the frontend</param>
    /// <param name="templateId">Optional template ID for backend optimization</param>
    /// <returns>DTO format expected by the backend API</returns>
    public static SaveQuestionnaireResponseDto ConvertToSaveQuestionnaireResponseDto(
        Dictionary<Guid, SectionResponse> sectionResponses,
        Guid? templateId = null)
    {
        var dto = new SaveQuestionnaireResponseDto
        {
            TemplateId = templateId
        };

        // Flatten section responses into question responses
        foreach (var sectionResponse in sectionResponses.Values)
        {
            foreach (var roleResponse in sectionResponse.RoleResponses)
            {
                foreach (var questionResponse in roleResponse.Value)
                {
                    var questionId = questionResponse.Key;
                    var response = questionResponse.Value;

                    // Detect actual question type based on response data (more reliable than QuestionType field)
                    var actualQuestionType = DetectQuestionType(response);

                    // Convert QuestionResponse to QuestionResponseCommandDto based on actual question type
                    var commandDto = new QuestionResponseCommandDto
                    {
                        QuestionId = questionId,
                        QuestionType = actualQuestionType
                    };

                    // Map response data based on actual question type
                    switch (actualQuestionType)
                    {
                        case QuestionType.TextQuestion:
                            commandDto.TextResponse = ConvertTextResponse(response);
                            break;

                        case QuestionType.Assessment:
                            commandDto.AssessmentResponse = ConvertAssessmentResponse(response);
                            break;

                        case QuestionType.Goal:
                            commandDto.GoalResponse = ConvertGoalResponse(response);
                            break;

                        default:
                            throw new ArgumentException($"Unsupported question type: {response.QuestionType}");
                    }

                    dto.Responses[questionId] = commandDto;
                }
            }
        }

        return dto;
    }

    /// <summary>
    /// Converts text question response from ComplexValue format to TextResponseCommandDto.
    /// </summary>
    private static TextResponseCommandDto? ConvertTextResponse(QuestionResponse response)
    {
        if (response.ComplexValue == null) return null;

        var textSections = new List<string>();

        // Handle both single value and multiple sections format
        if (response.ComplexValue.ContainsKey("value"))
        {
            // Single section format
            var value = response.ComplexValue["value"]?.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                textSections.Add(value);
            }
        }
        else if (response.ComplexValue.ContainsKey("textSections"))
        {
            // Multiple sections format
            var sections = response.ComplexValue["textSections"] as List<string>;
            if (sections != null)
            {
                textSections.AddRange(sections);
            }
        }
        else
        {
            // Check for section_0, section_1, etc. format used by OptimizedTextQuestion
            var sectionIndex = 0;
            while (response.ComplexValue.ContainsKey($"section_{sectionIndex}"))
            {
                var sectionValue = response.ComplexValue[$"section_{sectionIndex}"]?.ToString();
                if (!string.IsNullOrEmpty(sectionValue))
                {
                    textSections.Add(sectionValue);
                }
                sectionIndex++;
            }
        }

        return textSections.Any() ? new TextResponseCommandDto { TextSections = textSections } : null;
    }

    /// <summary>
    /// Converts assessment question response from ComplexValue format to AssessmentResponseCommandDto.
    /// </summary>
    private static AssessmentResponseCommandDto? ConvertAssessmentResponse(QuestionResponse response)
    {
        if (response.ComplexValue == null) return null;

        var competencies = new Dictionary<string, CompetencyRatingCommandDto>();

        // Extract competency ratings from ComplexValue using "Rating_" prefix format
        foreach (var kvp in response.ComplexValue)
        {
            if (kvp.Key.StartsWith("Rating_") && kvp.Value != null)
            {
                var competencyKey = kvp.Key.Substring(7); // Remove "Rating_" prefix

                if (int.TryParse(kvp.Value.ToString(), out var rating))
                {
                    var commentKey = $"comment_{competencyKey}";
                    var comment = response.ComplexValue.ContainsKey(commentKey)
                        ? response.ComplexValue[commentKey]?.ToString() ?? ""
                        : "";

                    competencies[competencyKey] = new CompetencyRatingCommandDto
                    {
                        Rating = rating,
                        Comment = comment
                    };
                }
            }
        }

        return competencies.Any()
            ? new AssessmentResponseCommandDto { Competencies = competencies }
            : null;
    }

    /// <summary>
    /// Converts goal question response from ComplexValue format to GoalResponseCommandDto.
    /// This is a placeholder implementation - actual goal data structure may need refinement.
    /// </summary>
    private static GoalResponseCommandDto? ConvertGoalResponse(QuestionResponse response)
    {
        if (response.ComplexValue == null) return null;

        // TODO: Implement proper goal response conversion based on actual goal data structure
        // For now, return a basic structure to prevent errors

        var goals = new List<GoalDataCommandDto>();
        var predecessorRatings = new List<PredecessorRatingCommandDto>();
        Guid? predecessorAssignmentId = null;

        // Check if this is a goal achievement response format
        if (response.ComplexValue.ContainsKey("AchievementPercentage"))
        {
            // This appears to be a predecessor rating format
            if (response.ComplexValue.ContainsKey("Description") &&
                response.ComplexValue.ContainsKey("AchievementPercentage") &&
                response.ComplexValue.ContainsKey("Justification"))
            {
                // Extract predecessor rating data - this is a simplified implementation
                // Real implementation would need to map to proper PredecessorRatingCommandDto structure
            }
        }

        return new GoalResponseCommandDto
        {
            Goals = goals,
            PredecessorRatings = predecessorRatings,
            PredecessorAssignmentId = predecessorAssignmentId
        };
    }

    /// <summary>
    /// Detects the actual question type based on the response data present.
    /// More reliable than trusting the QuestionType field which may be inconsistent.
    /// </summary>
    private static QuestionType DetectQuestionType(QuestionResponse response)
    {
        if (response.ComplexValue == null || !response.ComplexValue.Any())
        {
            // No complex data, default to the stated question type or TextQuestion
            return response.QuestionType;
        }

        // Check for assessment data (Rating_ prefix indicates assessment)
        if (response.ComplexValue.Keys.Any(k => k.StartsWith("Rating_", StringComparison.OrdinalIgnoreCase)))
        {
            return QuestionType.Assessment;
        }

        // Check for goal data (AchievementPercentage, Description, Justification indicate goals)
        if (response.ComplexValue.ContainsKey("AchievementPercentage") ||
            response.ComplexValue.ContainsKey("Description") ||
            response.ComplexValue.ContainsKey("Justification"))
        {
            return QuestionType.Goal;
        }

        // Check for text data (value, section_0, textSections indicate text)
        if (response.ComplexValue.ContainsKey("value") ||
            response.ComplexValue.ContainsKey("textSections") ||
            response.ComplexValue.Keys.Any(k => k.StartsWith("section_")))
        {
            return QuestionType.TextQuestion;
        }

        // Fallback to stated question type
        return response.QuestionType;
    }
}