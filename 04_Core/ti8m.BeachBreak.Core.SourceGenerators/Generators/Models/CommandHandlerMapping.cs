namespace ti8m.BeachBreak.Core.SourceGenerators.Models;

/// <summary>
/// Represents a mapping between a command type and its response type for a specific handler.
/// Used to track which ICommandHandler{TCommand, TResponse} interfaces a handler class implements.
/// </summary>
public class CommandHandlerMapping
{
    /// <summary>The command type (e.g. "CreateQuestionnaireTemplateCommand")</summary>
    public string CommandType { get; }

    /// <summary>The response type (e.g. "Result" or "Result{Guid}")</summary>
    public string ResponseType { get; }

    public CommandHandlerMapping(string commandType, string responseType)
    {
        CommandType = commandType;
        ResponseType = responseType;
    }
}