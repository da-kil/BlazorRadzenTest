namespace ti8m.BeachBreak.Client.Models;

public enum TemplatePermission
{
    // Basic operations
    View,
    Create,
    Edit,
    Delete,

    // Publishing operations
    Publish,
    Unpublish,
    RequestPublishing,     // Request approval to publish
    ApprovePublishing,     // Approve publishing requests

    // Status management
    Activate,
    Deactivate,

    // Advanced operations
    ApproveForPublishing,
    ViewAuditTrail,
    ManagePermissions
}

public enum ApprovalStatus
{
    PendingApproval,
    Approved,
    Rejected,
    RequestWithdrawn
}

public class PublishingRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; } = DateTime.Now;
    public string RequestReason { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PendingApproval;
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewComments { get; set; }
}

public enum AuditAction
{
    Created,
    Modified,
    Published,
    Unpublished,
    Activated,
    Deactivated,
    Deleted,
    PublishingRequested,
    PublishingApproved,
    PublishingRejected,
    Cloned,
    Assigned
}

public class AuditTrailEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public AuditAction Action { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? Details { get; set; }
    public string? PreviousValues { get; set; }  // JSON of changed fields
    public string? NewValues { get; set; }       // JSON of new values
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public enum UserRole
{
    // Basic roles
    Employee,           // Can view assigned questionnaires
    TeamLead,          // Can manage team questionnaires
    HRManager,         // Can create and assign questionnaires

    // Template management roles
    TemplateEditor,    // Can create and edit templates
    TemplateReviewer,  // Can review and approve templates
    TemplateAdmin,     // Full template management access

    // System roles
    SystemAdmin        // Full system access
}

public static class TemplatePermissions
{
    private static readonly Dictionary<UserRole, HashSet<TemplatePermission>> RolePermissions = new()
    {
        [UserRole.Employee] = new HashSet<TemplatePermission>
        {
            TemplatePermission.View
        },

        [UserRole.TeamLead] = new HashSet<TemplatePermission>
        {
            TemplatePermission.View
        },

        [UserRole.HRManager] = new HashSet<TemplatePermission>
        {
            TemplatePermission.View,
            TemplatePermission.Create,
            TemplatePermission.Edit,
            TemplatePermission.RequestPublishing
        },

        [UserRole.TemplateEditor] = new HashSet<TemplatePermission>
        {
            TemplatePermission.View,
            TemplatePermission.Create,
            TemplatePermission.Edit,
            TemplatePermission.Delete,
            TemplatePermission.Activate,
            TemplatePermission.Deactivate,
            TemplatePermission.RequestPublishing
        },

        [UserRole.TemplateReviewer] = new HashSet<TemplatePermission>
        {
            TemplatePermission.View,
            TemplatePermission.Edit,
            TemplatePermission.Publish,
            TemplatePermission.Unpublish,
            TemplatePermission.ApprovePublishing,
            TemplatePermission.ApproveForPublishing,
            TemplatePermission.ViewAuditTrail
        },

        [UserRole.TemplateAdmin] = new HashSet<TemplatePermission>
        {
            TemplatePermission.View,
            TemplatePermission.Create,
            TemplatePermission.Edit,
            TemplatePermission.Delete,
            TemplatePermission.Publish,
            TemplatePermission.Unpublish,
            TemplatePermission.Activate,
            TemplatePermission.Deactivate,
            TemplatePermission.ApproveForPublishing,
            TemplatePermission.ViewAuditTrail,
            TemplatePermission.ManagePermissions
        },

        [UserRole.SystemAdmin] = new HashSet<TemplatePermission>
        {
            TemplatePermission.View,
            TemplatePermission.Create,
            TemplatePermission.Edit,
            TemplatePermission.Delete,
            TemplatePermission.Publish,
            TemplatePermission.Unpublish,
            TemplatePermission.Activate,
            TemplatePermission.Deactivate,
            TemplatePermission.ApproveForPublishing,
            TemplatePermission.ViewAuditTrail,
            TemplatePermission.ManagePermissions
        }
    };

