using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RecruitmentApp.Core.Interfaces;
using RecruitmentApp.WebApi.Filters;

namespace RecruitmentApp.WebApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class RequireApiKeyAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new RequireApiKeyFilter(
            (IApiKeyService)serviceProvider.GetRequiredService(typeof(IApiKeyService)),
            (ILogger<RequireApiKeyFilter>)serviceProvider.GetRequiredService(typeof(ILogger<RequireApiKeyFilter>)));
    }
}
