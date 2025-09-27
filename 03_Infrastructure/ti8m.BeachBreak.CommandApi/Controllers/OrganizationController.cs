using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.OrganizationCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/organizations")]
public class OrganizationController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;

    public OrganizationController(ICommandDispatcher commandDispatcher)
    {
        this.commandDispatcher = commandDispatcher;
    }

    [HttpPost("bulk-import")]
    public async Task<IActionResult> BulkImportOrganizations([FromBody] IEnumerable<SyncOrganizationDto> organizations)
    {
        Result result = await commandDispatcher.SendAsync(new BulkImportOrganizationCommand(
            organizations.Select(dto => new SyncOrganization
            {
                Number = dto.Number,
                ParentNumber = dto.ParentNumber,
                Name = dto.Name,
                ManagerUserId = dto.ManagerUserId
            })));

        return CreateResponse(result);
    }

    [HttpPost("bulk-update")]
    [Authorize(Policy = "DataImportPolicy")]
    public async Task<IActionResult> BulkUpdateOrganizations([FromBody] IEnumerable<SyncOrganizationDto> organizations)
    {
        Result result = await commandDispatcher.SendAsync(new BulkUpdateOrganizationsCommand(
            organizations.Select(dto => new SyncOrganization
            {
                Number = dto.Number,
                ParentNumber = dto.ParentNumber,
                Name = dto.Name,
                ManagerUserId = dto.ManagerUserId
            })));

        return CreateResponse(result);
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = "DataImportPolicy")]
    public async Task<IActionResult> BulkDeleteOrganizations([FromBody] IEnumerable<SyncDeletedOrganizationDto> organizations)
    {
        Result result = await commandDispatcher.SendAsync(new BulkDeleteOrganizationsCommand(
            organizations.Select(dto => dto.OrgNumber)));

        return CreateResponse(result);
    }

    [HttpPut("{organizationId}/ignore")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public async Task<IActionResult> IgnoreOrganization(Guid organizationId)
    {
        Result result = await commandDispatcher.SendAsync(new IgnoreOrganizationCommand(organizationId));

        return CreateResponse(result);
    }
}