using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IPermissionService
{
    Task<string> GetCurrentUserRoleAsync();
    Task<bool> HasPermissionAsync(TemplatePermission permission);
    Task<bool> CanEditTemplateAsync(QuestionnaireTemplate template);
    Task<bool> CanPublishTemplateAsync(QuestionnaireTemplate template);
    Task<bool> CanDeleteTemplateAsync(QuestionnaireTemplate template);
    Task<TemplateValidationResult> ValidateTemplateForPublishingAsync(QuestionnaireTemplate template);
    Task<TemplateValidationResult> ValidateTemplateForEditAsync(QuestionnaireTemplate template);
    string GetCurrentUserId();
    string GetCurrentUserName();
}