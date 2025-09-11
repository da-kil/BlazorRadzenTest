using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;

namespace ti8m.BeachBreak.CommandApi.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult CreateResponse(Result result)
    {
        return result.Succeeded ? Ok(string.IsNullOrWhiteSpace(result.Message) ? null : new { result.Message }) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }
}