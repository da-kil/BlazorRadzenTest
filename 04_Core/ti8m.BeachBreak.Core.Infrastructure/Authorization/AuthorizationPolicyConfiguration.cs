using Microsoft.AspNetCore.Authorization;

namespace ti8m.BeachBreak.Core.Infrastructure.Authorization;

/// <summary>
/// Shared authorization policy configuration for API.
/// Defines claim-based policies that check the ApplicationRole claim.
/// Note: We use claim-based authorization instead of role-based because roles are stored
/// in the database, not in the authentication token.
/// </summary>
public static class AuthorizationPolicyConfiguration
{
    /// <summary>
    /// Configures authorization policies for the application.
    /// Policy names match ApplicationRole enum values for consistency.
    /// Each policy allows access to that role and all higher-privileged roles.
    /// </summary>
    /// <param name="options">The authorization options to configure.</param>
    public static void ConfigureAuthorizationPolicies(this AuthorizationOptions options)
    {
        // Employee policy: All authenticated employees can access (Employee, TeamLead, HR, HRLead, Admin)
        options.AddPolicy("Employee", policy =>
            policy.RequireClaim("ApplicationRole", "Employee", "TeamLead", "HR", "HRLead", "Admin"));

        // TeamLead policy: TeamLead and above can access (TeamLead, HR, HRLead, Admin)
        options.AddPolicy("TeamLead", policy =>
            policy.RequireClaim("ApplicationRole", "TeamLead", "HR", "HRLead", "Admin"));

        // HR policy: HR and above can access (HR, HRLead, Admin)
        options.AddPolicy("HR", policy =>
            policy.RequireClaim("ApplicationRole", "HR", "HRLead", "Admin"));

        // HRLead policy: HRLead and above can access (HRLead, Admin)
        options.AddPolicy("HRLead", policy =>
            policy.RequireClaim("ApplicationRole", "HRLead", "Admin"));

        // Admin policy: Only Admin can access
        options.AddPolicy("Admin", policy =>
            policy.RequireClaim("ApplicationRole", "Admin"));

        // AdminOrApp policy: Admin users OR service principals with DataSeeder app role
        // Used for bulk operations that can be called by automated scripts
        options.AddPolicy("AdminOrApp", policy =>
            policy.RequireAssertion(context =>
                // Allow users with Admin role
                context.User.HasClaim("ApplicationRole", "Admin") ||
                // OR allow service principals with DataSeeder app role
                context.User.HasClaim("roles", "DataSeeder") ||
                context.User.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "DataSeeder")
            ));

        // HROrApp policy: HR users OR service principals with DataSeeder app role
        // Used for bulk operations that can be called by automated scripts
        options.AddPolicy("HROrApp", policy =>
            policy.RequireAssertion(context =>
                // Allow users with HR, HRLead, or Admin roles
                context.User.HasClaim("ApplicationRole", "HR") ||
                context.User.HasClaim("ApplicationRole", "HRLead") ||
                context.User.HasClaim("ApplicationRole", "Admin") ||
                // OR allow service principals with DataSeeder app role
                context.User.HasClaim("roles", "DataSeeder") ||
                context.User.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "DataSeeder")
            ));

        // TeamLeadOrApp policy: TeamLead users OR service principals with DataSeeder app role
        // Used for bulk operations that can be called by automated scripts
        options.AddPolicy("TeamLeadOrApp", policy =>
            policy.RequireAssertion(context =>
                // Allow users with TeamLead, HR, HRLead, or Admin roles
                context.User.HasClaim("ApplicationRole", "TeamLead") ||
                context.User.HasClaim("ApplicationRole", "HR") ||
                context.User.HasClaim("ApplicationRole", "HRLead") ||
                context.User.HasClaim("ApplicationRole", "Admin") ||
                // OR allow service principals with DataSeeder app role
                context.User.HasClaim("roles", "DataSeeder") ||
                context.User.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "DataSeeder")
            ));
    }
}
