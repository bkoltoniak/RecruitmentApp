using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RecruitmentApp.Core.Interfaces;

namespace RecruitmentApp.WebApi.Filters;

public class RequireApiKeyFilter : IAsyncAuthorizationFilter
{
    private readonly IApiKeyService _apiKeyService;
    private ILogger<RequireApiKeyFilter> _logger;

    public RequireApiKeyFilter(IApiKeyService apiKeyService, ILogger<RequireApiKeyFilter> logger)
    {
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        context.HttpContext.Request.EnableBuffering();
        var apiKey = await context.HttpContext.Request.ReadFromJsonAsync<ApiKeyModel>();
        context.HttpContext.Request.Body.Position = 0;
         
        if (apiKey is null)
        {
            _logger.LogWarning("Authorization invoked but key is missing.");
            context.Result = context.Result = new UnauthorizedResult();
        }
        else if(!_apiKeyService.ValidateKey(apiKey.ApiKey!))
        {
            _logger.LogWarning("Authorization has failed for key {0}.", apiKey.ApiKey);
            context.Result = context.Result = new UnauthorizedResult();
        }
    }

    class ApiKeyModel
    {
        public string? ApiKey { get; set; }
    }
}
