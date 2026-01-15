# Implementation Plan: Feedback Question Type Integration

## Overview
Integrate the existing EmployeeFeedback aggregate with questionnaires by allowing managers to link existing feedback records to questionnaire assignments during the initialization phase. Feedback will be displayed read-only during the workflow.

## User Requirements Clarified
- **Template Design**: Feedback sections added during template design in Questionnaire Builder (like Goals)
- **Enum Strategy**: Reuse existing `QuestionType.EmployeeFeedback = 3` (no new enum value)
- **Linking Method**: Managers manually select specific EmployeeFeedback records during initialization
- **Cardinality**: Multiple feedback records can be linked per Feedback section

## Design Pattern: Follow Goals Implementation
This implementation mirrors the Goals pattern:
- Minimal template configuration (placeholder only)
- Instance-specific data added during workflow
- Linking during initialization phase (Assigned state)
- Read-only display during workflow
- Storage via response data (not separate aggregate properties)

---

## Implementation Steps

### Phase 1: Domain Layer

#### 1.1 Verify EmployeeFeedbackConfiguration
**Location**: `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/EmployeeFeedbackConfiguration.cs`

**Action**: Verify existing configuration class has minimal structure (like GoalConfiguration)
- If it has complex configuration (criteria, text sections, etc.), simplify it to just visibility flags
- Add `ShowFeedbackSection` boolean property (default: true)
- Ensure `IsValid()` returns true (always valid like Goals)

**Expected Structure**:
```csharp
public sealed class EmployeeFeedbackConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.EmployeeFeedback;
    public bool ShowFeedbackSection { get; set; } = true;
    public bool IsValid() => true;
}
```

#### 1.2 Add Feedback Linking to QuestionnaireAssignment
**Location**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/QuestionnaireAssignment.cs`

**Changes**:

**Add private field** for tracking feedback links (stores multiple feedback IDs per question):
```csharp
// Add after line ~65 (near _predecessorLinks)
private readonly Dictionary<Guid, HashSet<Guid>> _linkedFeedback = new();
public IReadOnlyDictionary<Guid, IReadOnlySet<Guid>> LinkedFeedback =>
    _linkedFeedback.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlySet<Guid>)kvp.Value);
```

**Add domain method** (add after `LinkPredecessorQuestionnaire` around line 683):
```csharp
public void LinkEmployeeFeedback(
    Guid questionId,
    Guid feedbackId,
    ApplicationRole linkedByRole,
    Guid linkedByEmployeeId)
{
    // Validation 1: Question exists and is EmployeeFeedback type
    var question = GetQuestionById(questionId);
    if (question == null || question.Type != QuestionType.EmployeeFeedback)
        throw new ArgumentException("Question must be of type EmployeeFeedback", nameof(questionId));

    // Validation 2: Not locked or withdrawn
    if (IsLocked || IsWithdrawn)
        throw new InvalidOperationException("Cannot link feedback to locked or withdrawn assignment");

    // Validation 3: Role-based workflow validation
    if (linkedByRole == ApplicationRole.Employee && !CanEmployeeEdit())
        throw new UnauthorizedAccessException("Employee cannot link feedback in current workflow state");
    if (linkedByRole >= ApplicationRole.TeamLead && !CanManagerEdit())
        throw new UnauthorizedAccessException("Manager cannot link feedback in current workflow state");

    // Validation 4: Feedback not already linked to this question
    if (_linkedFeedback.ContainsKey(questionId) && _linkedFeedback[questionId].Contains(feedbackId))
        throw new InvalidOperationException($"Feedback {feedbackId} is already linked to question {questionId}");

    RaiseEvent(new EmployeeFeedbackLinkedToAssignment(
        feedbackId,
        questionId,
        linkedByRole,
        DateTime.UtcNow,
        linkedByEmployeeId));
}

public void UnlinkEmployeeFeedback(
    Guid questionId,
    Guid feedbackId,
    ApplicationRole unlinkedByRole,
    Guid unlinkedByEmployeeId)
{
    // Validation: Feedback link exists
    if (!_linkedFeedback.ContainsKey(questionId) || !_linkedFeedback[questionId].Contains(feedbackId))
        throw new InvalidOperationException($"Feedback {feedbackId} is not linked to question {questionId}");

    // Same workflow validation as linking
    if (IsLocked || IsWithdrawn)
        throw new InvalidOperationException("Cannot unlink feedback from locked or withdrawn assignment");

    RaiseEvent(new EmployeeFeedbackUnlinkedFromAssignment(
        feedbackId,
        questionId,
        unlinkedByRole,
        DateTime.UtcNow,
        unlinkedByEmployeeId));
}

// Helper method for getting linked feedback
public IReadOnlySet<Guid> GetLinkedFeedback(Guid questionId)
{
    return _linkedFeedback.TryGetValue(questionId, out var feedbackIds)
        ? feedbackIds
        : new HashSet<Guid>();
}
```

#### 1.3 Add Domain Events
**Location**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/Events/`

**Create new file**: `EmployeeFeedbackLinkedToAssignment.cs`
```csharp
namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

public record EmployeeFeedbackLinkedToAssignment(
    Guid FeedbackId,
    Guid QuestionId,
    ApplicationRole LinkedByRole,
    DateTime LinkedDate,
    Guid LinkedByEmployeeId) : IDomainEvent;
```

**Create new file**: `EmployeeFeedbackUnlinkedFromAssignment.cs`
```csharp
namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

public record EmployeeFeedbackUnlinkedFromAssignment(
    Guid FeedbackId,
    Guid QuestionId,
    ApplicationRole UnlinkedByRole,
    DateTime UnlinkedDate,
    Guid UnlinkedByEmployeeId) : IDomainEvent;
```

#### 1.4 Add Apply Methods for Events
**Location**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/QuestionnaireAssignment.cs`

**Add after other Apply methods** (around line 820+):
```csharp
public void Apply(EmployeeFeedbackLinkedToAssignment @event)
{
    if (!_linkedFeedback.ContainsKey(@event.QuestionId))
        _linkedFeedback[@event.QuestionId] = new HashSet<Guid>();

    _linkedFeedback[@event.QuestionId].Add(@event.FeedbackId);
}

