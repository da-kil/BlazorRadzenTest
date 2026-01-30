# Authorization Implementation Patterns

## Overview

This guide covers authorization patterns for Blazor components, including page-level security and role-based access control.

---

## Page-Level Authorization Pattern

### Standard Authorization Pattern for Admin Pages

**ALWAYS** use the AuthorizeView component for admin pages instead of `@attribute [Authorize]`:

```razor
@page "/admin/some-page"
@using Microsoft.AspNetCore.Components.Authorization

<AuthorizeView Policy="PolicyName">
    <Authorized>
        <StandardPageLayout Title="Page Title" Description="Page description">
            <!-- Page content here -->
        </StandardPageLayout>
    </Authorized>
    <NotAuthorized>
        <AccessDeniedComponent RequiredRole="RequiredRole" PageName="Descriptive Page Name" />
    </NotAuthorized>
</AuthorizeView>
```

### Authorization Policies Available

1. **Employee** - All authenticated users (Employee+)
2. **TeamLead** - Team leads and above (TeamLead, HR, HRLead, Admin)
3. **HR** - HR staff and above (HR, HRLead, Admin)
4. **HRLead** - HR leads and above (HRLead, Admin)
5. **Admin** - Administrator only

### Pages Using AuthorizeView Pattern

**Admin Pages (HR Policy)**:
- HRDashboard.razor
- QuestionnaireManagement.razor
- QuestionnaireBuilder.razor
- QuestionnaireAssignments.razor
- CategoryAdmin.razor
- RoleManagement.razor
- OrganizationQuestionnaires.razor

**Manager Pages (TeamLead Policy)**:
- ManagerDashboard.razor
- TeamQuestionnaires.razor

**System Admin (Admin Policy)**:
- ProjectionReplayAdmin.razor

**Employee Pages (Generic @attribute [Authorize])**:
- Dashboard.razor
- MyQuestionnaires.razor
- DynamicQuestionnaire.razor
- Home.razor

### Key Benefits of AuthorizeView Pattern

1. **Consistent UX**: Users get clear "Access Denied" messages instead of blank pages
2. **Professional Error Handling**: Custom error UI with icons and explanations
3. **Maintainable**: Centralized AccessDeniedComponent for consistent styling
4. **User-Friendly**: Clear messaging about required permissions

### AccessDeniedComponent Usage

The reusable component accepts two parameters:
- `RequiredRole`: Display name of required role (e.g., "HR", "Admin", "TeamLead")
- `PageName`: Descriptive name of the page (e.g., "Questionnaire Builder", "Role Management")

### When to Use Each Pattern

- **Use AuthorizeView**: For admin pages where users need clear feedback about access restrictions
- **Use @attribute [Authorize]**: For employee-accessible pages where generic authentication is sufficient

## Historical Context

### Authorization Pattern Standardization (2025-11-21)

This pattern was standardized to replace inconsistent authorization approaches. Previously, most admin pages used `@attribute [Authorize(Policy = "...")]` which resulted in blank pages for unauthorized users, while only RoleManagement.razor provided proper error messages.

**Before Standardization**:
- Inconsistent user experience across admin pages
- Some pages showed blank screens for unauthorized users
- No clear indication of required permissions
- Mix of authorization approaches

**After Standardization**:
- Consistent "Access Denied" messages across all admin pages
- Professional error UI with clear permission requirements
- Centralized AccessDeniedComponent for maintainability
- Clear distinction between admin and employee pages

---

## References

- **CLAUDE.md**: Core authorization pattern rule
- **AccessDeniedComponent.razor**: Reusable access denied UI component
- **Authorization Policies**: Configured in Program.cs

---

*Last Updated: 2026-01-30*
*Document Version: 1.0*