using ti8m.BeachBreak.Application.Query.Models;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Registry of all projections that can be rebuilt
/// </summary>
public interface IProjectionRegistry
{
    /// <summary>
    /// Get all registered projections
    /// </summary>
    IReadOnlyList<ProjectionInfo> GetAllProjections();

    /// <summary>
    /// Get projection by name
    /// </summary>
    ProjectionInfo? GetProjection(string name);

    /// <summary>
    /// Check if projection can be rebuilt
    /// </summary>
    bool CanRebuild(string name);

    /// <summary>
    /// Get the document type for a projection name
    /// </summary>
    Type? GetDocumentType(string projectionName);

    /// <summary>
    /// Get the table name for a projection
    /// </summary>
    string? GetTableName(string projectionName);
}
