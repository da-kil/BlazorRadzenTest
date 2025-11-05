namespace ti8m.BeachBreak.Client.Models.CommandDTOs;

/// <summary>
/// Command DTO for saving questionnaire responses with strong typing.
/// Eliminates the need for complex Dictionary<string, object> parsing.
/// </summary>
public class SaveQuestionnaireResponseDto
{
    /// <summary>
    /// Optional template ID to optimize section mapping.
    /// When provided, skips assignment lookup and goes directly to template lookup.
    /// When null, falls back to querying assignment to get template ID.
    /// </summary>
    public Guid? TemplateId { get; set; }

    public Dictionary<Guid, QuestionResponseCommandDto> Responses { get; set; } = new();

    /// <summary>
    /// Adds a text response to the DTO.
    /// </summary>
    public void AddTextResponse(Guid questionId, List<string> textSections)
    {
        Responses[questionId] = new QuestionResponseCommandDto
        {
            QuestionId = questionId,
            QuestionType = QuestionType.TextQuestion,
            TextResponse = new TextResponseCommandDto { TextSections = textSections }
        };
    }

    /// <summary>
    /// Adds an assessment response to the DTO.
    /// </summary>
    public void AddAssessmentResponse(Guid questionId, Dictionary<string, CompetencyRatingCommandDto> competencies)
    {
        Responses[questionId] = new QuestionResponseCommandDto
        {
            QuestionId = questionId,
            QuestionType = QuestionType.Assessment,
            AssessmentResponse = new AssessmentResponseCommandDto { Competencies = competencies }
        };
    }

    /// <summary>
    /// Adds a goal response to the DTO.
    /// </summary>
    public void AddGoalResponse(Guid questionId, GoalResponseCommandDto goalResponse)
    {
        Responses[questionId] = new QuestionResponseCommandDto
        {
            QuestionId = questionId,
            QuestionType = QuestionType.Goal,
            GoalResponse = goalResponse
        };
    }
}