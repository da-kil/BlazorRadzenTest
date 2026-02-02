using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;
using ti8m.BeachBreak.Application.Command.Models;
using ti8m.BeachBreak.Application.Command.Services;
using ti8m.BeachBreak.CommandApi.Authorization;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.CommandApi.Mappers;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/assignments")]
[Authorize]
public class AssignmentsController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly UserContext userContext;
    private readonly ILogger<AssignmentsController> logger;
    private readonly ICommandAuthorizationService commandAuthorizationService;
    private readonly IManagerAuthorizationService authorizationService;
    private readonly IEmployeeRoleService employeeRoleService;
    private readonly IQuestionSectionMapper questionSectionMapper;
    private readonly Services.QuestionResponseMappingService questionResponseMappingService;

    public AssignmentsController(
        ICommandDispatcher commandDispatcher,
        UserContext userContext,
        ILogger<AssignmentsController> logger,
        ICommandAuthorizationService commandAuthorizationService,
        IManagerAuthorizationService authorizationService,
        IEmployeeRoleService employeeRoleService,
        IQuestionSectionMapper questionSectionMapper,
        Services.QuestionResponseMappingService questionResponseMappingService)
    {
        this.commandDispatcher = commandDispatcher;
        this.userContext = userContext;
        this.logger = logger;
        this.commandAuthorizationService = commandAuthorizationService;
        this.authorizationService = authorizationService;
        this.employeeRoleService = employeeRoleService;
        this.questionSectionMapper = questionSectionMapper;
        this.questionResponseMappingService = questionResponseMappingService;
    }

    /// <summary>
    /// Creates bulk assignments for any employees. HR/Admin only.
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Policy = "HR")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateBulkAssignments([FromBody] CreateBulkAssignmentsDto bulkAssignmentDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        if (bulkAssignmentDto.EmployeeAssignments == null || !bulkAssignmentDto.EmployeeAssignments.Any())
            return CreateResponse(Result.Fail("At least one employee assignment is required", 400));

        // Get current user ID
        if (!userContext.TryGetUserId(out var userId, out var errorMessage))
        {
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var employeeAssignments = bulkAssignmentDto.EmployeeAssignments
            .Select(e => new EmployeeAssignmentData(
                e.EmployeeId,
                e.EmployeeName,
                e.EmployeeEmail))
            .ToList();

        var command = new CreateBulkAssignmentsCommand(
            bulkAssignmentDto.TemplateId,
            ProcessTypeMapper.MapToDomain(bulkAssignmentDto.ProcessType),
            employeeAssignments,
            bulkAssignmentDto.DueDate,
            userId,
            bulkAssignmentDto.Notes);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Creates bulk assignments for a manager's direct reports only. TeamLead role.
    /// Validates that all employees are direct reports of the authenticated manager.
    /// </summary>
    [HttpPost("manager/bulk")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateManagerBulkAssignments([FromBody] CreateBulkAssignmentsDto bulkAssignmentDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        if (bulkAssignmentDto.EmployeeAssignments == null || !bulkAssignmentDto.EmployeeAssignments.Any())
            return CreateResponse(Result.Fail("At least one employee assignment is required", 400));

        // Get authenticated manager ID using authorization service
        var managerId = await authorizationService.GetCurrentManagerIdAsync();
        logger.LogInformation("Manager {ManagerId} attempting to create {Count} assignments",
            managerId, bulkAssignmentDto.EmployeeAssignments.Count);

        // Validate all employee IDs are direct reports using authorization service
        var employeeIds = bulkAssignmentDto.EmployeeAssignments.Select(e => e.EmployeeId).ToList();
        var areAllDirectReports = await authorizationService.AreAllDirectReportsAsync(managerId, employeeIds);

        if (!areAllDirectReports)
        {
            logger.LogWarning("Manager {ManagerId} attempted to assign to employees who are not direct reports", managerId);
            return CreateResponse(Result.Fail("Cannot assign to employees who are not direct reports", 403));
        }

        var employeeAssignments = bulkAssignmentDto.EmployeeAssignments
            .Select(e => new EmployeeAssignmentData(
                e.EmployeeId,
                e.EmployeeName,
                e.EmployeeEmail))
            .ToList();

        var command = new CreateBulkAssignmentsCommand(
            bulkAssignmentDto.TemplateId,
            ProcessTypeMapper.MapToDomain(bulkAssignmentDto.ProcessType),
            employeeAssignments,
            bulkAssignmentDto.DueDate,
            managerId,
            bulkAssignmentDto.Notes);

        var result = await commandDispatcher.SendAsync(command);

        if (result.Succeeded)
        {
            logger.LogInformation("Manager {ManagerId} successfully created {Count} assignments",
                managerId, bulkAssignmentDto.EmployeeAssignments.Count);
        }

        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/start")]
    [Authorize(Policy = "HR")]
    public async Task<IActionResult> StartAssignmentWork(Guid assignmentId)
    {
        var command = new StartAssignmentWorkCommand(assignmentId);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/complete")]
    public async Task<IActionResult> CompleteAssignmentWork(Guid assignmentId)
    {
        var command = new CompleteAssignmentWorkCommand(assignmentId);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Extends the due date of an assignment.
    /// Managers can only extend assignments for their direct reports.
    /// HR/Admin can extend any assignment.
    /// </summary>
    [HttpPost("extend-due-date")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExtendAssignmentDueDate([FromBody] ExtendAssignmentDueDateDto extendDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        return await ExecuteWithAuthorizationAsync(
            commandAuthorizationService,
            async managerId =>
            {
                var command = new ExtendAssignmentDueDateCommand(
                    extendDto.AssignmentId,
                    extendDto.NewDueDate,
                    extendDto.ExtensionReason);

                return await commandDispatcher.SendAsync(command);
            },
            resourceId: extendDto.AssignmentId,
            requiresResourceAccess: true);
    }

    /// <summary>
    /// Withdraws (cancels) an assignment.
    /// Managers can only withdraw assignments for their direct reports.
    /// HR/Admin can withdraw any assignment.
    /// </summary>
    [HttpPost("withdraw")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> WithdrawAssignment([FromBody] WithdrawAssignmentDto withdrawDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        return await ExecuteWithAuthorizationAsync(
            commandAuthorizationService,
            async managerId =>
            {
                var command = new WithdrawAssignmentCommand(
                    withdrawDto.AssignmentId,
                    managerId,
                    withdrawDto.WithdrawalReason);

                return await commandDispatcher.SendAsync(command);
            },
            resourceId: withdrawDto.AssignmentId,
            requiresResourceAccess: true);
    }

    /// <summary>
    /// Initializes a questionnaire assignment.
    /// Transitions assignment from Assigned to Initialized state.
    /// Managers can only initialize assignments for their direct reports.
    /// HR/Admin can initialize any assignment.
    /// </summary>
    [HttpPost("{assignmentId}/initialize")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InitializeAssignment(Guid assignmentId, [FromBody] InitializeAssignmentDto initializeDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        return await ExecuteWithAuthorizationAsync(
            commandAuthorizationService,
            async managerId =>
            {
                var command = new InitializeAssignmentCommand(
                    assignmentId,
                    managerId,
                    initializeDto.InitializationNotes);

                return await commandDispatcher.SendAsync(command);
            },
            resourceId: assignmentId,
            requiresResourceAccess: true);
    }

    /// <summary>
    /// Adds custom question sections to an assignment during initialization.
    /// Custom sections are instance-specific and excluded from aggregate reports.
    /// Managers can only add sections to assignments for their direct reports.
    /// HR/Admin can add sections to any assignment.
    /// </summary>
    [HttpPost("{assignmentId}/custom-sections")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddCustomSections(Guid assignmentId, [FromBody] AddCustomSectionsDto sectionsDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        if (sectionsDto.Sections == null || !sectionsDto.Sections.Any())
            return CreateResponse(Result.Fail("At least one custom section is required", 400));

        return await ExecuteWithAuthorizationAsync(
            commandAuthorizationService,
            async managerId =>
            {
                // Map QuestionSectionDto to CommandQuestionSection using mapper
                var commandSections = questionSectionMapper.MapToCommandList(sectionsDto.Sections);

                var command = new AddCustomSectionsCommand(
                    assignmentId,
                    commandSections,
                    managerId);

                return await commandDispatcher.SendAsync(command);
            },
            resourceId: assignmentId,
            requiresResourceAccess: true);
    }

    // Workflow endpoints
    [HttpPost("{assignmentId}/sections/{sectionId}/complete-employee")]
    public async Task<IActionResult> CompleteSectionAsEmployee(Guid assignmentId, Guid sectionId)
    {
        var command = new CompleteSectionAsEmployeeCommand(assignmentId, sectionId);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/sections/bulk-complete-employee")]
    public async Task<IActionResult> CompleteBulkSectionsAsEmployee(Guid assignmentId, [FromBody] List<Guid> sectionIds)
    {
        if (sectionIds == null || !sectionIds.Any())
            return CreateResponse(Result.Fail("Section IDs list cannot be null or empty", 400));

        var command = new CompleteBulkSectionsAsEmployeeCommand(assignmentId, sectionIds);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/sections/{sectionId}/complete-manager")]
    [Authorize(Policy = "TeamLead")]
    public async Task<IActionResult> CompleteSectionAsManager(Guid assignmentId, Guid sectionId)
    {
        var command = new CompleteSectionAsManagerCommand(assignmentId, sectionId);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/sections/bulk-complete-manager")]
    [Authorize(Policy = "TeamLead")]
    public async Task<IActionResult> CompleteBulkSectionsAsManager(Guid assignmentId, [FromBody] List<Guid> sectionIds)
    {
        if (sectionIds == null || !sectionIds.Any())
            return CreateResponse(Result.Fail("Section IDs list cannot be null or empty", 400));

        var command = new CompleteBulkSectionsAsManagerCommand(assignmentId, sectionIds);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/submit-employee")]
    public async Task<IActionResult> SubmitEmployeeQuestionnaire(Guid assignmentId, [FromBody] SubmitQuestionnaireDto submitDto)
    {
        // Get employee ID from authenticated user context
        if (!userContext.TryGetUserId(out var employeeId, out var errorMessage))
        {
            logger.LogWarning("SubmitEmployeeQuestionnaire failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var command = new SubmitEmployeeQuestionnaireCommand(
            assignmentId,
            employeeId,
            submitDto.ExpectedVersion);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/submit-manager")]
    [Authorize(Policy = "TeamLead")]
    public async Task<IActionResult> SubmitManagerQuestionnaire(Guid assignmentId, [FromBody] SubmitQuestionnaireDto submitDto)
    {
        // Get manager ID from authenticated user context
        if (!userContext.TryGetUserId(out var managerId, out var errorMessage))
        {
            logger.LogWarning("SubmitManagerQuestionnaire failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var command = new SubmitManagerQuestionnaireCommand(
            assignmentId,
            managerId,
            submitDto.ExpectedVersion);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/initiate-review")]
    [Authorize(Policy = "TeamLead")]
    public async Task<IActionResult> InitiateReview(Guid assignmentId)
    {
        // Get manager ID from authenticated user context
        if (!userContext.TryGetUserId(out var managerId, out var errorMessage))
        {
            logger.LogWarning("InitiateReview failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var command = new InitiateReviewCommand(assignmentId, managerId);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/edit-answer")]
    public async Task<IActionResult> EditAnswerDuringReview(Guid assignmentId, [FromBody] EditAnswerDto editDto)
    {
        if (!Enum.TryParse<ApplicationRole>(editDto.OriginalCompletionRole, out var commandRole))
        {
            return CreateResponse(Result.Fail($"Invalid ApplicationRole value: '{editDto.OriginalCompletionRole}'", 400));
        }

        // Get user ID from authenticated user context
        if (!userContext.TryGetUserId(out var userId, out var errorMessage))
        {
            logger.LogWarning("EditAnswerDuringReview failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var domainRole = ApplicationRoleMapper.MapToDomain(commandRole);

        // Parse JSON answer to domain object (Infrastructure responsibility)
        var answerValue = questionResponseMappingService.ConvertSingleAnswerFromJson(editDto.Answer);

        var command = new EditAnswerDuringReviewCommand(
            assignmentId,
            editDto.SectionId,
            editDto.QuestionId,
            ApplicationRoleMapper.MapFromDomain(domainRole),
            editDto.Answer, // Raw JSON for audit
            answerValue,    // Parsed domain object for storage
            userId);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }


    /// <summary>
    /// Edit individual goal during review using RESTful approach.
    /// Updates only the specific goal identified by goalId using dedicated goal command.
    /// </summary>
    [HttpPut("{assignmentId}/goals/{goalId}")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditGoal(Guid assignmentId, Guid goalId, [FromBody] EditGoalDto editDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));


        if (!Enum.TryParse<ApplicationRole>(editDto.OriginalCompletionRole, out var commandRole))
        {
            return CreateResponse(Result.Fail($"Invalid ApplicationRole value: '{editDto.OriginalCompletionRole}'", 400));
        }

        // Get user ID from authenticated user context
        if (!userContext.TryGetUserId(out var userId, out var errorMessage))
        {
            logger.LogWarning("EditGoal failed for assignment {AssignmentId}, goal {GoalId}: {ErrorMessage}",
                assignmentId, goalId, errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        // Use dedicated goal editing command
        var command = new EditGoalDuringReviewCommand(
            assignmentId,
            goalId,
            editDto.SectionId,
            editDto.QuestionId,
            commandRole,
            editDto.ObjectiveDescription,
            editDto.MeasurementMetric,
            editDto.TimeframeFrom,
            editDto.TimeframeTo,
            editDto.WeightingPercentage,
            userId
        );

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }


    // Refined review workflow endpoints
    /// <summary>
    /// Manager finishes the review meeting.
    /// Transitions from InReview to ReviewFinished state.
    /// </summary>
    [HttpPost("{assignmentId}/review/finish")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FinishReviewMeeting(Guid assignmentId, [FromBody] FinishReviewMeetingDto finishDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        // Get manager ID from authenticated user context
        if (!userContext.TryGetUserId(out var managerId, out var errorMessage))
        {
            logger.LogWarning("FinishReviewMeeting failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var command = new FinishReviewMeetingCommand(
            assignmentId,
            managerId,
            finishDto.ReviewSummary,
            finishDto.ExpectedVersion);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Employee signs-off on review outcome.
    /// This is the intermediate step after manager finishes review meeting
    /// but before final employee confirmation.
    /// Transitions from ReviewFinished to EmployeeReviewConfirmed state.
    /// </summary>
    [HttpPost("{assignmentId}/review/sign-off")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SignOffReviewOutcome(Guid assignmentId, [FromBody] SignOffReviewDto signOffDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        // Get employee ID from authenticated user context
        if (!userContext.TryGetUserId(out var employeeId, out var errorMessage))
        {
            logger.LogWarning("SignOffReviewOutcome failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var command = new SignOffReviewOutcomeAsEmployeeCommand(
            assignmentId,
            employeeId,
            signOffDto.SignOffComments,
            signOffDto.ExpectedVersion);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Employee confirms the review outcome.
    /// Employee cannot reject but can add comments.
    /// Transitions from ReviewFinished to EmployeeReviewConfirmed state.
    /// </summary>
    [HttpPost("{assignmentId}/review/confirm-employee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmReviewOutcomeAsEmployee(Guid assignmentId, [FromBody] ConfirmReviewOutcomeDto confirmDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        // Get employee ID from authenticated user context
        if (!userContext.TryGetUserId(out var employeeId, out var errorMessage))
        {
            logger.LogWarning("ConfirmReviewOutcomeAsEmployee failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var command = new ConfirmReviewOutcomeAsEmployeeCommand(
            assignmentId,
            employeeId,
            confirmDto.EmployeeComments,
            confirmDto.ExpectedVersion);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Manager finalizes the questionnaire after employee confirmation.
    /// This is the final step in the review process.
    /// Transitions from EmployeeReviewConfirmed to Finalized state.
    /// </summary>
    [HttpPost("{assignmentId}/review/finalize-manager")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FinalizeQuestionnaireAsManager(Guid assignmentId, [FromBody] FinalizeAsManagerDto finalizeDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        // Get manager ID from authenticated user context
        if (!userContext.TryGetUserId(out var managerId, out var errorMessage))
        {
            logger.LogWarning("FinalizeQuestionnaireAsManager failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var command = new FinalizeQuestionnaireAsManagerCommand(
            assignmentId,
            managerId,
            finalizeDto.ManagerFinalNotes,
            finalizeDto.ExpectedVersion);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("{assignmentId}/notes")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddInReviewNote(Guid assignmentId, [FromBody] AddInReviewNoteDto noteDto)
    {
        var command = new AddInReviewNoteCommand(
            assignmentId,
            noteDto.Content,
            noteDto.SectionId);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPut("{assignmentId}/notes/{noteId}")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInReviewNote(Guid assignmentId, Guid noteId, [FromBody] UpdateInReviewNoteDto noteDto)
    {
        var command = new UpdateInReviewNoteCommand(
            assignmentId,
            noteId,
            noteDto.Content);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpDelete("{assignmentId}/notes/{noteId}")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInReviewNote(Guid assignmentId, Guid noteId)
    {
        var command = new DeleteInReviewNoteCommand(assignmentId, noteId);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    [HttpPost("reminder")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendReminder([FromBody] SendReminderDto reminderDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        // Get manager ID from authenticated user context
        if (!userContext.TryGetUserId(out var managerId, out var errorMessage))
        {
            logger.LogWarning("SendReminder failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        var command = new SendAssignmentReminderCommand(
            reminderDto.AssignmentId,
            reminderDto.Message,
            managerId);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Checks if the current user has an elevated role (HR, HRLead, or Admin).
    /// Returns true if elevated, false if user is only TeamLead/Employee.
    /// </summary>

    /// <summary>
    /// Reopens a questionnaire assignment for corrections.
    /// Requires Admin, HR, or TeamLead role.
    /// TeamLead can only reopen questionnaires for their own team members.
    /// Finalized questionnaires cannot be reopened - create new assignment instead.
    /// Sends email notifications to affected parties.
    /// </summary>
    /// <param name="assignmentId">The assignment to reopen</param>
    /// <param name="reopenDto">Target state and reason for reopening</param>
    [HttpPost("{assignmentId}/reopen")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReopenQuestionnaire(
        Guid assignmentId,
        [FromBody] ReopenQuestionnaireDto reopenDto)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        // Get user ID from authenticated user context
        if (!userContext.TryGetUserId(out var userId, out var errorMessage))
        {
            logger.LogWarning("ReopenQuestionnaire failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        // Get user role with cache-through pattern
        var employeeRole = await employeeRoleService.GetEmployeeRoleAsync(userId, HttpContext.RequestAborted);

        if (employeeRole == null)
        {
            logger.LogWarning("ReopenQuestionnaire failed: Unable to retrieve employee role for user {UserId}", userId);
            return CreateResponse(Result.Fail("User role not found", 401));
        }

        // Map ApplicationRole enum to string for command
        var queryRole = (Application.Query.Models.ApplicationRole)employeeRole.ApplicationRoleValue;
        var roleString = queryRole.ToString();

        var command = new ReopenQuestionnaireCommand(
            assignmentId,
            reopenDto.TargetState,
            reopenDto.ReopenReason,
            userId,
            roleString);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    #region Goal Operations

    /// <summary>
    /// Links a predecessor questionnaire to the current assignment for rating previous goals.
    /// Employee or Manager can link (first wins).
    /// </summary>
    [HttpPost("{assignmentId}/goals/link-predecessor")]
    public async Task<IActionResult> LinkPredecessorQuestionnaire(
        Guid assignmentId,
        [FromBody] LinkPredecessorQuestionnaireDto dto)
    {
        if (!userContext.TryGetUserId(out var userId, out var errorMessage))
        {
            logger.LogWarning("LinkPredecessorQuestionnaire failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        // Parse role from DTO
        if (!Enum.TryParse<ApplicationRole>(dto.LinkedByRole, out var commandRole))
        {
            return CreateResponse(Result.Fail($"Invalid role: {dto.LinkedByRole}", 400));
        }

        var domainRole = ApplicationRoleMapper.MapToDomain(commandRole);

        var command = new LinkPredecessorQuestionnaireCommand(
            assignmentId,
            dto.QuestionId,
            dto.PredecessorAssignmentId,
            ApplicationRoleMapper.MapFromDomain(domainRole),
            userId);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    #endregion

    #region Employee Feedback Operations

    /// <summary>
    /// Links employee feedback to a questionnaire assignment.
    /// Managers can link feedback during initialization phase.
    /// </summary>
    [HttpPost("{assignmentId}/feedback/link")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkEmployeeFeedback(
        Guid assignmentId,
        [FromBody] LinkEmployeeFeedbackDto dto)
    {
        if (!userContext.TryGetUserId(out var userId, out var errorMessage))
        {
            logger.LogWarning("LinkEmployeeFeedback failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        // Get user role from database
        var employeeRole = await employeeRoleService.GetEmployeeRoleAsync(userId, HttpContext.RequestAborted);
        if (employeeRole == null)
        {
            logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
            return CreateResponse(Result.Fail("User role not found", 401));
        }

        var queryRole = (Application.Query.Models.ApplicationRole)employeeRole.ApplicationRoleValue;
        var userRole = ApplicationRoleMapper.MapFromQuery(queryRole);

        var command = new LinkEmployeeFeedbackCommand(
            assignmentId,
            dto.QuestionId,
            dto.FeedbackId,
            userRole,
            userId);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Unlinks employee feedback from a questionnaire assignment.
    /// Managers can unlink feedback during initialization phase.
    /// </summary>
    [HttpPost("{assignmentId}/feedback/unlink")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkEmployeeFeedback(
        Guid assignmentId,
        [FromBody] UnlinkEmployeeFeedbackDto dto)
    {
        if (!userContext.TryGetUserId(out var userId, out var errorMessage))
        {
            logger.LogWarning("UnlinkEmployeeFeedback failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Result.Fail(errorMessage, 401));
        }

        // Get user role from database
        var employeeRole = await employeeRoleService.GetEmployeeRoleAsync(userId, HttpContext.RequestAborted);
        if (employeeRole == null)
        {
            logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
            return CreateResponse(Result.Fail("User role not found", 401));
        }

        var queryRole = (Application.Query.Models.ApplicationRole)employeeRole.ApplicationRoleValue;
        var userRole = ApplicationRoleMapper.MapFromQuery(queryRole);

        var command = new UnlinkEmployeeFeedbackCommand(
            assignmentId,
            dto.QuestionId,
            dto.FeedbackId,
            userRole,
            userId);

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    #endregion

}