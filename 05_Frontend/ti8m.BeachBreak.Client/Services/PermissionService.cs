using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class PermissionService : IPermissionService
{
    private readonly IEmployeeApiService employeeApiService;
    private string? _currentUserRole;

    public PermissionService(IEmployeeApiService employeeApiService)
    {
        this.employeeApiService = employeeApiService;
    }

    public async Task<string> GetCurrentUserRoleAsync()
    {
        if (_currentUserRole != null)
        {
            return _currentUserRole;
        }

        try
        {
            // For now, simulate getting the user role
            // In a real application, this would come from authentication context
            // TODO: Replace with actual user context implementation
            _currentUserRole = UserRole.TemplateAdmin.ToString(); // Default to admin for testing
            return _currentUserRole;
        }
        catch (Exception)
        {
            // Fallback to Employee role in case of error
            _currentUserRole = UserRole.Employee.ToString();
            return _currentUserRole;
        }
    }

    public async Task<bool> HasPermissionAsync(TemplatePermission permission)
    {
        var role = await GetCurrentUserRoleAsync();
        return TemplatePermissions.HasPermission(role, permission);
    }

    public async Task<bool> CanEditTemplateAsync(QuestionnaireTemplate template)
    {
        var role = await GetCurrentUserRoleAsync();

        // Basic edit permission check
        if (!TemplatePermissions.HasPermission(role, TemplatePermission.Edit))
        {
            return false;
        }

        // Business logic: Can't edit published templates unless you're a reviewer or admin
        if (template.IsPublished)
        {
            return TemplatePermissions.HasPermission(role, TemplatePermission.ApproveForPublishing) ||
                   TemplatePermissions.HasPermission(role, TemplatePermission.ManagePermissions);
        }

        return true;
    }

    public async Task<bool> CanPublishTemplateAsync(QuestionnaireTemplate template)
    {
        var role = await GetCurrentUserRoleAsync();

        // Basic publish permission check
        if (!TemplatePermissions.HasPermission(role, TemplatePermission.Publish))
        {
            return false;
        }

        // Business logic: Template must be active and valid
        if (!template.IsActive)
        {
            return false;
        }

        // Validate template content
        var validation = await ValidateTemplateForPublishingAsync(template);
        return validation.IsValid;
    }

    public async Task<bool> CanDeleteTemplateAsync(QuestionnaireTemplate template)
    {
        var role = await GetCurrentUserRoleAsync();

        // Basic delete permission check
        if (!TemplatePermissions.HasPermission(role, TemplatePermission.Delete))
        {
            return false;
        }

        // Business logic: Can't delete published templates unless you're an admin
        if (template.IsPublished)
        {
            return TemplatePermissions.HasPermission(role, TemplatePermission.ManagePermissions);
        }

        return true;
    }

    public async Task<TemplateValidationResult> ValidateTemplateForPublishingAsync(QuestionnaireTemplate template)
    {
        var role = await GetCurrentUserRoleAsync();
        var result = TemplateValidator.ValidateForPublishing(template);

        // Add role-specific validation
        if (!TemplatePermissions.HasPermission(role, TemplatePermission.Publish))
        {
            result.AddError($"User role '{role}' does not have permission to publish templates");
        }

        return result;
    }

    public async Task<TemplateValidationResult> ValidateTemplateForEditAsync(QuestionnaireTemplate template)
    {
        var role = await GetCurrentUserRoleAsync();
        var result = TemplateValidator.ValidateForEdit(template);

        // Add role-specific validation
        if (!TemplatePermissions.HasPermission(role, TemplatePermission.Edit))
        {
            result.AddError($"User role '{role}' does not have permission to edit templates");
        }

        // Check if template is published and user can edit published templates
        if (template.IsPublished && !TemplatePermissions.HasPermission(role, TemplatePermission.ApproveForPublishing))
        {
            result.AddError("Cannot edit published templates. Template must be unpublished first.");
        }

        return result;
    }

    public string GetCurrentUserId()
    {
        // TODO: Replace with actual user context implementation
        return "admin-user-id";
    }

    public string GetCurrentUserName()
    {
        // TODO: Replace with actual user context implementation
        return "Admin User";
    }
}