using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Factory for creating appropriate question type handlers.
/// Uses Strategy Pattern to provide the right handler for each question type.
/// </summary>
public class QuestionHandlerFactory
{
    private readonly Dictionary<QuestionType, IQuestionTypeHandler> handlers;

    public QuestionHandlerFactory(
        AssessmentQuestionHandler assessmentHandler,
        TextQuestionHandler textQuestionHandler)
    {
        handlers = new Dictionary<QuestionType, IQuestionTypeHandler>
        {
            { QuestionType.Assessment, assessmentHandler },
            { QuestionType.TextQuestion, textQuestionHandler }
        };
    }

    /// <summary>
    /// Gets the handler for the specified question type
    /// </summary>
    public IQuestionTypeHandler GetHandler(QuestionType type)
    {
        if (handlers.TryGetValue(type, out var handler))
        {
            return handler;
        }

        throw new ArgumentException($"No handler found for question type: {type}", nameof(type));
    }

    /// <summary>
    /// Gets the handler for a question (convenience method)
    /// </summary>
    public IQuestionTypeHandler GetHandler(QuestionItem question)
    {
        return GetHandler(question.Type);
    }

    /// <summary>
    /// Checks if a handler exists for the given question type
    /// </summary>
    public bool HasHandler(QuestionType type)
    {
        return handlers.ContainsKey(type);
    }

    /// <summary>
    /// Gets all supported question types
    /// </summary>
    public IEnumerable<QuestionType> GetSupportedTypes()
    {
        return handlers.Keys;
    }
}
