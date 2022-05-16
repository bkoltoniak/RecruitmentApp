using Microsoft.AspNetCore.Mvc.Testing;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;
using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace RecruitmentApp.StressTests;

public class WebApiStressTest : IDisposable
{
    private readonly int _maxDayParameter = 31;
    private readonly WebApplicationFactory<WebApi.Program> _webApplicationFactory;

    public Scenario WebApiStressTestScenario { get; set; }

    public string[] CurrencyCodes { get; set; } = new string[]
    {
        "EUR",
        "USD",
        "PLN",
        "CHF",
        "SEK",
        "CAD",
        "JPY",
        "CZK",
    };

    public WebApiStressTest()
    {
        // TODO Add separate database for stress tests.

        var requestBodyDataFeed = Feed.CreateRandom("bodies", GetRequestBodies(500));

        _webApplicationFactory = new WebApplicationFactory<WebApi.Program>();

        var httpClientFactory = ClientFactory.Create(
            name: "webapi_client",
            initClient: (number, context) => Task.FromResult(_webApplicationFactory.CreateClient()),
            clientCount: 1);

        var step = Step.Create(
            name: "step",
            clientFactory: httpClientFactory,
            feed: requestBodyDataFeed,
            execute: context =>
            {
                var requset = Http.CreateRequest("POST", "https://localhost:5001/api/exchangerates/getrates")
                .WithBody(context.FeedItem);

                return Http.Send(requset, context);
            });

        WebApiStressTestScenario = ScenarioBuilder
            .CreateScenario("webapi_stresstest_scenario", step)
            .WithWarmUpDuration(duration: TimeSpan.FromSeconds(5))
            .WithLoadSimulations(Simulation
                .InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30)));
    }

    ~WebApiStressTest()
    {
        Dispose();
    }

    public void Dispose()
    {
        _webApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }


    private IEnumerable<StringContent> GetRequestBodies(int count)
    {
        foreach (var i in Enumerable.Range(0, count))
        {
            yield return GetRequestBody();
        }
    }

    private StringContent GetRequestBody()
    {
        var random = new Random();
        var refDate = new DateTime(2022, 01, 01);
        var startDate = refDate.AddDays(random.Next(1, _maxDayParameter));
        var endDate = startDate.AddDays(random.Next(1, _maxDayParameter));
        var baseCurrencyIndex = random.Next(0, CurrencyCodes.Length);
        var quotedCurrencyIndex = random.Next(0, CurrencyCodes.Length);


        var json = string.Format("{{ \"apiKey\": \"d6RZLN+W6TNyuLjULMhidzvFdPvIc6GPf3H92SUneGUhp4Fi\", \"startDate\": \"{0}\", \"endDate\": \"{1}\", \"currencyCodes\": {{ \"{2}\": \"{3}\" }}}}",
            startDate.ToString("yyyy-MM-dd"),
            endDate.ToString("yyyy-MM-dd"),
            CurrencyCodes[baseCurrencyIndex],
            CurrencyCodes[quotedCurrencyIndex]);

        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    [Fact]
    public void EcbExchangeRateService_100RequestPerSecondFor30Seconds_LatencyAndResponsesPerMinutesAreGood()
    {
        var nodeStats = NBomberRunner.RegisterScenarios(WebApiStressTestScenario).Run();
        var stepStats = nodeStats.ScenarioStats[0].StepStats[0];

        Assert.True(stepStats.Ok.Request.RPS > 10);
        Assert.True(stepStats.Ok.Latency.Percent75 < 500);
    }
}
