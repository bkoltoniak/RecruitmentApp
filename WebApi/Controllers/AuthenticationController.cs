using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecruitmentApp.Core.Interfaces;

namespace RecruitmentApp.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public AuthenticationController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpGet("GetKey")]
    public ActionResult GetKey()
    {
        return Ok(new { key = _apiKeyService.GetKey() });
    }
}
