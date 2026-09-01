using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace MerchantCashFlow.PerformanceTests;

[Trait("Category", "Performance")]
public class LedgerLoadTests
{
    private const double MaxErrorRatePercent = 5;
    private const int MaxP95Ms = 300;

    [Fact]
    public async Task Ledger_performance()
    {
        var token = await PerformanceEnvironment.GetTokenAsync();
        using var client = PerformanceEnvironment.CreateAuthenticatedClient(token);

        var scenario = Scenario.Create("register_ledger_entry", async context =>
        {
            var body = JsonSerializer.Serialize(new { type = "Credit", amount = 10.50m });

            var request = Http.CreateRequest("POST", "/api/ledger")
                .WithHeader("Idempotency-Key", Guid.NewGuid().ToString())
                .WithBody(new StringContent(body, Encoding.UTF8, "application/json"));

            var response = await Http.Send(client, request);
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(20)));

        var stats = NBomberRunner.RegisterScenarios(scenario)
            .WithReportFolder("reports")
            .WithReportFileName("ledger-load")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv, ReportFormat.Md)
            .Run();

        var scenarioStats = stats.ScenarioStats.Get("register_ledger_entry");

        Assert.True(scenarioStats.Fail.Request.Percent <= MaxErrorRatePercent,
            $"error rate {scenarioStats.Fail.Request.Percent}% excedeu o limite de {MaxErrorRatePercent}%");
        Assert.True(scenarioStats.Ok.Latency.Percent95 <= MaxP95Ms,
            $"p95 {scenarioStats.Ok.Latency.Percent95}ms excedeu o limite de {MaxP95Ms}ms");
    }
}
