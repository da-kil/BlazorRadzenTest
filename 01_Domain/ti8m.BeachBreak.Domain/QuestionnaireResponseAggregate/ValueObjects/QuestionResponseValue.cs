using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

/// <summary>
/// Discriminated union representing different types of question responses.
/// Replaces Dictionary&lt;string, object&gt; with compile-time type safety.
/// </summary>
public abstract partial class QuestionResponseValue : ValueObject
{
}
