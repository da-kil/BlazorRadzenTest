using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.QueryApi.Dto;
using ti8m.BeachBreak.QueryApi.Services.Pdf;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/pdf-export")]
[Authorize]
public class PdfExportController : BaseController
{
    private readonly IPdfExportApplicationService applicationService;
    private readonly UserContext userContext;
    private readonly ILogger<PdfExportController> logger;

    public PdfExportController(
        IPdfExportApplicationService applicationService,
        UserContext userContext,
        ILogger<PdfExportController> logger)
    {
        this.applicationService = applicationService;
        this.userContext = userContext;
        this.logger = logger;
    }

    /// <summary>
    /// Exports a finalized questionnaire as a PDF.
    /// Employees can export their own; TeamLeads their direct reports'; HR+ anyone's.
    /// </summary>
    [HttpGet("assignments/{assignmentId:guid}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAssignmentPdf(Guid assignmentId, [FromQuery] Language language, CancellationToken ct)
    {
        if (!Guid.TryParse(userContext.Id, out var userId))
            return Unauthorized("User identification failed");

        var result = await applicationService.ExportSingleAsync(userId, assignmentId, language, ct);
        if (!result.Succeeded)
            return Problem(detail: result.Message, statusCode: result.StatusCode);

        return File(result.Payload!.Bytes, result.Payload!.ContentType, result.Payload!.FileName);
    }

    /// <summary>
    /// Exports multiple finalized questionnaires as a ZIP of PDFs.
    /// TeamLead+ access required.
    /// </summary>
    [HttpPost("assignments/bulk")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportBulkPdf([FromBody] BulkPdfExportRequestDto request, CancellationToken ct)
    {
        if (!Guid.TryParse(userContext.Id, out var userId))
            return Unauthorized("User identification failed");

        if (request.AssignmentIds.Count == 0)
            return BadRequest("No assignment IDs provided");

        if (request.AssignmentIds.Count > 50)
            return BadRequest("Maximum 50 assignments per bulk export");

        var result = await applicationService.ExportBulkAsync(userId, request.AssignmentIds, request.Language, ct);
        if (!result.Succeeded)
            return Problem(detail: result.Message, statusCode: result.StatusCode);

        return File(result.Payload!.Bytes, result.Payload!.ContentType, result.Payload!.FileName);
    }
}
