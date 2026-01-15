using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;

/// <summary>
/// Strongly-typed questionnaire response for the Application.Query layer.
/// Now uses the same strongly-typed structure as the ReadModel for consistency and type safety.
/// </summary>
public class QuestionnaireResponse
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Strongly-typed section responses: SectionId -> CompletionRole -> QuestionResponseValue
    /// This matches the ReadModel structure and eliminates the need for object-based conversions.
    /// </summary>
    public Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> SectionResponses { get; set; } = new();

    public int ProgressPercentage { get; set; }
}