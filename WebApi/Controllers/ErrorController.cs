using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RecruitmentApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErrorController : ControllerBase
{
    private readonly IHostEnvironment _environment;

    public ErrorController(IHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet]
    public ActionResult Error()
    {
        var exceptionHandlerFeature =
            HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        if (exceptionHandlerFeature is null)
        {
            return NotFound();
        }

        if (_environment.IsDevelopment())
        {
            return Problem(detail: exceptionHandlerFeature.Error.StackTrace, title: exceptionHandlerFeature.Error.Message);
        }

        return Problem("Please contact administrator.");
    }
}

