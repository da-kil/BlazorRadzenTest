using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;

namespace ti8m.BeachBreak.CommandApi.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult CreateResponse(Result result)
    {
        return result.Succeeded ? Ok(result) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }

    protected IActionResult CreateResponse<T>(Result<T> result)
    {
        return result.Succeeded ? Ok(result) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }
}