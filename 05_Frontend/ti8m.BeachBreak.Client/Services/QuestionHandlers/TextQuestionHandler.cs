using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for Text Question type.
/// Manages text sections for open-ended questions.
/// </summary>
public class TextQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.TextQuestion;

    public void InitializeQuestion(QuestionItem question)
    {
        // Initialize with one default text section
        question.Configuration = new TextQuestionConfiguration
        {
            TextSections = new List<TextSection>
            {
                new TextSection
                {
                    TitleEnglish = "",
                    TitleGerman = "",
                    DescriptionEnglish = "",
                    DescriptionGerman = "",
                    IsRequired = false,
                    Order = 0
                }
            }
        };
    }

    public void AddItem(QuestionItem question)
    {
        if (question.Configuration is TextQuestionConfiguration config)
        {
            var nextOrder = config.TextSections.Count > 0 ? config.TextSections.Max(s => s.Order) + 1 : 0;
            var newSection = new TextSection
            {
                TitleEnglish = "",
                TitleGerman = "",
                DescriptionEnglish = "",
                DescriptionGerman = "",
                IsRequired = false,
                Order = nextOrder
            };

            config.TextSections.Add(newSection);
        }
    }

    public void RemoveItem(QuestionItem question, int index)
    {
        if (question.Configuration is TextQuestionConfiguration config)
        {
            if (index >= 0 && index < config.TextSections.Count)
            {
                config.TextSections.RemoveAt(index);

                // Reorder remaining sections
                for (int i = 0; i < config.TextSections.Count; i++)
                {
                    config.TextSections[i].Order = i;
                }
            }
        }
    }

    public int GetItemCount(QuestionItem question)
    {
        if (question.Configuration is TextQuestionConfiguration config)
        {
            return config.TextSections.Count;
        }
        return 0;
    }

    public List<string> Validate(QuestionItem question, string questionLabel)
    {
        var errors = new List<string>();

        if (question.Configuration is TextQuestionConfiguration config)
        {
            if (config.TextSections.Count == 0)
            {
                errors.Add($"{questionLabel} must have at least one text section");
            }
            else
            {
                for (int i = 0; i < config.TextSections.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(config.TextSections[i].TitleEnglish) &&
                        string.IsNullOrWhiteSpace(config.TextSections[i].TitleGerman))
                    {
                        errors.Add($"Text section {i + 1} in {questionLabel} requires a title (in English or German)");
                    }
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
