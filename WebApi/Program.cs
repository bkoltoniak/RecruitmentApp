using Microsoft.Extensions.Logging.Console;
using RecruitmentApp.Core.Interfaces;
using RecruitmentApp.Core.Models;
using RecruitmentApp.Core.Options;
using RecruitmentApp.Core.Services;
using RecruitmentApp.Data;
using RecruitmentApp.Data.Repositories;
using RecruitmentApp.Data.Services;
using RecruitmentApp.WebApi.Services;

namespace RecruitmentApp.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddFile(builder.Configuration.GetSection("Logging"));

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddSimpleConsole(config =>
            {
                config.SingleLine = true;
                config.TimestampFormat = "dd-MM-yyyy hh:mm:ss.fff ";
                config.ColorBehavior = LoggerColorBehavior.Enabled;
            });
        }


        builder.Services.AddControllers();
        builder.Services.Configure<ApiKeyOptions>(
            builder.Configuration.GetRequiredSection(ApiKeyOptions.Section));
        builder.Services.Configure<CacheOptions>(
            builder.Configuration.GetRequiredSection(CacheOptions.Section));
        builder.Services.AddScoped<IApiKeyService, ApiKeySerivce>();
        builder.Services.AddScoped<IExchangeRateApiService, EcbExchangeRateService>();
        builder.Services.AddScoped<IRepository<ExchangeRate>, ExchangeRateRepository>();
        builder.Services.AddSingleton<IChacheService, CacheService>();
        builder.Services.AddHttpClient();
        builder.Services.AddMemoryCache();
        builder.Services.AddDbContext<AppDbContext>();

        var app = builder.Build();

        app.UseExceptionHandler("/api/error");
        app.UseHttpsRedirection();
        app.UseRouting();
        app.MapControllers();

        await app.RunAsync();
    }
}
