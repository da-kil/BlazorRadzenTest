using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for EmployeeFeedback question type.
/// Feedback questions don't use template items - feedback records are linked during workflow initialization.
/// </summary>
public class EmployeeFeedbackQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.EmployeeFeedback;

    public void InitializeQuestion(QuestionSection question)
    {
        // Employee Feedback questions use EmployeeFeedbackConfiguration with ShowFeedbackSection flag
        // Feedback records are linked during initialization phase
        question.Configuration = new EmployeeFeedbackConfiguration
        {
            ShowFeedbackSection = true // Default to showing the feedback section
        };
    }

    public void AddItem(QuestionSection question)
    {
        // No-op: Feedback questions don't use template items
        // Feedback records are linked during initialization, not in the template
    }

    public void RemoveItem(QuestionSection question, int index)
    {
        // No-op: Feedback questions don't use template items
    }

    public int GetItemCount(QuestionSection question)
    {
        // Always return 0 - Feedback questions don't have template items
        return 0;
    }

    public void MoveItemUp(QuestionSection question, int index)
    {
        // No-op: Feedback questions don't use template items
    }

    public void MoveItemDown(QuestionSection question, int index)
    {
        // No-op: Feedback questions don't use template items
    }

    public List<string> Validate(QuestionSection question, string questionLabel)
    {
        var errors = new List<string>();

        // Only validate that title is present (description is optional)
        // No template items to validate
        if (string.IsNullOrWhiteSpace(question.TitleEnglish) && string.IsNullOrWhiteSpace(question.TitleGerman))
        {
            errors.Add($"{questionLabel} requires a title");
        }

        return errors;
    }
}
