using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.OrganizationQueries;
using ti8m.BeachBreak.Core.Infrastructure;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for organization queries.
/// </summary>
public static class OrganizationEndpoints
{
    /// <summary>
    /// Maps organization query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapOrganizationEndpoints(this WebApplication app)
    {
        var organizationGroup = app.MapGroup("/q/api/v{version:apiVersion}/organizations")
            .WithTags("Organizations")
            .RequireAuthorization();

        // Get all organizations
        organizationGroup.MapGet("/", async (
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            bool includeDeleted = false,
            bool includeIgnored = false,
            Guid? parentId = null,
            string? managerId = null,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogGetAllOrganizationsRequest(includeDeleted, includeIgnored, parentId, managerId);

            try
            {
                var query = new OrganizationListQuery
                {
                    IncludeDeleted = includeDeleted,
                    IncludeIgnored = includeIgnored,
                    ParentId = parentId,
                    ManagerId = managerId
                };

                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    var organizationDtos = result.Payload!.Select(MapToDto);
                    logger.LogGetAllOrganizationsSuccess(organizationDtos.Count());
                    return Results.Ok(organizationDtos);
                }

                logger.LogGetAllOrganizationsFailed(result.Message ?? "Unknown error");
                return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogGetAllOrganizationsError(ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An unexpected error occurred",
                    statusCode: 500);
            }
        })
        .WithName("GetAllOrganizations")
        .WithSummary("Get all organizations")
        .WithDescription("Retrieves all organizations with optional filtering parameters")
        .Produces<IEnumerable<OrganizationDto>>(200)
        .Produces(500);

        // Get organization by ID
        organizationGroup.MapGet("/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogGetOrganizationByIdRequest(id);

            try
            {
                var query = new OrganizationQuery(id);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    if (result.Payload == null)
                    {
                        logger.LogOrganizationByIdNotFound(id);
                        return Results.NotFound();
                    }

                    var organizationDto = MapToDto(result.Payload);
                    logger.LogGetOrganizationByIdSuccess(id);
                    return Results.Ok(organizationDto);
                }

                logger.LogGetOrganizationByIdFailed(id, result.Message ?? "Unknown error");
                return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogGetOrganizationByIdError(ex, id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An unexpected error occurred",
                    statusCode: 500);
            }
        })
        .WithName("GetOrganizationById")
        .WithSummary("Get organization by ID")
        .WithDescription("Retrieves a specific organization by its ID")
        .Produces<OrganizationDto>(200)
        .Produces(404)
        .Produces(500);

        // Get organization by number
        organizationGroup.MapGet("/by-number/{number}", async (
            string number,
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogGetOrganizationByNumberRequest(number);

            try
            {
                var query = new OrganizationByNumberQuery(number);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    if (result.Payload == null)
                    {
                        logger.LogOrganizationByNumberNotFound(number);
                        return Results.NotFound();
                    }

                    var organizationDto = MapToDto(result.Payload);
                    logger.LogGetOrganizationByNumberSuccess(number);
                    return Results.Ok(organizationDto);
                }

                logger.LogGetOrganizationByNumberFailed(number, result.Message ?? "Unknown error");
                return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogGetOrganizationByNumberError(ex, number);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An unexpected error occurred",
                    statusCode: 500);
            }
        })
        .WithName("GetOrganizationByNumber")
        .WithSummary("Get organization by number")
        .WithDescription("Retrieves a specific organization by its number")
        .Produces<OrganizationDto>(200)
        .Produces(404)
        .Produces(500);
    }

    private static OrganizationDto MapToDto(Organization organization)
    {
        return new OrganizationDto
        {
            Id = organization.Id,
            Number = organization.Number,
            ManagerId = organization.ManagerId,
            ParentId = organization.ParentId,
            Name = organization.Name,
            IsIgnored = organization.IsIgnored,
            IsDeleted = organization.IsDeleted
        };
    }
}