public void Apply(EmployeeFeedbackUnlinkedFromAssignment @event)
{
    if (_linkedFeedback.ContainsKey(@event.QuestionId))
    {
        _linkedFeedback[@event.QuestionId].Remove(@event.FeedbackId);
        if (_linkedFeedback[@event.QuestionId].Count == 0)
            _linkedFeedback.Remove(@event.QuestionId);
    }
}
```

---

### Phase 2: Application Layer - Command Side

#### 2.1 Create Link Command
**Location**: `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireAssignmentCommands/`

**Create new file**: `LinkEmployeeFeedbackCommand.cs`
```csharp
namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record LinkEmployeeFeedbackCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid FeedbackId,
    ApplicationRole LinkedByRole,
    Guid LinkedByEmployeeId) : ICommand<Result>;
```

**Create new file**: `UnlinkEmployeeFeedbackCommand.cs`
```csharp
namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record UnlinkEmployeeFeedbackCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid FeedbackId,
    ApplicationRole UnlinkedByRole,
    Guid UnlinkedByEmployeeId) : ICommand<Result>;
```

#### 2.2 Create Command Handlers
**Location**: `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireAssignmentCommands/`

**Create new file**: `LinkEmployeeFeedbackCommandHandler.cs`
```csharp
namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class LinkEmployeeFeedbackCommandHandler : ICommandHandler<LinkEmployeeFeedbackCommand, Result>
{
    private readonly IQuestionnaireAssignmentRepository _assignmentRepository;
    private readonly IEmployeeFeedbackRepository _feedbackRepository;
    private readonly ILogger<LinkEmployeeFeedbackCommandHandler> _logger;

    public LinkEmployeeFeedbackCommandHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IEmployeeFeedbackRepository feedbackRepository,
        ILogger<LinkEmployeeFeedbackCommandHandler> logger)
    {
        _assignmentRepository = assignmentRepository;
        _feedbackRepository = feedbackRepository;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(LinkEmployeeFeedbackCommand command)
    {
        try
        {
            // Load assignment aggregate
            var assignment = await _assignmentRepository.GetByIdAsync(command.AssignmentId);
            if (assignment == null)
                return Result.Failure($"Assignment {command.AssignmentId} not found");

            // Validate feedback exists and belongs to the same employee
            var feedback = await _feedbackRepository.GetByIdAsync(command.FeedbackId);
            if (feedback == null)
                return Result.Failure($"Feedback {command.FeedbackId} not found");

            if (feedback.EmployeeId != assignment.EmployeeId)
                return Result.Failure("Feedback must belong to the same employee as the assignment");

            if (feedback.IsDeleted)
                return Result.Failure("Cannot link deleted feedback");

            // Link feedback via domain method
            assignment.LinkEmployeeFeedback(
                command.QuestionId,
                command.FeedbackId,
                command.LinkedByRole,
                command.LinkedByEmployeeId);

            // Save aggregate
            await _assignmentRepository.SaveAsync(assignment);

            LogEmployeeFeedbackLinked(_logger, command.AssignmentId, command.QuestionId, command.FeedbackId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogEmployeeFeedbackLinkFailed(_logger, command.AssignmentId, command.FeedbackId, ex);
            return Result.Failure(ex.Message);
        }
    }

    [LoggerMessage(EventId = 6019, Level = LogLevel.Information,
        Message = "Linked feedback {FeedbackId} to question {QuestionId} in assignment {AssignmentId}")]
    static partial void LogEmployeeFeedbackLinked(ILogger logger, Guid assignmentId, Guid questionId, Guid feedbackId);

    [LoggerMessage(EventId = 6020, Level = LogLevel.Error,
        Message = "Failed to link feedback {FeedbackId} to assignment {AssignmentId}")]
    static partial void LogEmployeeFeedbackLinkFailed(ILogger logger, Guid assignmentId, Guid feedbackId, Exception ex);
}
```

**Create new file**: `UnlinkEmployeeFeedbackCommandHandler.cs`
```csharp
namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class UnlinkEmployeeFeedbackCommandHandler : ICommandHandler<UnlinkEmployeeFeedbackCommand, Result>
{
    private readonly IQuestionnaireAssignmentRepository _assignmentRepository;
    private readonly ILogger<UnlinkEmployeeFeedbackCommandHandler> _logger;

    public UnlinkEmployeeFeedbackCommandHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        ILogger<UnlinkEmployeeFeedbackCommandHandler> logger)
    {
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(UnlinkEmployeeFeedbackCommand command)
    {
        try
        {
            var assignment = await _assignmentRepository.GetByIdAsync(command.AssignmentId);
            if (assignment == null)
                return Result.Failure($"Assignment {command.AssignmentId} not found");

            assignment.UnlinkEmployeeFeedback(
                command.QuestionId,
                command.FeedbackId,
                command.UnlinkedByRole,
                command.UnlinkedByEmployeeId);

            await _assignmentRepository.SaveAsync(assignment);

            LogEmployeeFeedbackUnlinked(_logger, command.AssignmentId, command.QuestionId, command.FeedbackId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogEmployeeFeedbackUnlinkFailed(_logger, command.AssignmentId, command.FeedbackId, ex);
            return Result.Failure(ex.Message);
        }
    }

    [LoggerMessage(EventId = 6021, Level = LogLevel.Information,
        Message = "Unlinked feedback {FeedbackId} from question {QuestionId} in assignment {AssignmentId}")]
    static partial void LogEmployeeFeedbackUnlinked(ILogger logger, Guid assignmentId, Guid questionId, Guid feedbackId);

    [LoggerMessage(EventId = 6022, Level = LogLevel.Error,
        Message = "Failed to unlink feedback {FeedbackId} from assignment {AssignmentId}")]
    static partial void LogEmployeeFeedbackUnlinkFailed(ILogger logger, Guid assignmentId, Guid feedbackId, Exception ex);
}
```

---

### Phase 3: Application Layer - Query Side

#### 3.1 Create Query DTOs
**Location**: `02_Application/ti8m.BeachBreak.Application.Query/DTOs/`

**Create new file**: `LinkedEmployeeFeedbackDto.cs`
```csharp
namespace ti8m.BeachBreak.Application.Query.DTOs;

public record LinkedEmployeeFeedbackDto
{
    public Guid FeedbackId { get; init; }
    public Guid EmployeeId { get; init; }
    public FeedbackSourceType SourceType { get; init; }
    public string ProviderName { get; init; } = string.Empty;
    public DateTime FeedbackDate { get; init; }
    public ConfigurableFeedbackData FeedbackData { get; init; } = null!;
    public string? ProjectName { get; init; }
    public string? ProjectRole { get; init; }
    public decimal? AverageRating { get; init; }
    public int RatedItemsCount { get; init; }
    public bool HasComments { get; init; }
}
```

**Create new file**: `FeedbackQuestionDataDto.cs`
```csharp
namespace ti8m.BeachBreak.Application.Query.DTOs;

public record FeedbackQuestionDataDto
{
    public Guid QuestionId { get; init; }
    public WorkflowState WorkflowState { get; init; }
    public List<LinkedEmployeeFeedbackDto> LinkedFeedback { get; init; } = new();
}
```

#### 3.2 Create Queries
**Location**: `02_Application/ti8m.BeachBreak.Application.Query/Queries/QuestionnaireAssignmentQueries/`

**Create new file**: `GetAvailableEmployeeFeedbackQuery.cs`
```csharp
namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public record GetAvailableEmployeeFeedbackQuery(
    Guid AssignmentId) : IQuery<Result<List<LinkedEmployeeFeedbackDto>>>;
```

**Create new file**: `GetFeedbackQuestionDataQuery.cs`
```csharp
namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public record GetFeedbackQuestionDataQuery(
    Guid AssignmentId,
    Guid QuestionId) : IQuery<Result<FeedbackQuestionDataDto>>;
```

#### 3.3 Create Query Handlers
**Location**: `02_Application/ti8m.BeachBreak.Application.Query/Queries/QuestionnaireAssignmentQueries/`

**Create new file**: `GetAvailableEmployeeFeedbackQueryHandler.cs`
```csharp
namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class GetAvailableEmployeeFeedbackQueryHandler
    : IQueryHandler<GetAvailableEmployeeFeedbackQuery, Result<List<LinkedEmployeeFeedbackDto>>>
{
    private readonly IEmployeeFeedbackRepository _feedbackRepository;
    private readonly IQuestionnaireAssignmentRepository _assignmentRepository;

    public GetAvailableEmployeeFeedbackQueryHandler(
        IEmployeeFeedbackRepository feedbackRepository,
        IQuestionnaireAssignmentRepository assignmentRepository)
    {
        _feedbackRepository = feedbackRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<Result<List<LinkedEmployeeFeedbackDto>>> HandleAsync(GetAvailableEmployeeFeedbackQuery query)
    {
        // Get assignment to find employee
        var assignment = await _assignmentRepository.GetByIdAsync(query.AssignmentId);
        if (assignment == null)
            return Result<List<LinkedEmployeeFeedbackDto>>.Failure($"Assignment {query.AssignmentId} not found");

        // Get all non-deleted feedback for this employee
        var feedback = await _feedbackRepository.GetByEmployeeIdAsync(assignment.EmployeeId);

        var feedbackDtos = feedback
            .Where(f => !f.IsDeleted)
            .OrderByDescending(f => f.FeedbackDate)
            .Select(f => new LinkedEmployeeFeedbackDto
            {
                FeedbackId = f.Id,
                EmployeeId = f.EmployeeId,
                SourceType = f.SourceType,
                ProviderName = f.ProviderInfo.ProviderName,
                FeedbackDate = f.FeedbackDate,
                FeedbackData = f.FeedbackData,
                ProjectName = f.ProviderInfo.ProjectName,
                ProjectRole = f.ProviderInfo.ProjectRole,
                AverageRating = f.AverageRating,
                RatedItemsCount = f.RatedItemsCount,
                HasComments = f.HasComments
            })
            .ToList();

        return Result<List<LinkedEmployeeFeedbackDto>>.Success(feedbackDtos);
    }
}
```

**Create new file**: `GetFeedbackQuestionDataQueryHandler.cs`
```csharp
namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class GetFeedbackQuestionDataQueryHandler
    : IQueryHandler<GetFeedbackQuestionDataQuery, Result<FeedbackQuestionDataDto>>
{
    private readonly IQuestionnaireAssignmentRepository _assignmentRepository;
    private readonly IEmployeeFeedbackRepository _feedbackRepository;

    public GetFeedbackQuestionDataQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IEmployeeFeedbackRepository feedbackRepository)
    {
        _assignmentRepository = assignmentRepository;
        _feedbackRepository = feedbackRepository;
    }

    public async Task<Result<FeedbackQuestionDataDto>> HandleAsync(GetFeedbackQuestionDataQuery query)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(query.AssignmentId);
        if (assignment == null)
            return Result<FeedbackQuestionDataDto>.Failure($"Assignment {query.AssignmentId} not found");

        // Get linked feedback IDs for this question
        var linkedFeedbackIds = assignment.GetLinkedFeedback(query.QuestionId);

        // Fetch full feedback data
        var linkedFeedback = new List<LinkedEmployeeFeedbackDto>();
        foreach (var feedbackId in linkedFeedbackIds)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(feedbackId);
            if (feedback != null && !feedback.IsDeleted)
            {
                linkedFeedback.Add(new LinkedEmployeeFeedbackDto
                {
                    FeedbackId = feedback.Id,
                    EmployeeId = feedback.EmployeeId,
                    SourceType = feedback.SourceType,
                    ProviderName = feedback.ProviderInfo.ProviderName,
                    FeedbackDate = feedback.FeedbackDate,
                    FeedbackData = feedback.FeedbackData,
                    ProjectName = feedback.ProviderInfo.ProjectName,
                    ProjectRole = feedback.ProviderInfo.ProjectRole,
                    AverageRating = feedback.AverageRating,
                    RatedItemsCount = feedback.RatedItemsCount,
                    HasComments = feedback.HasComments
                });
            }
        }

        var result = new FeedbackQuestionDataDto
        {
            QuestionId = query.QuestionId,
            WorkflowState = assignment.WorkflowState,
            LinkedFeedback = linkedFeedback.OrderByDescending(f => f.FeedbackDate).ToList()
        };

        return Result<FeedbackQuestionDataDto>.Success(result);
    }
}
```

---

### Phase 4: Infrastructure - API Endpoints

#### 4.1 Command API DTOs
**Location**: `03_Infrastructure/ti8m.BeachBreak.CommandApi/DTOs/`

**Create new file**: `LinkEmployeeFeedbackDto.cs`
```csharp
namespace ti8m.BeachBreak.CommandApi.DTOs;

