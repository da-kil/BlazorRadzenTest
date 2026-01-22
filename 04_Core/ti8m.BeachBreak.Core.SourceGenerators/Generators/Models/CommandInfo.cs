namespace ti8m.BeachBreak.Core.SourceGenerators.Models;

/// <summary>
/// Contains metadata about a command that implements ICommand{TResponse}.
/// </summary>
public class CommandInfo
{
    /// <summary>The simple type name of the command (e.g. "CreateQuestionnaireTemplateCommand")</summary>
    public string TypeName { get; }

    /// <summary>The fully qualified type name including namespace</summary>
    public string FullTypeName { get; }

    /// <summary>The response type from ICommand{TResponse} (e.g. "Result" or "Result{Guid}")</summary>
    public string ResponseType { get; }

    /// <summary>The namespace containing the command type</summary>
    public string Namespace { get; }

    public CommandInfo(string typeName, string fullTypeName, string responseType, string @namespace)
    {
        TypeName = typeName;
        FullTypeName = fullTypeName;
        ResponseType = responseType;
        Namespace = @namespace;
    }
}