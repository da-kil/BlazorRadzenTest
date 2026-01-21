using System.Collections.Generic;

namespace ti8m.BeachBreak.Core.SourceGenerators.Models;

/// <summary>
/// Contains metadata about a command handler that implements one or more ICommandHandler{TCommand, TResponse} interfaces.
/// </summary>
public class HandlerInfo
{
    /// <summary>The simple type name of the handler (e.g. "QuestionnaireTemplateCommandHandler")</summary>
    public string TypeName { get; }

    /// <summary>The fully qualified type name including namespace</summary>
    public string FullTypeName { get; }

    /// <summary>The namespace containing the handler type</summary>
    public string Namespace { get; }

    /// <summary>List of command/response mappings this handler implements</summary>
    public List<CommandHandlerMapping> HandledCommands { get; }

    /// <summary>True if this handler implements multiple command interfaces (consolidated pattern)</summary>
    public bool IsConsolidated { get; }

    public HandlerInfo(string typeName, string fullTypeName, string @namespace, List<CommandHandlerMapping> handledCommands, bool isConsolidated)
    {
        TypeName = typeName;
        FullTypeName = fullTypeName;
        Namespace = @namespace;
        HandledCommands = handledCommands;
        IsConsolidated = isConsolidated;
    }
}