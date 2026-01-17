using System.Collections.Generic;

namespace ti8m.BeachBreak.Core.SourceGenerators.Models;

/// <summary>
/// Contains metadata about a query handler that implements one or more IQueryHandler{TQuery, TResponse} interfaces.
/// </summary>
public class QueryHandlerInfo
{
    /// <summary>The simple type name of the handler (e.g. "QuestionnaireTemplateQueryHandler")</summary>
    public string TypeName { get; }

    /// <summary>The fully qualified type name including namespace</summary>
    public string FullTypeName { get; }

    /// <summary>The namespace containing the handler type</summary>
    public string Namespace { get; }

    /// <summary>List of query/response mappings this handler implements</summary>
    public List<QueryHandlerMapping> HandledQueries { get; }

    /// <summary>True if this handler implements multiple query interfaces (consolidated pattern)</summary>
    public bool IsConsolidated { get; }

    public QueryHandlerInfo(string typeName, string fullTypeName, string @namespace, List<QueryHandlerMapping> handledQueries, bool isConsolidated)
    {
        TypeName = typeName;
        FullTypeName = fullTypeName;
        Namespace = @namespace;
        HandledQueries = handledQueries;
        IsConsolidated = isConsolidated;
    }
}