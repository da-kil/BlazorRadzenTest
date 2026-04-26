using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for Binary (Yes/No) question type.
/// Binary questions always have exactly two radio button options — no template items to manage.
/// </summary>
public class BinaryQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.Binary;

    public void InitializeQuestion(QuestionSection question)
    {
        question.Configuration = new BinaryConfiguration
        {
            OptionALabelEnglish = "Yes",
            OptionALabelGerman = "Ja",
            OptionBLabelEnglish = "No",
            OptionBLabelGerman = "Nein",
            IsRequired = false
        };
    }

    public void AddItem(QuestionSection question) { }
    public void RemoveItem(QuestionSection question, int index) { }
    public int GetItemCount(QuestionSection question) => 0;
    public void MoveItemUp(QuestionSection question, int index) { }
    public void MoveItemDown(QuestionSection question, int index) { }

    public List<string> Validate(QuestionSection question, string questionLabel)
    {
        var errors = new List<string>();

        if (question.Configuration is not BinaryConfiguration)
            errors.Add($"'{questionLabel}' has Binary type but invalid configuration");

        return errors;
    }
}
