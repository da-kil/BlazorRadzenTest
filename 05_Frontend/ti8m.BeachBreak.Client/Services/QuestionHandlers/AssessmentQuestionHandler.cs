using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for Assessment question type.
/// Manages competencies with rating scales - can be completed by employee or manager.
/// </summary>
public class AssessmentQuestionHandler : IQuestionTypeHandler
{
    private readonly QuestionConfigurationService configService;

    public AssessmentQuestionHandler(QuestionConfigurationService configService)
    {
        this.configService = configService;
    }

    public QuestionType SupportedType => QuestionType.Assessment;

    public void InitializeQuestion(QuestionItem question)
    {
        // Initialize with one default competency
        var competencies = new List<CompetencyDefinition>
        {
            new CompetencyDefinition("competency_1", "", "", false, 0)
        };
        configService.SetCompetencies(question, competencies);

        // Initialize default rating scale settings
        question.Configuration["RatingScale"] = 4; // Default to 1-4 scale
        question.Configuration["ScaleLowLabel"] = "Poor";
        question.Configuration["ScaleHighLabel"] = "Excellent";
    }

    public void AddItem(QuestionItem question)
    {
        var competencies = configService.GetCompetencies(question);
        var nextOrder = competencies.Count > 0 ? competencies.Max(c => c.Order) + 1 : 0;
        var newCompetency = new CompetencyDefinition(
            $"competency_{competencies.Count + 1}",
            "",
            "",
            false,
            nextOrder
        );

        // Create a new list to ensure change detection
        var updatedCompetencies = new List<CompetencyDefinition>(competencies) { newCompetency };
        configService.SetCompetencies(question, updatedCompetencies);
    }

    public void RemoveItem(QuestionItem question, int index)
    {
        var competencies = configService.GetCompetencies(question);
        if (index >= 0 && index < competencies.Count)
        {
            competencies.RemoveAt(index);

            // Reorder remaining competencies
            for (int i = 0; i < competencies.Count; i++)
            {
                competencies[i] = new CompetencyDefinition(
                    competencies[i].Key,
                    competencies[i].Title,
                    competencies[i].Description,
                    competencies[i].IsRequired,
                    i
                );
            }

            configService.SetCompetencies(question, competencies);
        }
    }

    public int GetItemCount(QuestionItem question)
    {
        return configService.GetCompetencies(question).Count;
    }

    public List<string> Validate(QuestionItem question, string questionLabel)
    {
        var errors = new List<string>();
        var competencies = configService.GetCompetencies(question);

        if (competencies.Count == 0)
        {
            errors.Add($"{questionLabel} must have at least one competency");
        }
        else
        {
            for (int i = 0; i < competencies.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(competencies[i].Title))
                {
                    errors.Add($"Competency {i + 1} in {questionLabel} requires a title");
                }
            }
        }

        return errors;
    }

    public string GetDefaultTitle()
    {
        return "Competency Assessment";
    }
}
