using ti8m.BeachBreak.Core.Infrastructure.Services;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Application.Query.Mappers;
using ti8m.BeachBreak.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query handler to retrieve available predecessor questionnaires for goal rating.
/// Returns finalized questionnaires for the same employee and category that have goals.
/// Queries QuestionnaireResponse for actual goal storage (not QuestionnaireAssignment).
/// </summary>
public class GetAvailablePredecessorsQueryHandler
    : IQueryHandler<GetAvailablePredecessorsQuery, Result<IEnumerable<AvailablePredecessorDto>>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly IQuestionnaireTemplateRepository templateRepository;
    private readonly IQuestionnaireResponseRepository responseRepository;
    private readonly ILanguageContext languageContext;

    public GetAvailablePredecessorsQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IQuestionnaireTemplateRepository templateRepository,
        IQuestionnaireResponseRepository responseRepository,
        ILanguageContext languageContext)
    {
        this.assignmentRepository = assignmentRepository;
        this.templateRepository = templateRepository;
        this.responseRepository = responseRepository;
        this.languageContext = languageContext;
    }

    public async Task<Result<IEnumerable<AvailablePredecessorDto>>> HandleAsync(
        GetAvailablePredecessorsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Get current assignment to determine employee and category
        var currentAssignment = await assignmentRepository.GetAssignmentByIdAsync(
            query.AssignmentId, cancellationToken);

        if (currentAssignment == null)
        {
            return Result<IEnumerable<AvailablePredecessorDto>>.Fail(
                $"Assignment {query.AssignmentId} not found", 404);
        }

        // SECURITY: Validate that the assignment belongs to the requesting user
        if (currentAssignment.EmployeeId != query.RequestingUserId)
        {
            return Result<IEnumerable<AvailablePredecessorDto>>.Fail(
                "You do not have permission to access this assignment", 403);
        }

        // Get current template to determine category
        var currentTemplate = await templateRepository.GetByIdAsync(
            currentAssignment.TemplateId, cancellationToken);

        if (currentTemplate == null)
        {
            return Result<IEnumerable<AvailablePredecessorDto>>.Fail(
                $"Template {currentAssignment.TemplateId} not found", 404);
        }

        // Get all assignments for the requesting user (already validated as owner)
        var employeeAssignments = await assignmentRepository.GetAssignmentsByEmployeeIdAsync(
            query.RequestingUserId, cancellationToken);

        // Get user's preferred language once
        var currentLanguageCode = await languageContext.GetCurrentLanguageCodeAsync();
        var currentLanguage = LanguageMapper.FromLanguageCode(currentLanguageCode);

        // Filter for available predecessors
        var availablePredecessors = new List<AvailablePredecessorDto>();

        foreach (var assignment in employeeAssignments)
        {
            // Skip the current assignment
            if (assignment.Id == query.AssignmentId)
                continue;

            // Business rule: Must be finalized
            if (assignment.WorkflowState != WorkflowState.Finalized)
                continue;

            // Get template to check category
            var template = await templateRepository.GetByIdAsync(
                assignment.TemplateId, cancellationToken);

            if (template == null)
                continue;

            // Business rule: Must be same category
            if (template.CategoryId != currentTemplate.CategoryId)
                continue;


            // Business rule: Must have ANY goals
            // Query QuestionnaireResponse for actual goal storage (not GoalsByQuestion which is empty)
            var response = await responseRepository.GetByAssignmentIdAsync(
                assignment.Id, cancellationToken);

            if (response == null)
                continue;

            // Extract all goal responses from all sections and roles
            var goalResponses = response.SectionResponses.Values
                .SelectMany(roleDict => roleDict.Values)
                .SelectMany(questionDict => questionDict.Values)
                .OfType<QuestionResponseValue.GoalResponse>()
                .Where(gr => gr.Goals != null && gr.Goals.Any())
                .ToList();

            if (!goalResponses.Any())
            {
                continue;
            }

            // Count total goals across all sections and roles
            var totalGoals = goalResponses.Sum(gr => gr.Goals.Count);

            // Add to available predecessors
            availablePredecessors.Add(new AvailablePredecessorDto
            {
                AssignmentId = assignment.Id,
                TemplateName = GetLocalizedTemplateName(template, currentLanguage),
                AssignedDate = assignment.AssignedDate,
                CompletedDate = assignment.CompletedDate,
                GoalCount = totalGoals
            });
        }

        // Order by completion date descending (most recent first)
        var orderedPredecessors = availablePredecessors
            .OrderByDescending(p => p.CompletedDate)
            .ToList();

        return Result<IEnumerable<AvailablePredecessorDto>>.Success(orderedPredecessors);
    }

    private static string GetLocalizedTemplateName(Projections.QuestionnaireTemplateReadModel template, Models.Language language)
    {
        return language == Models.Language.German ? template.NameGerman : template.NameEnglish;
    }
}
