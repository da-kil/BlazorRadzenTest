using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for Goal question type.
/// Goal questions don't use template items - goals are added dynamically during workflow execution.
/// </summary>
public class GoalQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.Goal;

    public void InitializeQuestion(QuestionSection question)
    {
        // Goal questions use GoalConfiguration with ShowGoalSection flag
        // Goals themselves are added dynamically during workflow execution
        question.Configuration = new GoalConfiguration
        {
            ShowGoalSection = true // Default to showing the goal section
        };
    }

    public void AddItem(QuestionSection question)
    {
        // No-op: Goal questions don't use template items
        // Goals are added during questionnaire execution, not in the template
    }

    public void RemoveItem(QuestionSection question, int index)
    {
        // No-op: Goal questions don't use template items
    }

    public int GetItemCount(QuestionSection question)
    {
        // Always return 0 - Goal questions don't have template items
        return 0;
    }

    public void MoveItemUp(QuestionSection question, int index)
    {
        // No-op: Goal questions don't use template items
        // Goals are added during questionnaire execution, not in the template
    }

    public void MoveItemDown(QuestionSection question, int index)
    {
        // No-op: Goal questions don't use template items
        // Goals are added during questionnaire execution, not in the template
    }

    public List<string> Validate(QuestionSection question, string questionLabel)
    {
        var errors = new List<string>();

        // Only validate that title and description are present
        // No template items to validate
        if (string.IsNullOrWhiteSpace(question.TitleEnglish) && string.IsNullOrWhiteSpace(question.TitleGerman))
        {
            errors.Add($"{questionLabel} requires a title");
        }

        return errors;
    }

}
