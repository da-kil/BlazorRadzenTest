namespace ti8m.BeachBreak.Core.SourceGenerators.Models;

/// <summary>
/// Contains metadata about a repository that implements IRepository interface.
/// </summary>
public class RepositoryInfo
{
    /// <summary>The simple type name of the repository implementation (e.g. "CategoryRepository")</summary>
    public string TypeName { get; }

    /// <summary>The fully qualified type name including namespace</summary>
    public string FullTypeName { get; }

    /// <summary>The interface type this repository implements (e.g. "ICategoryRepository")</summary>
    public string InterfaceType { get; }

    /// <summary>The namespace containing the repository implementation</summary>
    public string Namespace { get; }

    /// <summary>Whether this repository class is marked as internal</summary>
    public bool IsInternal { get; }

    public RepositoryInfo(string typeName, string fullTypeName, string interfaceType, string @namespace, bool isInternal)
    {
        TypeName = typeName;
        FullTypeName = fullTypeName;
        InterfaceType = interfaceType;
        Namespace = @namespace;
        IsInternal = isInternal;
    }
}