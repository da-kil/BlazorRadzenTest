using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for MultipleChoice question type.
/// Manages a list of choice options with configurable min/max selection constraints.
/// </summary>
public class MultipleChoiceQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.MultipleChoice;

    public void InitializeQuestion(QuestionSection question)
    {
        question.Configuration = new MultipleChoiceConfiguration
        {
            Choices = new List<ChoiceOption>
            {
                new ChoiceOption("choice_1", "", "", 0)
            },
            MinSelections = 1,
            MaxSelections = 1
        };
    }

    public void AddItem(QuestionSection question)
    {
        if (question.Configuration is MultipleChoiceConfiguration config)
        {
            var nextOrder = config.Choices.Count > 0 ? config.Choices.Max(c => c.Order) + 1 : 0;
            config.Choices.Add(new ChoiceOption(
                $"choice_{config.Choices.Count + 1}",
                "",
                "",
                nextOrder));
        }
    }

    public void RemoveItem(QuestionSection question, int index)
    {
        if (question.Configuration is MultipleChoiceConfiguration config)
        {
            if (index >= 0 && index < config.Choices.Count)
            {
                config.Choices.RemoveAt(index);
                for (int i = 0; i < config.Choices.Count; i++)
                    config.Choices[i].Order = i;

                // Clamp MaxSelections to available choices
                if (config.MaxSelections > config.Choices.Count)
                    config.MaxSelections = Math.Max(config.MinSelections, config.Choices.Count);
            }
        }
    }

    public int GetItemCount(QuestionSection question)
    {
        return question.Configuration is MultipleChoiceConfiguration config ? config.Choices.Count : 0;
    }

    public void MoveItemUp(QuestionSection question, int index)
    {
        if (question.Configuration is MultipleChoiceConfiguration config)
        {
            if (index > 0 && index < config.Choices.Count)
            {
                (config.Choices[index], config.Choices[index - 1]) = (config.Choices[index - 1], config.Choices[index]);
                config.Choices[index].Order = index;
                config.Choices[index - 1].Order = index - 1;
            }
        }
    }

    public void MoveItemDown(QuestionSection question, int index)
    {
        if (question.Configuration is MultipleChoiceConfiguration config)
        {
            if (index >= 0 && index < config.Choices.Count - 1)
            {
                (config.Choices[index], config.Choices[index + 1]) = (config.Choices[index + 1], config.Choices[index]);
                config.Choices[index].Order = index;
                config.Choices[index + 1].Order = index + 1;
            }
        }
    }

    public List<string> Validate(QuestionSection question, string questionLabel)
    {
        var errors = new List<string>();

        if (question.Configuration is not MultipleChoiceConfiguration config)
            return errors;

        if (!config.Choices.Any())
        {
            errors.Add($"{questionLabel} must have at least one choice option");
            return errors;
        }

        for (int i = 0; i < config.Choices.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(config.Choices[i].LabelEnglish) && string.IsNullOrWhiteSpace(config.Choices[i].LabelGerman))
                errors.Add($"Choice {i + 1} in '{questionLabel}' requires a label");
        }

        if (config.MaxSelections < config.MinSelections)
            errors.Add($"'{questionLabel}': Max selections ({config.MaxSelections}) must be >= min selections ({config.MinSelections})");

        if (config.MaxSelections > config.Choices.Count)
            errors.Add($"'{questionLabel}': Max selections ({config.MaxSelections}) cannot exceed number of choices ({config.Choices.Count})");

        return errors;
    }
}
