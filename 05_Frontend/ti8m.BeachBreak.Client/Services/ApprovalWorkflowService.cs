using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class ApprovalWorkflowService : IApprovalWorkflowService
{
    private readonly IPermissionService permissionService;
    private readonly List<PublishingRequest> _publishingRequests = new();
    private readonly List<AuditTrailEntry> _auditTrail = new();

    public ApprovalWorkflowService(IPermissionService permissionService)
    {
        this.permissionService = permissionService;
        // Initialize with some sample data for testing
        InitializeSampleData();
    }

    public async Task<PublishingRequest?> RequestPublishingAsync(Guid templateId, string reason)
    {
        if (!await permissionService.HasPermissionAsync(TemplatePermission.RequestPublishing))
        {
            return null;
        }

        var request = new PublishingRequest
        {
            TemplateId = templateId,
            RequestedBy = permissionService.GetCurrentUserName(),
            RequestReason = reason,
            Status = ApprovalStatus.PendingApproval
        };

        _publishingRequests.Add(request);

        // Log the action
        await LogActionAsync(templateId, AuditAction.PublishingRequested,
            $"Publishing approval requested. Reason: {reason}");

        return request;
    }

    public async Task<List<PublishingRequest>> GetPendingRequestsAsync()
    {
        if (!await permissionService.HasPermissionAsync(TemplatePermission.ApprovePublishing))
        {
            return new List<PublishingRequest>();
        }

        return _publishingRequests
            .Where(r => r.Status == ApprovalStatus.PendingApproval)
            .OrderBy(r => r.RequestedDate)
            .ToList();
    }

    public async Task<List<PublishingRequest>> GetMyRequestsAsync()
    {
        var currentUser = permissionService.GetCurrentUserName();
        return _publishingRequests
            .Where(r => r.RequestedBy == currentUser)
            .OrderByDescending(r => r.RequestedDate)
            .ToList();
    }

    public async Task<PublishingRequest?> ApproveRequestAsync(Guid requestId, string comments)
    {
        if (!await permissionService.HasPermissionAsync(TemplatePermission.ApprovePublishing))
        {
            return null;
        }

        var request = _publishingRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null || request.Status != ApprovalStatus.PendingApproval)
        {
            return null;
        }

        request.Status = ApprovalStatus.Approved;
        request.ReviewedBy = permissionService.GetCurrentUserName();
        request.ReviewedDate = DateTime.Now;
        request.ReviewComments = comments;

        // Log the action
        await LogActionAsync(request.TemplateId, AuditAction.PublishingApproved,
            $"Publishing request approved by {request.ReviewedBy}. Comments: {comments}");

        return request;
    }

    public async Task<PublishingRequest?> RejectRequestAsync(Guid requestId, string comments)
    {
        if (!await permissionService.HasPermissionAsync(TemplatePermission.ApprovePublishing))
        {
            return null;
        }

        var request = _publishingRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null || request.Status != ApprovalStatus.PendingApproval)
        {
            return null;
        }

        request.Status = ApprovalStatus.Rejected;
        request.ReviewedBy = permissionService.GetCurrentUserName();
        request.ReviewedDate = DateTime.Now;
        request.ReviewComments = comments;

        // Log the action
        await LogActionAsync(request.TemplateId, AuditAction.PublishingRejected,
            $"Publishing request rejected by {request.ReviewedBy}. Comments: {comments}");

        return request;
    }

    public async Task<PublishingRequest?> WithdrawRequestAsync(Guid requestId)
    {
        var currentUser = permissionService.GetCurrentUserName();
        var request = _publishingRequests.FirstOrDefault(r => r.Id == requestId && r.RequestedBy == currentUser);

        if (request == null || request.Status != ApprovalStatus.PendingApproval)
        {
            return null;
        }

        request.Status = ApprovalStatus.RequestWithdrawn;

        // Log the action
        await LogActionAsync(request.TemplateId, AuditAction.PublishingRejected,
            $"Publishing request withdrawn by {currentUser}");

        return request;
    }

    public async Task<List<AuditTrailEntry>> GetAuditTrailAsync(Guid templateId)
    {
        if (!await permissionService.HasPermissionAsync(TemplatePermission.ViewAuditTrail))
        {
            return new List<AuditTrailEntry>();
        }

        return _auditTrail
            .Where(a => a.TemplateId == templateId)
            .OrderByDescending(a => a.Timestamp)
            .ToList();
    }

    public async Task<AuditTrailEntry> LogActionAsync(Guid templateId, AuditAction action, string? details = null)
    {
        var entry = new AuditTrailEntry
        {
            TemplateId = templateId,
            Action = action,
            PerformedBy = permissionService.GetCurrentUserName(),
            Details = details,
            Timestamp = DateTime.Now
        };

        _auditTrail.Add(entry);
        return entry;
    }

    private void InitializeSampleData()
    {
        // Add some sample requests for testing
        var sampleTemplateId = Guid.NewGuid();

        _publishingRequests.AddRange(new[]
        {
            new PublishingRequest
            {
                Id = Guid.NewGuid(),
                TemplateId = sampleTemplateId,
                RequestedBy = "John Editor",
                RequestedDate = DateTime.Now.AddDays(-2),
                RequestReason = "Ready for production use after extensive testing",
                Status = ApprovalStatus.PendingApproval
            },
            new PublishingRequest
            {
                Id = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                RequestedBy = "Sarah Manager",
                RequestedDate = DateTime.Now.AddDays(-1),
                RequestReason = "Approved by stakeholders, needs to go live",
                Status = ApprovalStatus.Approved,
                ReviewedBy = "Admin User",
                ReviewedDate = DateTime.Now.AddHours(-2),
                ReviewComments = "Looks good, approved for publishing"
            }
        });

        // Add sample audit trail entries
        _auditTrail.AddRange(new[]
        {
            new AuditTrailEntry
            {
                TemplateId = sampleTemplateId,
                Action = AuditAction.Created,
                PerformedBy = "John Editor",
                Timestamp = DateTime.Now.AddDays(-5),
                Details = "Template created with basic structure"
            },
            new AuditTrailEntry
            {
                TemplateId = sampleTemplateId,
                Action = AuditAction.Modified,
                PerformedBy = "John Editor",
                Timestamp = DateTime.Now.AddDays(-3),
                Details = "Added validation rules and improved question flow"
            },
            new AuditTrailEntry
            {
                TemplateId = sampleTemplateId,
                Action = AuditAction.PublishingRequested,
                PerformedBy = "John Editor",
                Timestamp = DateTime.Now.AddDays(-2),
                Details = "Publishing approval requested. Reason: Ready for production use after extensive testing"
            }
        });
    }
}