using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto.Commands;
using ti8m.BeachBreak.Client.Models.DTOs;

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
    /// Converts text question response from ResponseData format to TextResponseCommandDto.
    /// </summary>
    private static TextResponseCommandDto? ConvertTextResponse(QuestionResponse response)
    {
        if (response.ResponseData is not TextResponseDataDto textData) return null;

        var textSections = new List<string>(textData.TextSections);

        return textSections.Any() ? new TextResponseCommandDto { TextSections = textSections } : null;
    }

    /// <summary>
    /// Converts assessment question response from ResponseData format to AssessmentResponseCommandDto.
    /// </summary>
    private static AssessmentResponseCommandDto? ConvertAssessmentResponse(QuestionResponse response)
    {
        if (response.ResponseData is not AssessmentResponseDataDto assessmentData) return null;

        var competencies = new Dictionary<string, CompetencyRatingCommandDto>();

        // Convert from AssessmentResponseDataDto to command format
        foreach (var kvp in assessmentData.Competencies)
        {
            competencies[kvp.Key] = new CompetencyRatingCommandDto
            {
                Rating = kvp.Value.Rating,
                Comment = kvp.Value.Comment
            };
        }

        return competencies.Any()
            ? new AssessmentResponseCommandDto { Competencies = competencies }
            : null;
    }

    /// <summary>
    /// Converts goal question response from ResponseData format to GoalResponseCommandDto.
    /// </summary>
    private static GoalResponseCommandDto? ConvertGoalResponse(QuestionResponse response)
    {
        if (response.ResponseData is not GoalResponseDataDto goalData) return null;

        var goals = new List<GoalDataCommandDto>();
        var predecessorRatings = new List<PredecessorRatingCommandDto>();

        // Convert goals
        foreach (var goal in goalData.Goals)
        {
            goals.Add(new GoalDataCommandDto
            {
                GoalId = goal.GoalId,
                ObjectiveDescription = goal.ObjectiveDescription,
                TimeframeFrom = goal.TimeframeFrom,
                TimeframeTo = goal.TimeframeTo,
                MeasurementMetric = goal.MeasurementMetric,
                WeightingPercentage = goal.WeightingPercentage,
                AddedByRole = goal.AddedByRole
            });
        }

        // Convert predecessor ratings
        foreach (var rating in goalData.PredecessorRatings)
        {
            predecessorRatings.Add(new PredecessorRatingCommandDto
            {
                SourceGoalId = rating.SourceGoalId,
                DegreeOfAchievement = rating.DegreeOfAchievement,
                Justification = rating.Justification,
                RatedByRole = rating.RatedByRole,
                OriginalObjective = rating.OriginalObjective
            });
        }

        return new GoalResponseCommandDto
        {
            Goals = goals,
            PredecessorRatings = predecessorRatings,
            PredecessorAssignmentId = goalData.PredecessorAssignmentId
        };
    }

    /// <summary>
    /// Detects the actual question type based on the response data present.
    /// More reliable than trusting the QuestionType field which may be inconsistent.
    /// </summary>
    private static QuestionType DetectQuestionType(QuestionResponse response)
    {
        if (response.ResponseData == null)
        {
            // No response data, default to the stated question type
            return response.QuestionType;
        }

        // Check the actual ResponseData type
        return response.ResponseData switch
        {
            TextResponseDataDto => QuestionType.TextQuestion,
            AssessmentResponseDataDto => QuestionType.Assessment,
            GoalResponseDataDto => QuestionType.Goal,
            _ => response.QuestionType
        };
    }
}