public record LinkEmployeeFeedbackDto
{
    public Guid QuestionId { get; init; }
    public Guid FeedbackId { get; init; }
}
```

**Create new file**: `UnlinkEmployeeFeedbackDto.cs`
```csharp
namespace ti8m.BeachBreak.CommandApi.DTOs;

public record UnlinkEmployeeFeedbackDto
{
    public Guid QuestionId { get; init; }
    public Guid FeedbackId { get; init; }
}
```

#### 4.2 Command API Endpoints
**Location**: `03_Infrastructure/ti8m.BeachBreak.CommandApi/Controllers/AssignmentsController.cs`

**Add endpoints** (add after other assignment endpoints, around line 300+):
```csharp
[HttpPost("{assignmentId}/feedback/link")]
[Authorize(Policy = "TeamLead")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> LinkEmployeeFeedback(
    Guid assignmentId,
    [FromBody] LinkEmployeeFeedbackDto dto)
{
    var userId = Guid.Parse(_userContext.Id);
    var userRole = _userContext.ApplicationRole;

    var command = new LinkEmployeeFeedbackCommand(
        assignmentId,
        dto.QuestionId,
        dto.FeedbackId,
        userRole,
        userId);

    var result = await _commandDispatcher.DispatchAsync(command);
    return CreateResponse(result);
}

[HttpPost("{assignmentId}/feedback/unlink")]
[Authorize(Policy = "TeamLead")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> UnlinkEmployeeFeedback(
    Guid assignmentId,
    [FromBody] UnlinkEmployeeFeedbackDto dto)
{
    var userId = Guid.Parse(_userContext.Id);
    var userRole = _userContext.ApplicationRole;

    var command = new UnlinkEmployeeFeedbackCommand(
        assignmentId,
        dto.QuestionId,
        dto.FeedbackId,
        userRole,
        userId);

    var result = await _commandDispatcher.DispatchAsync(command);
    return CreateResponse(result);
}
```

#### 4.3 Query API Endpoints
**Location**: `03_Infrastructure/ti8m.BeachBreak.QueryApi/Controllers/AssignmentsController.cs`

**Add endpoints** (add after other query endpoints):
```csharp
[HttpGet("{assignmentId}/feedback/available")]
[Authorize]
[ProducesResponseType(typeof(List<LinkedEmployeeFeedbackDto>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetAvailableEmployeeFeedback(Guid assignmentId)
{
    var query = new GetAvailableEmployeeFeedbackQuery(assignmentId);
    var result = await _queryDispatcher.DispatchAsync(query);
    return CreateResponse(result);
}

[HttpGet("{assignmentId}/feedback/{questionId}")]
[Authorize]
[ProducesResponseType(typeof(FeedbackQuestionDataDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetFeedbackQuestionData(Guid assignmentId, Guid questionId)
{
    var query = new GetFeedbackQuestionDataQuery(assignmentId, questionId);
    var result = await _queryDispatcher.DispatchAsync(query);
    return CreateResponse(result);
}
```

---

### Phase 5: Frontend - Services

#### 5.1 Create Employee Feedback API Service
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Services/`

**Create new file**: `EmployeeFeedbackApiService.cs`
```csharp
namespace ti8m.BeachBreak.Client.Services;

public class EmployeeFeedbackApiService
{
    private readonly HttpClient _commandHttpClient;
    private readonly HttpClient _queryHttpClient;
    private readonly ILogger<EmployeeFeedbackApiService> _logger;

    public EmployeeFeedbackApiService(
        IHttpClientFactory httpClientFactory,
        ILogger<EmployeeFeedbackApiService> logger)
    {
        _commandHttpClient = httpClientFactory.CreateClient("CommandApi");
        _queryHttpClient = httpClientFactory.CreateClient("QueryApi");
        _logger = logger;
    }

    public async Task<bool> LinkFeedbackAsync(Guid assignmentId, LinkEmployeeFeedbackDto dto)
    {
        try
        {
            var response = await _commandHttpClient.PostAsJsonAsync(
                $"c/api/v1/assignments/{assignmentId}/feedback/link", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to link feedback: {Error}", error);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking feedback to assignment {AssignmentId}", assignmentId);
            return false;
        }
    }

    public async Task<bool> UnlinkFeedbackAsync(Guid assignmentId, UnlinkEmployeeFeedbackDto dto)
    {
        try
        {
            var response = await _commandHttpClient.PostAsJsonAsync(
                $"c/api/v1/assignments/{assignmentId}/feedback/unlink", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to unlink feedback: {Error}", error);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking feedback from assignment {AssignmentId}", assignmentId);
            return false;
        }
    }

    public async Task<List<LinkedEmployeeFeedbackDto>> GetAvailableFeedbackAsync(Guid assignmentId)
    {
        try
        {
            var response = await _queryHttpClient.GetAsync(
                $"q/api/v1/assignments/{assignmentId}/feedback/available");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get available feedback for assignment {AssignmentId}", assignmentId);
                return new List<LinkedEmployeeFeedbackDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<LinkedEmployeeFeedbackDto>>()
                ?? new List<LinkedEmployeeFeedbackDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available feedback for assignment {AssignmentId}", assignmentId);
            return new List<LinkedEmployeeFeedbackDto>();
        }
    }

    public async Task<FeedbackQuestionDataDto?> GetFeedbackQuestionDataAsync(Guid assignmentId, Guid questionId)
    {
        try
        {
            var response = await _queryHttpClient.GetAsync(
                $"q/api/v1/assignments/{assignmentId}/feedback/{questionId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get feedback question data for question {QuestionId}", questionId);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FeedbackQuestionDataDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching feedback question data for question {QuestionId}", questionId);
            return null;
        }
    }
}
```

**Register service** in `Program.cs`:
```csharp
builder.Services.AddScoped<EmployeeFeedbackApiService>();
```

#### 5.2 Create Frontend DTOs
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Models/Dto/`

**Create matching DTOs** for client-side use:
- `LinkEmployeeFeedbackDto.cs`
- `UnlinkEmployeeFeedbackDto.cs`
- `LinkedEmployeeFeedbackDto.cs`
- `FeedbackQuestionDataDto.cs`

(Copy structures from Application.Query DTOs, ensuring type matching)

---

### Phase 6: Frontend - Components

#### 6.1 Create Link Feedback Dialog
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Components/Dialogs/`

**Create new file**: `LinkEmployeeFeedbackDialog.razor`
```razor
@using ti8m.BeachBreak.Client.Services
@inject EmployeeFeedbackApiService FeedbackApiService
@inject NotificationService NotificationService

<RadzenStack Gap="1rem">
    @if (isLoading)
    {
        <RadzenProgressBarCircular Mode="ProgressBarMode.Indeterminate" Size="ProgressBarCircularSize.Medium" />
        <RadzenText>@T("messages.loading-available-feedback")</RadzenText>
    }
    else if (availableFeedback == null || availableFeedback.Count == 0)
    {
        <RadzenAlert AlertStyle="AlertStyle.Info" Variant="Variant.Flat">
            @T("messages.no-feedback-available")
        </RadzenAlert>
    }
    else
    {
        <RadzenText TextStyle="TextStyle.Body1">
            @T("sections.select-feedback-to-link")
        </RadzenText>

        <RadzenDataGrid Data="@availableFeedback"
                        TItem="LinkedEmployeeFeedbackDto"
                        AllowFiltering="true"
                        FilterMode="FilterMode.Simple"
                        SelectionMode="DataGridSelectionMode.Multiple"
                        @bind-Value="@selectedFeedback">
            <Columns>
                <RadzenDataGridColumn TItem="LinkedEmployeeFeedbackDto" Width="50px" Sortable="false" Filterable="false">
                    <HeaderTemplate>
                        <RadzenCheckBox TriState="false"
                                        TValue="bool"
                                        Value="@(selectedFeedback?.Count == availableFeedback?.Count)"
                                        Change="@((args) => SelectAllFeedback(args))" />
                    </HeaderTemplate>
                    <Template Context="feedback">
                        <RadzenCheckBox TriState="false"
                                        Value="@(selectedFeedback?.Contains(feedback) == true)"
                                        TValue="bool" />
                    </Template>
                </RadzenDataGridColumn>

                <RadzenDataGridColumn TItem="LinkedEmployeeFeedbackDto" Property="FeedbackDate" Title="@T("columns.feedback-date")" Width="140px">
                    <Template Context="feedback">
                        @feedback.FeedbackDate.ToShortDateString()
                    </Template>
                </RadzenDataGridColumn>

                <RadzenDataGridColumn TItem="LinkedEmployeeFeedbackDto" Property="SourceType" Title="@T("columns.source-type")" Width="140px">
                    <Template Context="feedback">
                        <RadzenBadge BadgeStyle="@GetSourceTypeBadgeStyle(feedback.SourceType)"
                                     Text="@GetSourceTypeDisplayName(feedback.SourceType)" />
                    </Template>
                </RadzenDataGridColumn>

                <RadzenDataGridColumn TItem="LinkedEmployeeFeedbackDto" Property="ProviderName" Title="@T("columns.provider")" />

                <RadzenDataGridColumn TItem="LinkedEmployeeFeedbackDto" Property="ProjectName" Title="@T("columns.project")" />

                <RadzenDataGridColumn TItem="LinkedEmployeeFeedbackDto" Property="AverageRating" Title="@T("columns.avg-rating")" Width="120px">
                    <Template Context="feedback">
                        @if (feedback.AverageRating.HasValue)
                        {
                            <RadzenRating ReadOnly="true" Value="@((int)Math.Round(feedback.AverageRating.Value))" Stars="5" />
                        }
                        else
                        {
                            <RadzenText TextStyle="TextStyle.Body2">@T("labels.no-ratings")</RadzenText>
                        }
                    </Template>
                </RadzenDataGridColumn>
            </Columns>
        </RadzenDataGrid>

        <RadzenStack Orientation="Orientation.Horizontal" JustifyContent="JustifyContent.End" Gap="0.5rem">
            <RadzenButton Text="@T("buttons.cancel")"
                          ButtonStyle="ButtonStyle.Light"
                          Click="Cancel" />
            <RadzenButton Text="@T("buttons.link-selected")"
                          ButtonStyle="ButtonStyle.Primary"
                          Click="LinkSelected"
                          Disabled="@(selectedFeedback == null || selectedFeedback.Count == 0 || isSubmitting)" />
        </RadzenStack>
    }
</RadzenStack>

@code {
    [Parameter] public Guid AssignmentId { get; set; }
    [Parameter] public Guid QuestionId { get; set; }
    [Parameter] public EventCallback<List<LinkedEmployeeFeedbackDto>> OnFeedbackLinked { get; set; }

    [CascadingParameter] public DialogService DialogService { get; set; } = null!;

    private bool isLoading = true;
    private bool isSubmitting = false;
    private List<LinkedEmployeeFeedbackDto>? availableFeedback;
    private IList<LinkedEmployeeFeedbackDto>? selectedFeedback;

    protected override async Task OnInitializedAsync()
    {
        await LoadAvailableFeedback();
    }

    private async Task LoadAvailableFeedback()
    {
        isLoading = true;
        availableFeedback = await FeedbackApiService.GetAvailableFeedbackAsync(AssignmentId);
        isLoading = false;
    }

    private void SelectAllFeedback(bool? selectAll)
    {
        selectedFeedback = selectAll == true ? availableFeedback : new List<LinkedEmployeeFeedbackDto>();
    }

    private async Task LinkSelected()
    {
        if (selectedFeedback == null || selectedFeedback.Count == 0)
            return;

        isSubmitting = true;

        var successCount = 0;
        var failureCount = 0;

        foreach (var feedback in selectedFeedback)
        {
            var dto = new LinkEmployeeFeedbackDto
            {
                QuestionId = QuestionId,
                FeedbackId = feedback.FeedbackId
            };

            var success = await FeedbackApiService.LinkFeedbackAsync(AssignmentId, dto);
            if (success)
                successCount++;
            else
                failureCount++;
        }

        isSubmitting = false;

        if (successCount > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = T("notifications.feedback-linked-title"),
                Detail = string.Format(T("notifications.feedback-linked-detail"), successCount),
                Duration = 4000
            });

            await OnFeedbackLinked.InvokeAsync(selectedFeedback.ToList());
            DialogService.Close(true);
        }

        if (failureCount > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = T("notifications.feedback-link-failed-title"),
                Detail = string.Format(T("notifications.feedback-link-failed-detail"), failureCount),
                Duration = 4000
            });
        }
    }

    private void Cancel()
    {
        DialogService.Close(false);
    }

    private BadgeStyle GetSourceTypeBadgeStyle(FeedbackSourceType sourceType) => sourceType switch
    {
        FeedbackSourceType.Customer => BadgeStyle.Success,
        FeedbackSourceType.Peer => BadgeStyle.Info,
        FeedbackSourceType.ProjectColleague => BadgeStyle.Warning,
        _ => BadgeStyle.Light
    };

    private string GetSourceTypeDisplayName(FeedbackSourceType sourceType) => sourceType switch
    {
        FeedbackSourceType.Customer => T("feedback-source.customer"),
        FeedbackSourceType.Peer => T("feedback-source.peer"),
        FeedbackSourceType.ProjectColleague => T("feedback-source.project-colleague"),
        _ => T("feedback-source.unknown")
    };
}
```

#### 6.2 Create Optimized Feedback Question Component
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/`

**Create new file**: `OptimizedEmployeeFeedbackQuestion.razor`
```razor
@using ti8m.BeachBreak.Client.Services
@inject EmployeeFeedbackApiService FeedbackApiService

<RadzenCard class="feedback-question-container">
    @if (isLoading)
    {
        <RadzenProgressBarCircular Mode="ProgressBarMode.Indeterminate" Size="ProgressBarCircularSize.Small" />
    }
    else if (linkedFeedback == null || linkedFeedback.Count == 0)
    {
        <RadzenAlert AlertStyle="AlertStyle.Info" Variant="Variant.Flat">
            @T("messages.no-feedback-linked")
        </RadzenAlert>
    }
    else
    {
        <RadzenStack Gap="1.5rem">
            @foreach (var feedback in linkedFeedback)
            {
                <EmployeeFeedbackDisplayCard Feedback="@feedback" />
            }
        </RadzenStack>
    }
</RadzenCard>

@code {
    [Parameter] public QuestionSection Section { get; set; } = null!;
    [Parameter] public Guid AssignmentId { get; set; }
    [Parameter] public bool IsReadOnly { get; set; } = true;  // Always read-only

    private bool isLoading = true;
    private List<LinkedEmployeeFeedbackDto> linkedFeedback = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadLinkedFeedback();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (HasParameterChanged(nameof(Section), Section) ||
            HasParameterChanged(nameof(AssignmentId), AssignmentId))
        {
            await LoadLinkedFeedback();
        }
    }

    private async Task LoadLinkedFeedback()
    {
        isLoading = true;

        var feedbackData = await FeedbackApiService.GetFeedbackQuestionDataAsync(AssignmentId, Section.Id);
        if (feedbackData != null)
        {
            linkedFeedback = feedbackData.LinkedFeedback;
        }

        isLoading = false;
    }

    private bool HasParameterChanged<T>(string parameterName, T currentValue)
    {
        // Implement parameter change tracking logic
        return true;
    }
}
```

#### 6.3 Update OptimizedQuestionRenderer
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/OptimizedQuestionRenderer.razor`

**Add case for EmployeeFeedback** (add after Goal case):
```razor
@switch (Section.Type)
{
    // ... existing cases ...

    case QuestionType.EmployeeFeedback:
        <OptimizedEmployeeFeedbackQuestion Section="@Section"
                                            AssignmentId="@AssignmentId"
                                            IsReadOnly="true" />
        break;

    default:
        <RadzenAlert AlertStyle="AlertStyle.Warning">
            Unknown question type: @Section.Type
        </RadzenAlert>
        break;
}
```

#### 6.4 Create Question Handler
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionHandlers/`

**Create new file**: `EmployeeFeedbackQuestionHandler.cs`
```csharp
namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

public class EmployeeFeedbackQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.EmployeeFeedback;

    public void InitializeQuestion(QuestionSection question)
    {
        // Initialize with minimal configuration (like Goals)
        question.Configuration = new EmployeeFeedbackConfiguration
        {
            ShowFeedbackSection = true
        };
    }

    public void ValidateQuestion(QuestionSection question, List<string> errors)
    {
        // Basic validation only (title/description)
        if (string.IsNullOrWhiteSpace(question.Title.English))
            errors.Add("Employee Feedback question title (English) is required");

        if (string.IsNullOrWhiteSpace(question.Title.German))
            errors.Add("Employee Feedback question title (German) is required");

        // No validation for items - feedback questions don't have template items
    }

    // All item manipulation methods are NO-OPS (like Goals)
    public void AddItem(QuestionSection question, object item) { }
    public void RemoveItem(QuestionSection question, Guid itemId) { }
    public void MoveItem(QuestionSection question, Guid itemId, int newPosition) { }
    public void UpdateItem(QuestionSection question, Guid itemId, object updatedItem) { }
}
```

**Register handler** in `Program.cs`:
```csharp
builder.Services.AddScoped<IQuestionTypeHandler, EmployeeFeedbackQuestionHandler>();
```

#### 6.5 Create Questionnaire Builder Renderer
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Components/QuestionnaireBuilder/Renderers/`

**Create new file**: `EmployeeFeedbackQuestionRenderer.razor`
```razor
<RadzenCard class="question-renderer-card">
    <RadzenStack Gap="1rem">
        <RadzenText TextStyle="TextStyle.H6">
            @T("question-types.employee-feedback")
        </RadzenText>

        <RadzenAlert AlertStyle="AlertStyle.Info" Variant="Variant.Flat">
            <RadzenStack Gap="0.5rem">
                <RadzenText TextStyle="TextStyle.Body1">
                    @T("messages.feedback-question-no-config-required")
                </RadzenText>
                <RadzenText TextStyle="TextStyle.Body2">
                    @T("messages.feedback-question-workflow-explanation")
                </RadzenText>
            </RadzenStack>
        </RadzenAlert>

        <RadzenText TextStyle="TextStyle.Body2" class="text-muted">
            @T("messages.feedback-linked-during-initialization")
        </RadzenText>
    </RadzenStack>
</RadzenCard>

@code {
    [Parameter] public QuestionSection Section { get; set; } = null!;
}
```

#### 6.6 Update InitializeAssignment Page
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Pages/InitializeAssignment.razor`

**Add feedback linking section** (after predecessor linking, before custom sections):
```razor
@* Link Employee Feedback *@
@if (template?.Sections.Any(s => s.Type == QuestionType.EmployeeFeedback) == true)
{
    <RadzenCard class="initialization-task-card">
        <RadzenStack Gap="0.75rem">
            <RadzenStack Orientation="Orientation.Horizontal" JustifyContent="JustifyContent.SpaceBetween" AlignItems="AlignItems.Center">
                <RadzenText TextStyle="TextStyle.Subtitle1">
                    @T("sections.link-employee-feedback")
                </RadzenText>
                @if (feedbackLinkedCount > 0)
                {
                    <RadzenBadge BadgeStyle="BadgeStyle.Success" Text="@($"{feedbackLinkedCount} {T("labels.linked")}")" />
                }
            </RadzenStack>

            <RadzenText TextStyle="TextStyle.Body2">
                @T("messages.feedback-linking-explanation")
            </RadzenText>

            @foreach (var feedbackSection in template.Sections.Where(s => s.Type == QuestionType.EmployeeFeedback))
            {
                <RadzenStack Gap="0.5rem" class="feedback-section-link-item">
                    <RadzenText TextStyle="TextStyle.Subtitle2">
                        @feedbackSection.Title.GetTranslation(currentLanguage)
                    </RadzenText>

                    <RadzenButton Text="@T("buttons.link-feedback")"
                                  ButtonStyle="ButtonStyle.Secondary"
                                  Size="ButtonSize.Small"
                                  Click="@(() => OpenLinkFeedbackDialog(feedbackSection.Id))" />
                </RadzenStack>
            }
        </RadzenStack>
    </RadzenCard>
}

@code {
    // Add to existing @code block
    private int feedbackLinkedCount = 0;

    private async Task OpenLinkFeedbackDialog(Guid questionId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "AssignmentId", assignmentId },
            { "QuestionId", questionId },
            { "OnFeedbackLinked", EventCallback.Factory.Create<List<LinkedEmployeeFeedbackDto>>(this, OnFeedbackLinkedToQuestion) }
        };

        var result = await DialogService.OpenAsync<LinkEmployeeFeedbackDialog>(
            T("dialogs.link-employee-feedback-title"),
            parameters,
            new DialogOptions { Width = "900px", Height = "600px" });

        if (result is bool success && success)
        {
            // Feedback linked successfully
            feedbackLinkedCount++;
            StateHasChanged();
        }
    }

    private void OnFeedbackLinkedToQuestion(List<LinkedEmployeeFeedbackDto> linkedFeedback)
    {
        feedbackLinkedCount += linkedFeedback.Count;
    }
}
```

---

### Phase 7: Translation Keys

Add the following translation keys to `TestDataGenerator/test-translations.json`:

```json
{
  "key": "question-types.employee-feedback",
  "german": "Mitarbeiter-Feedback",
  "english": "Employee Feedback",
  "category": "question-types",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "messages.feedback-question-no-config-required",
  "german": "F\u00FCr Feedback-Fragen ist keine Vorlagenkonfiguration erforderlich.",
  "english": "No template configuration is required for Feedback questions.",
  "category": "messages",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "messages.feedback-question-workflow-explanation",
  "german": "W\u00E4hrend der Initialisierungsphase verkn\u00FCpfen Manager vorhandene Mitarbeiter-Feedback-Datens\u00E4tze. Das Feedback wird w\u00E4hrend des Workflows schreibgesch\u00FCtzt angezeigt.",
  "english": "During the initialization phase, managers link existing employee feedback records. The feedback is displayed read-only during the workflow.",
  "category": "messages",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "messages.feedback-linked-during-initialization",
  "german": "Feedback wird w\u00E4hrend der Zuweisungsinitialisierung verkn\u00FCpft",
  "english": "Feedback is linked during assignment initialization",
  "category": "messages",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "sections.link-employee-feedback",
  "german": "Mitarbeiter-Feedback verkn\u00FCpfen",
  "english": "Link Employee Feedback",
  "category": "sections",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "messages.feedback-linking-explanation",
  "german": "Verkn\u00FCpfen Sie vorhandene Feedback-Datens\u00E4tze mit diesem Fragebogen, um sie w\u00E4hrend der \u00DCberpr\u00FCfung zu referenzieren.",
  "english": "Link existing feedback records to this questionnaire for reference during review.",
  "category": "messages",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "buttons.link-feedback",
  "german": "Feedback verkn\u00FCpfen",
  "english": "Link Feedback",
  "category": "buttons",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "dialogs.link-employee-feedback-title",
  "german": "Mitarbeiter-Feedback verkn\u00FCpfen",
  "english": "Link Employee Feedback",
  "category": "dialogs",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "messages.loading-available-feedback",
  "german": "Verf\u00FCgbares Feedback wird geladen...",
  "english": "Loading available feedback...",
  "category": "messages",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "messages.no-feedback-available",
  "german": "Kein Feedback zum Verkn\u00FCpfen verf\u00FCgbar",
  "english": "No feedback available to link",
  "category": "messages",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "sections.select-feedback-to-link",
  "german": "W\u00E4hlen Sie Feedback-Datens\u00E4tze zum Verkn\u00FCpfen aus:",
  "english": "Select feedback records to link:",
  "category": "sections",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "columns.feedback-date",
  "german": "Feedback-Datum",
  "english": "Feedback Date",
  "category": "columns",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "columns.source-type",
  "german": "Quellentyp",
  "english": "Source Type",
  "category": "columns",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "columns.provider",
  "german": "Anbieter",
  "english": "Provider",
  "category": "columns",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "columns.project",
  "german": "Projekt",
  "english": "Project",
  "category": "columns",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "columns.avg-rating",
  "german": "Durchschn. Bewertung",
  "english": "Avg Rating",
  "category": "columns",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "labels.no-ratings",
  "german": "Keine Bewertungen",
  "english": "No Ratings",
  "category": "labels",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "buttons.link-selected",
  "german": "Ausgew\u00E4hlte verkn\u00FCpfen",
  "english": "Link Selected",
  "category": "buttons",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "notifications.feedback-linked-title",
  "german": "Feedback verkn\u00FCpft",
  "english": "Feedback Linked",
  "category": "notifications",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "notifications.feedback-linked-detail",
  "german": "{0} Feedback-Datens\u00E4tze erfolgreich verkn\u00FCpft",
  "english": "{0} feedback record(s) linked successfully",
  "category": "notifications",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "notifications.feedback-link-failed-title",
  "german": "Verkn\u00FCpfung fehlgeschlagen",
  "english": "Link Failed",
  "category": "notifications",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "notifications.feedback-link-failed-detail",
  "german": "{0} Feedback-Datens\u00E4tze konnten nicht verkn\u00FCpft werden",
  "english": "Failed to link {0} feedback record(s)",
  "category": "notifications",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "messages.no-feedback-linked",
  "german": "Kein Feedback mit diesem Fragebogen verkn\u00FCpft",
  "english": "No feedback linked to this questionnaire",
  "category": "messages",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "feedback-source.customer",
  "german": "Kunde",
  "english": "Customer",
  "category": "feedback-source",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "feedback-source.peer",
  "german": "Kollege",
  "english": "Peer",
  "category": "feedback-source",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "feedback-source.project-colleague",
  "german": "Projektkollege",
  "english": "Project Colleague",
  "category": "feedback-source",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "feedback-source.unknown",
  "german": "Unbekannt",
  "english": "Unknown",
  "category": "feedback-source",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
},
{
  "key": "labels.linked",
  "german": "verkn\u00FCpft",
  "english": "linked",
  "category": "labels",
  "createdDate": "2026-01-13T00:00:00.0000000+00:00"
}
```

---

## Critical Files to Modify

### Domain Layer
- `01_Domain/QuestionnaireAssignmentAggregate/QuestionnaireAssignment.cs` (add linking methods)
- `01_Domain/QuestionnaireAssignmentAggregate/Events/` (add 2 new event files)
- `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/EmployeeFeedbackConfiguration.cs` (verify/simplify)

### Application Layer
- `02_Application/Application.Command/Commands/QuestionnaireAssignmentCommands/` (add 4 new files)
- `02_Application/Application.Query/Queries/QuestionnaireAssignmentQueries/` (add 4 new files)
- `02_Application/Application.Query/DTOs/` (add 2 new DTOs)

### Infrastructure Layer
- `03_Infrastructure/CommandApi/Controllers/AssignmentsController.cs` (add 2 endpoints)
- `03_Infrastructure/CommandApi/DTOs/` (add 2 DTOs)
- `03_Infrastructure/QueryApi/Controllers/AssignmentsController.cs` (add 2 endpoints)

### Frontend Layer
- `05_Frontend/Services/EmployeeFeedbackApiService.cs` (new service)
- `05_Frontend/Services/QuestionHandlers/EmployeeFeedbackQuestionHandler.cs` (new handler)
- `05_Frontend/Components/Dialogs/LinkEmployeeFeedbackDialog.razor` (new dialog)
- `05_Frontend/Components/Questions/OptimizedEmployeeFeedbackQuestion.razor` (new renderer)
- `05_Frontend/Components/Questions/OptimizedQuestionRenderer.razor` (add case)
- `05_Frontend/Components/QuestionnaireBuilder/Renderers/EmployeeFeedbackQuestionRenderer.razor` (new renderer)
- `05_Frontend/Pages/InitializeAssignment.razor` (add feedback linking section)
- `05_Frontend/Models/Dto/` (add 4 matching DTOs)
- `05_Frontend/Program.cs` (register service and handler)

### Translations
- `TestDataGenerator/test-translations.json` (add 24 new translation keys)

---

## Verification Strategy

### Unit Tests
1. Test `QuestionnaireAssignment.LinkEmployeeFeedback()` domain method:
   - Valid linking succeeds
   - Duplicate linking throws exception
   - Validation for non-EmployeeFeedback question types
   - Validation for locked/withdrawn assignments

2. Test Apply methods for new events:
   - `Apply(EmployeeFeedbackLinkedToAssignment)` updates `_linkedFeedback`
   - `Apply(EmployeeFeedbackUnlinkedFromAssignment)` removes from `_linkedFeedback`

### Integration Tests
1. Test command handlers:
   - `LinkEmployeeFeedbackCommandHandler` creates correct domain event
   - Validation: feedback belongs to same employee as assignment
   - Validation: feedback is not deleted

2. Test query handlers:
   - `GetAvailableEmployeeFeedbackQueryHandler` returns only non-deleted feedback
   - `GetFeedbackQuestionDataQueryHandler` retrieves linked feedback correctly

### E2E Manual Testing Checklist
1. **Questionnaire Builder**:
   - [ ] Can add EmployeeFeedback section to template
   - [ ] EmployeeFeedback renderer shows info message (no config UI)
   - [ ] Template saves with EmployeeFeedback section

2. **Assignment Creation**:
   - [ ] Creating assignment from template with EmployeeFeedback section works
   - [ ] Assignment starts in Assigned state

3. **Initialization Phase**:
   - [ ] InitializeAssignment page shows "Link Employee Feedback" section
   - [ ] Clicking "Link Feedback" opens LinkEmployeeFeedbackDialog
   - [ ] Dialog lists available feedback for employee
   - [ ] Can select multiple feedback records
   - [ ] Linking feedback succeeds with success notification
   - [ ] Linked count badge updates correctly
   - [ ] Can complete initialization after linking feedback

4. **Workflow Display**:
   - [ ] During EmployeeInProgress: OptimizedEmployeeFeedbackQuestion shows linked feedback read-only
   - [ ] Feedback displays correctly with provider, date, ratings, comments
   - [ ] Multiple linked feedbacks all display correctly
   - [ ] No edit capability (read-only enforcement)

5. **Translation Validation**:
   - [ ] All UI text uses @T() translation keys
   - [ ] German translations display correctly
   - [ ] Language switching works (EN ↔ DE)

6. **Error Handling**:
   - [ ] Linking deleted feedback shows error
   - [ ] Linking feedback to wrong question type shows error
   - [ ] Linking already-linked feedback shows error
   - [ ] Network errors show user-friendly notifications

---

## Implementation Notes

### Design Decisions
1. **Multiple feedback per section**: Allows comprehensive feedback context (all customer feedback, all peer feedback, etc.)
2. **Read-only display**: Feedback is historical context, not editable within questionnaire workflow
3. **Linking during initialization**: Follows established pattern (like predecessor linking for goals)
4. **Minimal template configuration**: Like Goals, feedback sections are just placeholders

### Key Differences from Goals
| Aspect | Goals | Employee Feedback |
|--------|-------|-------------------|
| Template Config | Minimal (visibility flag) | Minimal (visibility flag) |
| Data Storage | QuestionnaireResponse.SectionResponses | Separate EmployeeFeedback aggregate + links in QuestionnaireAssignment |
| Creation | Added during workflow by Employee/Manager | Pre-existing records linked during initialization |
| Editing | Editable during workflow | Read-only (historical context) |
| Cardinality | Multiple goals per section | Multiple feedback records per section |
| Linking | Optional predecessor for comparison | Required for display (no feedback = empty section) |

### Security Considerations
1. **Authorization**: Only TeamLead+ can link feedback (Assigned state operations)
2. **Validation**: Feedback must belong to same employee as assignment
3. **Soft-delete check**: Deleted feedback cannot be linked
4. **Workflow validation**: Cannot link to locked/withdrawn assignments

### Performance Considerations
1. **Lazy loading**: Feedback data fetched only when section is rendered
2. **Caching**: Consider caching linked feedback data in frontend for duration of page session
3. **Batch queries**: If assignment has multiple EmployeeFeedback sections, consider batching queries

---

## Future Enhancements (Out of Scope)
- Filter feedback by source type during linking
- Filter feedback by date range during linking
- Auto-suggest feedback based on questionnaire period
- Unlink feedback functionality in UI
- Feedback usage tracking (which feedback records are linked to how many questionnaires)
- Export questionnaire with embedded feedback data
