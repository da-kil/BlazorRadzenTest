namespace ti8m.BeachBreak.Core.SourceGenerators.Models;

/// <summary>
/// Represents a mapping between a query type and its response type for a specific handler.
/// Used to track which IQueryHandler{TQuery, TResponse} interfaces a handler class implements.
/// </summary>
public class QueryHandlerMapping
{
    /// <summary>The query type (e.g. "QuestionnaireTemplateListQuery")</summary>
    public string QueryType { get; }

    /// <summary>The response type (e.g. "Result{IEnumerable{QuestionnaireTemplate}}" or "QuestionnaireResponse")</summary>
    public string ResponseType { get; }

    public QueryHandlerMapping(string queryType, string responseType)
    {
        QueryType = queryType;
        ResponseType = responseType;
    }
}