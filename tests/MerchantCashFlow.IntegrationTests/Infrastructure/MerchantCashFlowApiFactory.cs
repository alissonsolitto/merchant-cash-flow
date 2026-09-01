using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MerchantCashFlow.IntegrationTests.Infrastructure;

public sealed class MerchantCashFlowApiFactory<TMarker>: WebApplicationFactory<TMarker> where TMarker : class
{
    private readonly Dictionary<string, string?> _settings;

    public MerchantCashFlowApiFactory(Dictionary<string, string?> settings) => this._settings = settings;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(this._settings));
    }
}

public static class ApiFactories
{
    public static MerchantCashFlowApiFactory<MerchantCashFlow.Auth.Api.MerchantCashFlowAuthApiProgram> Auth(MerchantCashFlowEnvironment environment) =>
        new(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Auth"] = environment.AuthConnectionString,
            ["Jwt:SigningKey"] = MerchantCashFlowEnvironment.SigningKey,
            ["DataProtection:KeyRingPath"] = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
        });

    public static MerchantCashFlowApiFactory<MerchantCashFlow.Ledger.Api.MerchantCashFlowLedgerApiProgram> Ledger(MerchantCashFlowEnvironment environment) =>
        new(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Ledger"] = environment.LedgerConnectionString,
            ["ConnectionStrings:Broker"] = environment.BrokerConnectionString,
            ["Outbox:PollIntervalMs"] = "200",
        });

    public static MerchantCashFlowApiFactory<MerchantCashFlow.Statement.Api.MerchantCashFlowStatementApiProgram> Statement(MerchantCashFlowEnvironment environment) =>
        new(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Statement"] = environment.StatementConnectionString,
            ["ConnectionStrings:Broker"] = environment.BrokerConnectionString,
        });
}
