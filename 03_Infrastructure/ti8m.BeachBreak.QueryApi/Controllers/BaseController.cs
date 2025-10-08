using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;

namespace ti8m.BeachBreak.QueryApi.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult CreateResponse<TPayload, TMappedPayload>(Result<TPayload> result, Func<TPayload, TMappedPayload> map)
    {
        return result.Succeeded ? Ok(map(result.Payload!)) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }

    protected IActionResult CreateResponse<TPayload>(Result<TPayload> result)
    {
        return result.Succeeded ? Ok(result.Payload) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }

    protected IActionResult CreateResponse(Result result)
    {
        return result.Succeeded ? Ok(string.IsNullOrWhiteSpace(result.Message) ? null : new { result.Message }) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }
}