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
    /// Initializes a new section with default configuration (Section IS the question)
    /// </summary>
    void InitializeQuestion(QuestionSection section);

    /// <summary>
    /// Adds a new item to the section (evaluation, goal category, or text section)
    /// </summary>
    void AddItem(QuestionSection section);

    /// <summary>
    /// Removes an item from the section at the specified index
    /// </summary>
    void RemoveItem(QuestionSection section, int index);

    /// <summary>
    /// Gets the count of items in this section
    /// </summary>
    int GetItemCount(QuestionSection section);

    /// <summary>
    /// Moves an item up in the list (decreases order)
    /// </summary>
    /// <param name="section">The section containing the item</param>
    /// <param name="index">Index of the item to move up</param>
    void MoveItemUp(QuestionSection section, int index);

    /// <summary>
    /// Moves an item down in the list (increases order)
    /// </summary>
    /// <param name="section">The section containing the item</param>
    /// <param name="index">Index of the item to move down</param>
    void MoveItemDown(QuestionSection section, int index);

    /// <summary>
    /// Validates the section configuration
    /// </summary>
    /// <returns>List of validation error messages (empty if valid)</returns>
    List<string> Validate(QuestionSection section, string questionLabel);

}
