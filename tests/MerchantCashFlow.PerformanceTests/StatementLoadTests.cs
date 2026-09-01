using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace MerchantCashFlow.PerformanceTests;

[Trait("Category", "Performance")]
public class StatementLoadTests
{
    private const double MaxErrorRatePercent = 5;
    private const int MaxP95Ms = 200;
    private const int MaxP99Ms = 500;

    [Fact]
    public async Task Statement_performance()
    {
        var token = await PerformanceEnvironment.GetTokenAsync();
        using var client = PerformanceEnvironment.CreateAuthenticatedClient(token);

        var scenario = Scenario.Create("get_daily_statement", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/statement");
            var response = await Http.Send(client, request);
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(20)));

        var stats = NBomberRunner.RegisterScenarios(scenario)
            .WithReportFolder("reports")
            .WithReportFileName("statement-load")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv, ReportFormat.Md)
            .Run();

        var scenarioStats = stats.ScenarioStats.Get("get_daily_statement");

        Assert.True(scenarioStats.Fail.Request.Percent <= MaxErrorRatePercent,
            $"error rate {scenarioStats.Fail.Request.Percent}% excedeu o limite de {MaxErrorRatePercent}%");
        Assert.True(scenarioStats.Ok.Latency.Percent95 <= MaxP95Ms,
            $"p95 {scenarioStats.Ok.Latency.Percent95}ms excedeu o limite de {MaxP95Ms}ms");
        Assert.True(scenarioStats.Ok.Latency.Percent99 <= MaxP99Ms,
            $"p99 {scenarioStats.Ok.Latency.Percent99}ms excedeu o limite de {MaxP99Ms}ms");
    }
}
