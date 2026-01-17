namespace ti8m.BeachBreak.Core.SourceGenerators.Models;

/// <summary>
/// Contains metadata about a query that implements IQuery{TResponse}.
/// </summary>
public class QueryInfo
{
    /// <summary>The simple type name of the query (e.g. "QuestionnaireTemplateListQuery")</summary>
    public string TypeName { get; }

    /// <summary>The fully qualified type name including namespace</summary>
    public string FullTypeName { get; }

    /// <summary>The response type from IQuery{TResponse} (e.g. "Result{IEnumerable{QuestionnaireTemplate}}" or "QuestionnaireResponse")</summary>
    public string ResponseType { get; }

    /// <summary>The namespace containing the query type</summary>
    public string Namespace { get; }

    public QueryInfo(string typeName, string fullTypeName, string responseType, string @namespace)
    {
        TypeName = typeName;
        FullTypeName = fullTypeName;
        ResponseType = responseType;
        Namespace = @namespace;
    }
}