using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

public class BinaryQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.Binary;

    public void InitializeQuestion(QuestionSection question)
    {
        question.Configuration = new BinaryConfiguration
        {
            Items =
            [
                new BinaryItem { Key = Guid.NewGuid().ToString("N")[..8], Order = 0 }
            ]
        };
    }

    public void AddItem(QuestionSection question)
    {
        if (question.Configuration is not BinaryConfiguration config) return;
        config.Items.Add(new BinaryItem
        {
            Key = Guid.NewGuid().ToString("N")[..8],
            Order = config.Items.Count
        });
    }

    public void RemoveItem(QuestionSection question, int index)
    {
        if (question.Configuration is not BinaryConfiguration config) return;
        if (index >= 0 && index < config.Items.Count)
        {
            config.Items.RemoveAt(index);
            for (var i = 0; i < config.Items.Count; i++) config.Items[i].Order = i;
        }
    }

    public int GetItemCount(QuestionSection question) =>
        question.Configuration is BinaryConfiguration config ? config.Items.Count : 0;

    public void MoveItemUp(QuestionSection question, int index)
    {
        if (question.Configuration is not BinaryConfiguration config) return;
        if (index > 0 && index < config.Items.Count)
        {
            (config.Items[index], config.Items[index - 1]) = (config.Items[index - 1], config.Items[index]);
            for (var i = 0; i < config.Items.Count; i++) config.Items[i].Order = i;
        }
    }

    public void MoveItemDown(QuestionSection question, int index)
    {
        if (question.Configuration is not BinaryConfiguration config) return;
        if (index >= 0 && index < config.Items.Count - 1)
        {
            (config.Items[index], config.Items[index + 1]) = (config.Items[index + 1], config.Items[index]);
            for (var i = 0; i < config.Items.Count; i++) config.Items[i].Order = i;
        }
    }

    public List<string> Validate(QuestionSection question, string questionLabel)
    {
        var errors = new List<string>();

        if (question.Configuration is not BinaryConfiguration config)
        {
            errors.Add($"'{questionLabel}' has Binary type but invalid configuration");
            return errors;
        }

        if (config.Items.Count == 0)
        {
            errors.Add($"'{questionLabel}' must have at least one binary item");
            return errors;
        }

        for (int i = 0; i < config.Items.Count; i++)
        {
            var item = config.Items[i];
            if (string.IsNullOrWhiteSpace(item.TitleEnglish) && string.IsNullOrWhiteSpace(item.TitleGerman))
                errors.Add($"Binary item {i + 1} in '{questionLabel}' requires a question title");
        }

        return errors;
    }
}