    public static bool HasPermission(string role, TemplatePermission permission)
    {
        if (Enum.TryParse<UserRole>(role, true, out var userRole))
        {
            return RolePermissions.TryGetValue(userRole, out var permissions) &&
                   permissions.Contains(permission);
        }

        // Default to Employee role if parsing fails
        return RolePermissions[UserRole.Employee].Contains(permission);
    }

    public static bool HasPermission(UserRole role, TemplatePermission permission)
    {
        return RolePermissions.TryGetValue(role, out var permissions) &&
               permissions.Contains(permission);
    }

    public static HashSet<TemplatePermission> GetPermissions(string role)
    {
        if (Enum.TryParse<UserRole>(role, true, out var userRole))
        {
            return RolePermissions.TryGetValue(userRole, out var permissions)
                ? permissions
                : RolePermissions[UserRole.Employee];
        }

        return RolePermissions[UserRole.Employee];
    }

    public static HashSet<TemplatePermission> GetPermissions(UserRole role)
    {
        return RolePermissions.TryGetValue(role, out var permissions)
            ? permissions
            : RolePermissions[UserRole.Employee];
    }

    public static bool CanPublish(string role)
    {
        return HasPermission(role, TemplatePermission.Publish);
    }

    public static bool CanEdit(string role)
    {
        return HasPermission(role, TemplatePermission.Edit);
    }

    public static bool CanDelete(string role)
    {
        return HasPermission(role, TemplatePermission.Delete);
    }

    public static bool CanApprove(string role)
    {
        return HasPermission(role, TemplatePermission.ApproveForPublishing);
    }
}

public class TemplateValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static TemplateValidationResult Success() => new() { IsValid = true };

    public static TemplateValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };

    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}

public static class TemplateValidator
{
    public static TemplateValidationResult ValidateForPublishing(QuestionnaireTemplate template)
    {
        var result = new TemplateValidationResult { IsValid = true };

        // Basic template validation
        if (string.IsNullOrWhiteSpace(template.Name))
        {
            result.AddError("Template name is required");
        }

        if (string.IsNullOrWhiteSpace(template.Description))
        {
            result.AddWarning("Template description is recommended");
        }

        // Section validation
        if (template.Sections == null || template.Sections.Count == 0)
        {
            result.AddError("Template must have at least one section");
        }
        else
        {
            for (int i = 0; i < template.Sections.Count; i++)
            {
                var section = template.Sections[i];

                if (string.IsNullOrWhiteSpace(section.Title))
                {
                    result.AddError($"Section {i + 1} must have a title");
                }

                // Question validation
                if (section.Questions == null || section.Questions.Count == 0)
                {
                    result.AddError($"Section '{section.Title}' must have at least one question");
                }
                else
                {
                    for (int j = 0; j < section.Questions.Count; j++)
                    {
                        var question = section.Questions[j];

                        if (string.IsNullOrWhiteSpace(question.Title))
                        {
                            result.AddError($"Question {j + 1} in section '{section.Title}' must have a title");
                        }

                        // Type-specific validation
                        if (question.Type == QuestionType.SelfAssessment || question.Type == QuestionType.GoalAchievement)
                        {
                            if (question.Options == null || question.Options.Count == 0)
                            {
                                result.AddWarning($"Question '{question.Title}' should have rating options defined");
                            }
                        }
                    }
                }
            }
        }

        // Publishing-specific validation
        if (!template.IsActive)
        {
            result.AddError("Template must be active before publishing");
        }

        return result;
    }

    public static TemplateValidationResult ValidateForEdit(QuestionnaireTemplate template)
    {
        var result = new TemplateValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(template.Name))
        {
            result.AddError("Template name is required");
        }

        return result;
    }
}