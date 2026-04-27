using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

public class MultipleChoiceQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.MultipleChoice;

    public void InitializeQuestion(QuestionSection question)
    {
        question.Configuration = new MultipleChoiceConfiguration
        {
            Questions =
            [
                new MultipleChoiceQuestion
                {
                    Key = "question_1",
                    Order = 0,
                    MinSelections = 1,
                    MaxSelections = 1,
                    Choices = [new ChoiceOption("choice_1", "", "", 0)]
                }
            ]
        };
    }

    public void AddItem(QuestionSection question)
    {
        if (question.Configuration is not MultipleChoiceConfiguration config) return;
        var nextOrder = config.Questions.Count > 0 ? config.Questions.Max(q => q.Order) + 1 : 0;
        config.Questions.Add(new MultipleChoiceQuestion
        {
            Key = $"question_{config.Questions.Count + 1}",
            Order = nextOrder,
            MinSelections = 1,
            MaxSelections = 1,
            Choices = [new ChoiceOption("choice_1", "", "", 0)]
        });
    }

    public void RemoveItem(QuestionSection question, int index)
    {
        if (question.Configuration is not MultipleChoiceConfiguration config) return;
        if (index >= 0 && index < config.Questions.Count)
        {
            config.Questions.RemoveAt(index);
            for (int i = 0; i < config.Questions.Count; i++)
                config.Questions[i].Order = i;
        }
    }

    public int GetItemCount(QuestionSection question) =>
        question.Configuration is MultipleChoiceConfiguration config ? config.Questions.Count : 0;

    public void MoveItemUp(QuestionSection question, int index)
    {
        if (question.Configuration is not MultipleChoiceConfiguration config) return;
        if (index > 0 && index < config.Questions.Count)
        {
            (config.Questions[index], config.Questions[index - 1]) = (config.Questions[index - 1], config.Questions[index]);
            config.Questions[index].Order = index;
            config.Questions[index - 1].Order = index - 1;
        }
    }

    public void MoveItemDown(QuestionSection question, int index)
    {
        if (question.Configuration is not MultipleChoiceConfiguration config) return;
        if (index >= 0 && index < config.Questions.Count - 1)
        {
            (config.Questions[index], config.Questions[index + 1]) = (config.Questions[index + 1], config.Questions[index]);
            config.Questions[index].Order = index;
            config.Questions[index + 1].Order = index + 1;
        }
    }

    public List<string> Validate(QuestionSection question, string questionLabel)
    {
        var errors = new List<string>();

        if (question.Configuration is not MultipleChoiceConfiguration config)
            return errors;

        if (!config.Questions.Any())
        {
            errors.Add($"'{questionLabel}' must have at least one question");
            return errors;
        }

        for (int qi = 0; qi < config.Questions.Count; qi++)
        {
            var q = config.Questions[qi];
            var qLabel = $"Question {qi + 1} in '{questionLabel}'";

            if (!q.Choices.Any())
            {
                errors.Add($"{qLabel} must have at least one choice");
                continue;
            }

            for (int ci = 0; ci < q.Choices.Count; ci++)
            {
                if (string.IsNullOrWhiteSpace(q.Choices[ci].LabelEnglish) && string.IsNullOrWhiteSpace(q.Choices[ci].LabelGerman))
                    errors.Add($"Choice {ci + 1} in {qLabel} requires a label");
            }

            if (q.MaxSelections < q.MinSelections)
                errors.Add($"{qLabel}: Max selections ({q.MaxSelections}) must be >= min ({q.MinSelections})");

            if (q.MaxSelections > q.Choices.Count)
                errors.Add($"{qLabel}: Max selections ({q.MaxSelections}) cannot exceed number of choices ({q.Choices.Count})");
        }

        return errors;
    }
}
