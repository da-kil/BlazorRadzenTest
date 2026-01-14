using ti8m.BeachBreak.Application.Query.Projections.Models;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query to get available employee feedback records that can be linked to a questionnaire assignment.
/// Returns all non-deleted feedback for the assignment's employee.
/// </summary>
public record GetAvailableEmployeeFeedbackQuery(
    Guid AssignmentId) : IQuery<Result<List<LinkedEmployeeFeedbackDto>>>;
