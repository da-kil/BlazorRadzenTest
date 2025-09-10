using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;

namespace ti8m.BeachBreak.QueryApi.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult CreateResponse<TPayload, TMappedPayload>(Result<TPayload> result, Func<TPayload, TMappedPayload> map)
    {
        return result.Succeeded ? Ok(map(result.Payload!)) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }
}