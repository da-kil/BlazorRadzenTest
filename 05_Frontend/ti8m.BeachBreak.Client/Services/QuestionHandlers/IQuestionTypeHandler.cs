using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Strategy interface for handling different question types.
/// Each question type (Assessment, Goal, TextQuestion) has its own handler.
/// </summary>
public interface IQuestionTypeHandler
{
    /// <summary>
    /// The question type this handler supports
    /// </summary>
    QuestionType SupportedType { get; }

    /// <summary>
    /// Initializes a new question with default configuration
    /// </summary>
    void InitializeQuestion(QuestionItem question);

    /// <summary>
    /// Adds a new item to the question (competency, goal category, or text section)
    /// </summary>
    void AddItem(QuestionItem question);

    /// <summary>
    /// Removes an item from the question at the specified index
    /// </summary>
    void RemoveItem(QuestionItem question, int index);

    /// <summary>
    /// Gets the count of items in this question
    /// </summary>
    int GetItemCount(QuestionItem question);

    /// <summary>
    /// Validates the question configuration
    /// </summary>
    /// <returns>List of validation error messages (empty if valid)</returns>
    List<string> Validate(QuestionItem question, string questionLabel);

    /// <summary>
    /// Gets the default question title for this type
    /// </summary>
    string GetDefaultTitle();
}
