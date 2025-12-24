using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Core.Infrastructure.Services;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Application.Query.Mappers;
using ti8m.BeachBreak.Domain;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class QuestionnaireAssignmentQueryHandler :
    IQueryHandler<QuestionnaireAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignment>>>,
    IQueryHandler<QuestionnaireAssignmentQuery, Result<QuestionnaireAssignment>>,
    IQueryHandler<QuestionnaireEmployeeAssignmentListQuery, Result<IEnumerable<QuestionnaireAssignment>>>
{
    private readonly IQuestionnaireAssignmentRepository repository;
    private readonly IQuestionnaireTemplateRepository templateRepository;
    private readonly IEmployeeRepository employeeRepository;
    private readonly ILanguageContext languageContext;
    private readonly ILogger<QuestionnaireAssignmentQueryHandler> logger;

    public QuestionnaireAssignmentQueryHandler(
        IQuestionnaireAssignmentRepository repository,
        IQuestionnaireTemplateRepository templateRepository,
        IEmployeeRepository employeeRepository,
        ILanguageContext languageContext,
        ILogger<QuestionnaireAssignmentQueryHandler> logger)
    {
        this.repository = repository;
        this.templateRepository = templateRepository;
        this.employeeRepository = employeeRepository;
        this.languageContext = languageContext;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignment>>> HandleAsync(QuestionnaireAssignmentListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving all questionnaire assignments");

            var assignmentReadModels = await repository.GetAllAssignmentsAsync(cancellationToken);
            var assignments = await EnrichAssignmentsAsync(assignmentReadModels, cancellationToken);

            logger.LogInformation("Retrieved {AssignmentCount} assignments", assignments.Count());
            return Result<IEnumerable<QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all assignments");
            return Result<IEnumerable<QuestionnaireAssignment>>.Fail("Failed to retrieve assignments: " + ex.Message, 500);
        }
    }

    public async Task<Result<QuestionnaireAssignment>> HandleAsync(QuestionnaireAssignmentQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving assignment {AssignmentId}", query.Id);

            var assignmentReadModel = await repository.GetAssignmentByIdAsync(query.Id, cancellationToken);
            if (assignmentReadModel != null)
            {
                var assignments = await EnrichAssignmentsAsync(new[] { assignmentReadModel }, cancellationToken);
                var assignment = assignments.First();
                logger.LogInformation("Retrieved assignment {AssignmentId}", assignment.Id);
                return Result<QuestionnaireAssignment>.Success(assignment);
            }

            logger.LogWarning("Assignment {AssignmentId} not found", query.Id);
            return Result<QuestionnaireAssignment>.Fail($"Assignment {query.Id} not found", 404);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment {AssignmentId}", query.Id);
            return Result<QuestionnaireAssignment>.Fail("Failed to retrieve assignment: " + ex.Message, 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignment>>> HandleAsync(QuestionnaireEmployeeAssignmentListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving assignments for employee {EmployeeId}", query.EmployeeId);

            var assignmentReadModels = await repository.GetAssignmentsByEmployeeIdAsync(query.EmployeeId, cancellationToken);
            var assignments = await EnrichAssignmentsAsync(assignmentReadModels, cancellationToken);

            logger.LogInformation("Retrieved {AssignmentCount} assignments for employee {EmployeeId}", assignments.Count(), query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Success(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", query.EmployeeId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Fail("Failed to retrieve employee assignments: " + ex.Message, 500);
        }
    }

    /// <summary>
    /// Enriches assignments with template metadata (name and category) and employee names.
    /// Only fetches templates and employees referenced by the assignments to minimize data transfer.
    /// </summary>
    private async Task<IEnumerable<QuestionnaireAssignment>> EnrichAssignmentsAsync(
        IEnumerable<Projections.QuestionnaireAssignmentReadModel> readModels,
        CancellationToken cancellationToken)
    {
        var readModelsList = readModels.ToList();
        if (!readModelsList.Any())
        {
            return Enumerable.Empty<QuestionnaireAssignment>();
        }

        // Get unique template IDs from assignments
        var templateIds = readModelsList.Select(a => a.TemplateId).Distinct().ToList();

        // Fetch only the templates we need
        var templates = await templateRepository.GetByIdsAsync(templateIds, cancellationToken);
        var templateLookup = templates.ToDictionary(t => t.Id, t => (template: t, t.CategoryId));

        // Get user's preferred language
        var currentLanguageCode = await languageContext.GetCurrentLanguageCodeAsync();
        var currentLanguage = LanguageMapper.FromLanguageCode(currentLanguageCode);

        // Get unique employee IDs that need to be resolved
        var employeeIds = readModelsList
            .SelectMany(rm => new[] {
                rm.WithdrawnByEmployeeId,
                rm.EmployeeSubmittedByEmployeeId,
                rm.ManagerSubmittedByEmployeeId,
                rm.ReviewInitiatedByEmployeeId,
                rm.ManagerReviewFinishedByEmployeeId,
                rm.EmployeeReviewConfirmedByEmployeeId,
                rm.FinalizedByEmployeeId,
                rm.LastReopenedByEmployeeId
            })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        // Batch fetch employees if there are any to fetch
        var employeeLookup = new Dictionary<Guid, string>();
        if (employeeIds.Any())
        {
            var employees = await employeeRepository.GetEmployeesAsync(cancellationToken: cancellationToken);
            employeeLookup = employees
                .Where(e => employeeIds.Contains(e.Id))
                .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");
        }

        // Map and enrich
        return readModelsList.Select(readModel =>
        {
            var assignment = MapToQuestionnaireAssignment(readModel);

            // Denormalize template metadata
            if (templateLookup.TryGetValue(readModel.TemplateId, out var templateMetadata))
            {
                assignment.TemplateName = GetLocalizedTemplateName(templateMetadata.template, currentLanguage);
                assignment.TemplateCategoryId = templateMetadata.CategoryId;
            }
            else
            {
                // Template not found - log warning but don't fail
                logger.LogWarning("Template {TemplateId} not found for assignment {AssignmentId}",
                    readModel.TemplateId, readModel.Id);
                assignment.TemplateName = "Unknown Template";
                assignment.TemplateCategoryId = null;
            }

            // Resolve employee names
            if (readModel.WithdrawnByEmployeeId.HasValue &&
                employeeLookup.TryGetValue(readModel.WithdrawnByEmployeeId.Value, out var withdrawnByName))
            {
                assignment.WithdrawnByEmployeeName = withdrawnByName;
            }

            if (readModel.EmployeeSubmittedByEmployeeId.HasValue &&
                employeeLookup.TryGetValue(readModel.EmployeeSubmittedByEmployeeId.Value, out var employeeSubmittedByName))
            {
                assignment.EmployeeSubmittedByEmployeeName = employeeSubmittedByName;
            }

            if (readModel.ManagerSubmittedByEmployeeId.HasValue &&
                employeeLookup.TryGetValue(readModel.ManagerSubmittedByEmployeeId.Value, out var managerSubmittedByName))
            {
                assignment.ManagerSubmittedByEmployeeName = managerSubmittedByName;
            }

            if (readModel.ReviewInitiatedByEmployeeId.HasValue &&
                employeeLookup.TryGetValue(readModel.ReviewInitiatedByEmployeeId.Value, out var reviewInitiatedByName))
            {
                assignment.ReviewInitiatedByEmployeeName = reviewInitiatedByName;
            }

            if (readModel.ManagerReviewFinishedByEmployeeId.HasValue &&
                employeeLookup.TryGetValue(readModel.ManagerReviewFinishedByEmployeeId.Value, out var managerReviewFinishedByName))
            {
                assignment.ManagerReviewFinishedByEmployeeName = managerReviewFinishedByName;
            }

            if (readModel.EmployeeReviewConfirmedByEmployeeId.HasValue &&
                employeeLookup.TryGetValue(readModel.EmployeeReviewConfirmedByEmployeeId.Value, out var employeeReviewConfirmedByName))
            {
                assignment.EmployeeReviewConfirmedByEmployeeName = employeeReviewConfirmedByName;
            }

            if (readModel.FinalizedByEmployeeId.HasValue &&
                employeeLookup.TryGetValue(readModel.FinalizedByEmployeeId.Value, out var finalizedByName))
            {
                assignment.FinalizedByEmployeeName = finalizedByName;
            }

            if (readModel.LastReopenedByEmployeeId.HasValue &&
                employeeLookup.TryGetValue(readModel.LastReopenedByEmployeeId.Value, out var lastReopenedByName))
            {
                assignment.LastReopenedByEmployeeName = lastReopenedByName;
            }

            return assignment;
        });
    }

    private static QuestionnaireAssignment MapToQuestionnaireAssignment(Projections.QuestionnaireAssignmentReadModel readModel)
    {
        return new QuestionnaireAssignment
        {
            Id = readModel.Id,
            TemplateId = readModel.TemplateId,
            RequiresManagerReview = readModel.RequiresManagerReview,
            EmployeeId = readModel.EmployeeId,
            EmployeeName = readModel.EmployeeName,
            EmployeeEmail = readModel.EmployeeEmail,
            AssignedDate = readModel.AssignedDate,
            DueDate = readModel.DueDate,
            StartedDate = readModel.StartedDate,
            CompletedDate = readModel.CompletedDate,
            IsWithdrawn = readModel.IsWithdrawn,
            WithdrawnDate = readModel.WithdrawnDate,
            WithdrawnByEmployeeId = readModel.WithdrawnByEmployeeId,
            WithdrawalReason = readModel.WithdrawalReason,
            AssignedBy = readModel.AssignedBy,
            Notes = readModel.Notes,

            // Workflow properties
            WorkflowState = readModel.WorkflowState,
            SectionProgress = readModel.SectionProgress.Select(sp => new SectionProgressDto
            {
                SectionId = sp.SectionId,
                IsEmployeeCompleted = sp.IsEmployeeCompleted,
                IsManagerCompleted = sp.IsManagerCompleted,
                EmployeeCompletedDate = sp.EmployeeCompletedDate,
                ManagerCompletedDate = sp.ManagerCompletedDate
            }).ToList(),

            // Submission phase
            EmployeeSubmittedDate = readModel.EmployeeSubmittedDate,
            EmployeeSubmittedByEmployeeId = readModel.EmployeeSubmittedByEmployeeId,
            ManagerSubmittedDate = readModel.ManagerSubmittedDate,
            ManagerSubmittedByEmployeeId = readModel.ManagerSubmittedByEmployeeId,

            // Review phase
            ReviewInitiatedDate = readModel.ReviewInitiatedDate,
            ReviewInitiatedByEmployeeId = readModel.ReviewInitiatedByEmployeeId,
            ManagerReviewFinishedDate = readModel.ManagerReviewFinishedDate,
            ManagerReviewFinishedByEmployeeId = readModel.ManagerReviewFinishedByEmployeeId,
            ManagerReviewSummary = readModel.ManagerReviewSummary,
            EmployeeReviewConfirmedDate = readModel.EmployeeReviewConfirmedDate,
            EmployeeReviewConfirmedByEmployeeId = readModel.EmployeeReviewConfirmedByEmployeeId,
            EmployeeReviewComments = readModel.EmployeeReviewComments,

            // Final state
            FinalizedDate = readModel.FinalizedDate,
            FinalizedByEmployeeId = readModel.FinalizedByEmployeeId,
            ManagerFinalNotes = readModel.ManagerFinalNotes,
            IsLocked = readModel.IsLocked,

            // Reopen tracking (audit trail)
            LastReopenedDate = readModel.LastReopenedDate,
            LastReopenedByEmployeeId = readModel.LastReopenedByEmployeeId,
            LastReopenedByRole = readModel.LastReopenedByRole,
            LastReopenReason = readModel.LastReopenReason
        };
    }

    private static string GetLocalizedTemplateName(Projections.QuestionnaireTemplateReadModel template, Models.Language language)
    {
        return language == Models.Language.German ? template.NameGerman : template.NameEnglish;
    }
}
