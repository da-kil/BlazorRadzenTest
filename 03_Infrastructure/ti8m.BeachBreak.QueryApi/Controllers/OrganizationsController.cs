using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.OrganizationQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/organizations")]
public class OrganizationsController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<OrganizationsController> logger;

    public OrganizationsController(
        IQueryDispatcher queryDispatcher,
        ILogger<OrganizationsController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganizationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrganizations(
        [FromQuery] bool includeDeleted = false,
        [FromQuery] bool includeIgnored = false,
        [FromQuery] Guid? parentId = null,
        [FromQuery] string? managerId = null)
    {
        logger.LogInformation("Received GetAllOrganizations request - IncludeDeleted: {IncludeDeleted}, IncludeIgnored: {IncludeIgnored}, ParentId: {ParentId}, ManagerId: {ManagerId}",
            includeDeleted, includeIgnored, parentId, managerId);

        var query = new OrganizationListQuery
        {
            IncludeDeleted = includeDeleted,
            IncludeIgnored = includeIgnored,
            ParentId = parentId,
            ManagerId = managerId
        };

        var result = await queryDispatcher.QueryAsync(query);

        logger.LogInformation("Successfully returned {Count} organizations", result.Payload?.Count() ?? 0);
        return CreateResponse(result, organizations => organizations.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationById(Guid id)
    {
        logger.LogInformation("Received GetOrganizationById request for Id: {Id}", id);

        var query = new OrganizationQuery(id);
        var result = await queryDispatcher.QueryAsync(query);

        if (result.Succeeded && result.Payload != null)
        {
            logger.LogInformation("Successfully returned organization with Id: {Id}", id);
        }

        return CreateResponse(result, MapToDto);
    }

    [HttpGet("by-number/{number}")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationByNumber(string number)
    {
        logger.LogInformation("Received GetOrganizationByNumber request for Number: {Number}", number);

        var query = new OrganizationByNumberQuery(number);
        var result = await queryDispatcher.QueryAsync(query);

        if (result.Succeeded && result.Payload != null)
        {
            logger.LogInformation("Successfully returned organization with Number: {Number}", number);
        }

        return CreateResponse(result, MapToDto);
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