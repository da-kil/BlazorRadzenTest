using ti8m.BeachBreak.Application.Query;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Mappers;
using Language = ti8m.BeachBreak.Core.Domain.QuestionConfiguration.Language;

namespace ti8m.BeachBreak.QueryApi.Services.Pdf;

public class PdfExportApplicationService : IPdfExportApplicationService
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly IQuestionnairePdfService pdfService;
    private readonly IBulkPdfExportService bulkService;
    private readonly IManagerAuthorizationService authorizationService;
    private readonly IEmployeeRoleService employeeRoleService;
    private readonly ILogger<PdfExportApplicationService> logger;

    public PdfExportApplicationService(
        IQueryDispatcher queryDispatcher,
        IQuestionnairePdfService pdfService,
        IBulkPdfExportService bulkService,
        IManagerAuthorizationService authorizationService,
        IEmployeeRoleService employeeRoleService,
        ILogger<PdfExportApplicationService> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.pdfService = pdfService;
        this.bulkService = bulkService;
        this.authorizationService = authorizationService;
        this.employeeRoleService = employeeRoleService;
        this.logger = logger;
    }

    public async Task<Result<PdfFileResult>> ExportSingleAsync(Guid userId, Guid assignmentId, Language language, CancellationToken ct)
    {
        logger.LogPdfExportStarting(assignmentId, userId);

        var assignmentResult = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentQuery(assignmentId));
        if (assignmentResult == null || !assignmentResult.Succeeded || assignmentResult.Payload == null)
            return Result<PdfFileResult>.Fail($"Assignment {assignmentId} not found", StatusCodes.Status404NotFound);

        var assignment = assignmentResult.Payload;

        if (assignment.WorkflowState != WorkflowState.Finalized)
        {
            logger.LogPdfExportNotFinalized(assignmentId, (int)assignment.WorkflowState);
            return Result<PdfFileResult>.Fail(
                $"Assignment {assignmentId} is not finalized (current state: {assignment.WorkflowState})",
                StatusCodes.Status409Conflict);
        }

        var userRole = await employeeRoleService.GetEmployeeRoleAsync(userId, ct);
        if (userRole == null)
            return Result<PdfFileResult>.Fail("User role not found", StatusCodes.Status401Unauthorized);

        var isElevated = userRole.ApplicationRole == ApplicationRole.HR
                      || userRole.ApplicationRole == ApplicationRole.HRLead
                      || userRole.ApplicationRole == ApplicationRole.Admin;

        if (!isElevated)
        {
            var isTeamLead = userRole.ApplicationRole == ApplicationRole.TeamLead;
            if (isTeamLead)
            {
                var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, assignmentId);
                if (!canAccess)
                {
                    logger.LogPdfExportUnauthorized(assignmentId, userId);
                    return Result<PdfFileResult>.Fail("Access denied", StatusCodes.Status403Forbidden);
                }
            }
            else
            {
                if (assignment.EmployeeId != userId)
                {
                    logger.LogPdfExportUnauthorized(assignmentId, userId);
                    return Result<PdfFileResult>.Fail("Access denied", StatusCodes.Status403Forbidden);
                }
            }
        }

        var response = await queryDispatcher.QueryAsync(new GetResponseByAssignmentIdQuery(assignmentId));

        var templateResult = await queryDispatcher.QueryAsync(new QuestionnaireTemplateQuery(assignment.TemplateId));
        if (templateResult == null || !templateResult.Succeeded || templateResult.Payload == null)
            return Result<PdfFileResult>.Fail($"Template {assignment.TemplateId} not found", StatusCodes.Status404NotFound);

        var pdfData = new QuestionnairePdfData(
            Assignment: PdfExportMapper.MapAssignmentToDto(assignment),
            Response: response != null
                ? PdfExportMapper.MapResponseToDto(response)
                : PdfExportMapper.BuildEmptyResponseDto(assignmentId, assignment.EmployeeId, assignment.TemplateId),
            Template: PdfExportMapper.MapTemplateToDto(templateResult.Payload),
            GeneratedAt: DateTime.UtcNow,
            Language: language);

        var bytes = await pdfService.GeneratePdfAsync(pdfData, ct);
        var fileName = PdfExportMapper.SanitizeFileName(
            $"{assignment.EmployeeName}_{assignment.TemplateName}_{assignment.FinalizedDate:yyyy-MM-dd}.pdf");

        logger.LogPdfExportCompleted(assignmentId, bytes.Length);

        return Result<PdfFileResult>.Success(new PdfFileResult(bytes, fileName, "application/pdf"));
    }

    public async Task<Result<PdfFileResult>> ExportBulkAsync(Guid userId, IReadOnlyList<Guid> assignmentIds, Language language, CancellationToken ct)
    {
        logger.LogBulkPdfExportStarting(assignmentIds.Count, userId);

        var userRole = await employeeRoleService.GetEmployeeRoleAsync(userId, ct);
        var isElevated = userRole != null && (
            userRole.ApplicationRole == ApplicationRole.HR
            || userRole.ApplicationRole == ApplicationRole.HRLead
            || userRole.ApplicationRole == ApplicationRole.Admin);

        var pdfDataItems = new List<QuestionnairePdfData>();

        foreach (var assignmentId in assignmentIds)
        {
            ct.ThrowIfCancellationRequested();

            var assignmentResult = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentQuery(assignmentId));
            if (assignmentResult == null || !assignmentResult.Succeeded || assignmentResult.Payload == null)
                continue;

            var assignment = assignmentResult.Payload;

            if (assignment.WorkflowState != WorkflowState.Finalized)
                continue;

            if (!isElevated)
            {
                var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, assignmentId);
                if (!canAccess)
                    continue;
            }

            var response = await queryDispatcher.QueryAsync(new GetResponseByAssignmentIdQuery(assignmentId));

            var templateResult = await queryDispatcher.QueryAsync(new QuestionnaireTemplateQuery(assignment.TemplateId));
            if (templateResult == null || !templateResult.Succeeded || templateResult.Payload == null)
                continue;

            pdfDataItems.Add(new QuestionnairePdfData(
                Assignment: PdfExportMapper.MapAssignmentToDto(assignment),
                Response: response != null
                    ? PdfExportMapper.MapResponseToDto(response)
                    : PdfExportMapper.BuildEmptyResponseDto(assignmentId, assignment.EmployeeId, assignment.TemplateId),
                Template: PdfExportMapper.MapTemplateToDto(templateResult.Payload),
                GeneratedAt: DateTime.UtcNow,
                Language: language));
        }

        if (pdfDataItems.Count == 0)
            return Result<PdfFileResult>.Fail("No finalized assignments found for the provided IDs", StatusCodes.Status400BadRequest);

        var zipBytes = await bulkService.GenerateBulkZipAsync(pdfDataItems, ct);
        var fileName = $"Questionnaire_Export_{DateTime.UtcNow:yyyy-MM-dd}.zip";

        logger.LogBulkPdfExportCompleted(pdfDataItems.Count, zipBytes.Length);

        return Result<PdfFileResult>.Success(new PdfFileResult(zipBytes, fileName, "application/zip"));
    }
}
