using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.ProjectionReplayQueries;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/admin/replay")]
[Authorize(Roles = "Admin")]
public class ReplayController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<ReplayController> logger;

    public ReplayController(
        IQueryDispatcher queryDispatcher,
        ILogger<ReplayController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    [HttpGet("{replayId:guid}")]
    [ProducesResponseType(typeof(ProjectionReplayReadModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReplayStatus(Guid replayId)
    {
        try
        {
            logger.LogInformation("Retrieving replay status for replay {ReplayId}", replayId);
            var query = new GetReplayStatusQuery(replayId);
            var result = await queryDispatcher.QueryAsync(query);

            if (result != null)
            {
                logger.LogInformation("Successfully retrieved replay status for replay {ReplayId}", replayId);
            }
            else
            {
                logger.LogWarning("Replay {ReplayId} not found", replayId);
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving replay status for replay {ReplayId}", replayId);
            return StatusCode(500, "An error occurred while retrieving the replay status");
        }
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<ProjectionReplayReadModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReplayHistory([FromQuery] int limit = 50)
    {
        try
        {
            logger.LogInformation("Retrieving replay history with limit {Limit}", limit);
            var query = new GetReplayHistoryQuery(limit);
            var result = await queryDispatcher.QueryAsync(query);

            if (result.Succeeded && result.Payload != null)
            {
                var count = result.Payload.Count();
                logger.LogInformation("Successfully retrieved {Count} replay history entries", count);
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving replay history");
            return StatusCode(500, "An error occurred while retrieving the replay history");
        }
    }

    [HttpGet("projections")]
    [ProducesResponseType(typeof(IEnumerable<ProjectionInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvailableProjections([FromQuery] bool rebuildableOnly = true)
    {
        try
        {
            logger.LogInformation("Retrieving available projections (rebuildableOnly: {RebuildableOnly})", rebuildableOnly);
            var query = new GetAvailableProjectionsQuery(rebuildableOnly);
            var result = await queryDispatcher.QueryAsync(query);

            if (result.Succeeded && result.Payload != null)
            {
                var count = result.Payload.Count();
                logger.LogInformation("Successfully retrieved {Count} available projections", count);
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving available projections");
            return StatusCode(500, "An error occurred while retrieving the available projections");
        }
    }
}
