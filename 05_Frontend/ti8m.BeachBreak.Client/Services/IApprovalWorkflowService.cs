using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IApprovalWorkflowService
{
    Task<PublishingRequest?> RequestPublishingAsync(Guid templateId, string reason);
    Task<List<PublishingRequest>> GetPendingRequestsAsync();
    Task<List<PublishingRequest>> GetMyRequestsAsync();
    Task<PublishingRequest?> ApproveRequestAsync(Guid requestId, string comments);
    Task<PublishingRequest?> RejectRequestAsync(Guid requestId, string comments);
    Task<PublishingRequest?> WithdrawRequestAsync(Guid requestId);
    Task<List<AuditTrailEntry>> GetAuditTrailAsync(Guid templateId);
    Task<AuditTrailEntry> LogActionAsync(Guid templateId, AuditAction action, string? details = null);
}