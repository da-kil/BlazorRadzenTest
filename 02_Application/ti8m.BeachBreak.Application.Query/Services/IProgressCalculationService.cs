using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Service for calculating role-based progress percentages for questionnaire responses.
/// Handles complex scenarios where sections have different completion roles (Employee, Manager, Both).
/// </summary>
public interface IProgressCalculationService
{
    /// <summary>
    /// Calculates progress for a questionnaire response based on the template structure.
    /// Only counts required sections and validates answers based on section type.
    /// </summary>
    /// <param name="template">The questionnaire template defining sections</param>
    /// <param name="sectionResponses">The response data structure: SectionId -> Role -> QuestionResponseValue</param>
    /// <returns>Progress calculation including employee, manager, and overall percentages</returns>
    ProgressCalculation Calculate(
        QuestionnaireTemplate template,
        Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> sectionResponses);
}
