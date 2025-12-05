using ti8m.BeachBreak.Client.Models;
using QuestionCardTypes = ti8m.BeachBreak.Client.Components.QuestionnaireBuilder.QuestionCard;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for Text Question type.
/// Manages text sections for open-ended questions.
/// </summary>
public class TextQuestionHandler : IQuestionTypeHandler
{
    private readonly QuestionConfigurationService configService;

    public TextQuestionHandler(QuestionConfigurationService configService)
    {
        this.configService = configService;
    }

    public QuestionType SupportedType => QuestionType.TextQuestion;

    public void InitializeQuestion(QuestionItem question)
    {
        // Initialize with one default text section
        var textSections = new List<QuestionCardTypes.TextSection>
        {
            new QuestionCardTypes.TextSection
            {
                TitleEnglish = "",
                TitleGerman = "",
                DescriptionEnglish = "",
                DescriptionGerman = "",
                IsRequired = false,
                Order = 0
            }
        };
        configService.SetTextSections(question, textSections);
    }

    public void AddItem(QuestionItem question)
    {
        var textSections = configService.GetTextSections(question);
        var nextOrder = textSections.Count > 0 ? textSections.Max(t => t.Order) + 1 : 0;
        var newSection = new QuestionCardTypes.TextSection
        {
            TitleEnglish = "",
            TitleGerman = "",
            DescriptionEnglish = "",
            DescriptionGerman = "",
            IsRequired = false,
            Order = nextOrder
        };

        // Create a new list to ensure change detection
        var updatedSections = new List<QuestionCardTypes.TextSection>(textSections) { newSection };
        configService.SetTextSections(question, updatedSections);
    }

    public void RemoveItem(QuestionItem question, int index)
    {
        var textSections = configService.GetTextSections(question);
        if (index >= 0 && index < textSections.Count)
        {
            textSections.RemoveAt(index);

            // Reorder remaining sections
            for (int i = 0; i < textSections.Count; i++)
            {
                textSections[i].Order = i;
            }

            configService.SetTextSections(question, textSections);
        }
    }

    public int GetItemCount(QuestionItem question)
    {
        return configService.GetTextSections(question).Count;
    }

    public List<string> Validate(QuestionItem question, string questionLabel)
    {
        var errors = new List<string>();
        var textSections = configService.GetTextSections(question);

        if (textSections.Count == 0)
        {
            errors.Add($"{questionLabel} must have at least one text section");
        }
        else
        {
            for (int i = 0; i < textSections.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(textSections[i].TitleEnglish) &&
                    string.IsNullOrWhiteSpace(textSections[i].TitleGerman))
                {
                    errors.Add($"Text section {i + 1} in {questionLabel} requires a title (in English or German)");
                }
            }
        }

        return errors;
    }

    public string GetDefaultTitle()
    {
        return "Career Development & Planning";
    }
}
