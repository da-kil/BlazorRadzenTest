using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for Goal question type.
/// Goal questions don't use template items - goals are added dynamically during workflow execution.
/// </summary>
public class GoalQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.Goal;

    public void InitializeQuestion(QuestionItem question)
    {
        // Goal questions don't need template items
        // Goals are added dynamically during in-progress states by Employee/Manager
        // Just ensure Configuration dictionary exists
        question.Configuration ??= new Dictionary<string, object>();
    }

    public void AddItem(QuestionItem question)
    {
        // No-op: Goal questions don't use template items
        // Goals are added during questionnaire execution, not in the template
    }

    public void RemoveItem(QuestionItem question, int index)
    {
        // No-op: Goal questions don't use template items
    }

    public int GetItemCount(QuestionItem question)
    {
        // Always return 0 - Goal questions don't have template items
        return 0;
    }

    public List<string> Validate(QuestionItem question, string questionLabel)
    {
        var errors = new List<string>();

        // Only validate that title and description are present
        // No template items to validate
        if (string.IsNullOrWhiteSpace(question.Title))
        {
            errors.Add($"{questionLabel} requires a title");
        }

        return errors;
    }

    public string GetDefaultTitle()
    {
        return "Goal Management";
    }
}
