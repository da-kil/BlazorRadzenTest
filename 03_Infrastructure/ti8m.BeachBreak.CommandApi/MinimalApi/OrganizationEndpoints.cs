using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.OrganizationCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for organization management.
/// </summary>
public static class OrganizationEndpoints
{
    /// <summary>
    /// Maps organization management endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapOrganizationEndpoints(this WebApplication app)
    {
        var organizationGroup = app.MapGroup("/c/api/v{version:apiVersion}/organizations")
            .WithTags("Organizations");

        // Bulk import organizations
        organizationGroup.MapPost("/bulk-import", async (
            IEnumerable<SyncOrganizationDto> organizations,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                Result result = await commandDispatcher.SendAsync(new BulkImportOrganizationCommand(
                    organizations.Select(dto => new SyncOrganization
                    {
                        Number = dto.Number,
                        ParentNumber = dto.ParentNumber,
                        Name = dto.Name,
                        ManagerUserId = dto.ManagerUserId
                    })), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Organizations imported successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Organization import failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while importing organizations",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("AdminOrApp") // Allows Admin users OR service principals with DataSeeder app role
        .WithName("BulkImportOrganizations")
        .WithSummary("Bulk import organizations")
        .WithDescription("Imports multiple organizations from external source")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Bulk update organizations
        organizationGroup.MapPost("/bulk-update", async (
            IEnumerable<SyncOrganizationDto> organizations,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                Result result = await commandDispatcher.SendAsync(new BulkUpdateOrganizationsCommand(
                    organizations.Select(dto => new SyncOrganization
                    {
                        Number = dto.Number,
                        ParentNumber = dto.ParentNumber,
                        Name = dto.Name,
                        ManagerUserId = dto.ManagerUserId
                    })), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Organizations updated successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Organization update failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while updating organizations",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("Admin")
        .WithName("BulkUpdateOrganizations")
        .WithSummary("Bulk update organizations")
        .WithDescription("Updates multiple organizations")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Bulk delete organizations
        organizationGroup.MapPost("/bulk-delete", async (
            IEnumerable<SyncDeletedOrganizationDto> organizations,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                Result result = await commandDispatcher.SendAsync(new BulkDeleteOrganizationsCommand(
                    organizations.Select(dto => dto.OrgNumber)), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Organizations deleted successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Organization deletion failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while deleting organizations",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("Admin")
        .WithName("BulkDeleteOrganizations")
        .WithSummary("Bulk delete organizations")
        .WithDescription("Deletes multiple organizations")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Ignore organization
        organizationGroup.MapPut("/{organizationId:guid}/ignore", async (
            Guid organizationId,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                Result result = await commandDispatcher.SendAsync(new IgnoreOrganizationCommand(organizationId), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Organization ignored successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Organization ignore failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while ignoring the organization",
                    statusCode: 500);
            }
        })
        .WithName("IgnoreOrganization")
        .WithSummary("Ignore an organization")
        .WithDescription("Marks an organization to be ignored in processing")
        .Produces(200)
        .Produces(400)
        .Produces(500);
    }
}