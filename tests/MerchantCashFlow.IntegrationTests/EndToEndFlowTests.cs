using System.Net.Http.Json;
using MerchantCashFlow.IntegrationTests.Infrastructure;

namespace MerchantCashFlow.IntegrationTests;

[Collection(MerchantCashFlowCollection.Name)]
public sealed class EndToEndFlowTests: IAsyncLifetime
{
    private sealed record StatementView(DateOnly Date, decimal Credit, decimal Debit, decimal Balance);

    private readonly MerchantCashFlowEnvironment _environment;
    private MerchantCashFlowApiFactory<MerchantCashFlow.Auth.Api.MerchantCashFlowAuthApiProgram> _auth = null!;
    private MerchantCashFlowApiFactory<MerchantCashFlow.Ledger.Api.MerchantCashFlowLedgerApiProgram> _ledger = null!;
    private MerchantCashFlowApiFactory<MerchantCashFlow.Statement.Api.MerchantCashFlowStatementApiProgram> _statement = null!;
    private HttpClient _ledgerClient = null!;
    private HttpClient _statementClient = null!;

    public EndToEndFlowTests(MerchantCashFlowEnvironment environment) => this._environment = environment;

    public async Task InitializeAsync()
    {
        this._auth = ApiFactories.Auth(this._environment);
        this._ledger = ApiFactories.Ledger(this._environment);
        this._statement = ApiFactories.Statement(this._environment);

        var token = await TestClient.GetTokenAsync(this._auth.CreateClient(), MerchantCashFlowEnvironment.FullDocument, MerchantCashFlowEnvironment.FullAccount);
        var (documentHash, accountNumberHash) = TestClient.ReadIdentity(token);

        this._ledgerClient = this._ledger.CreateClient().WithIdentity(documentHash, accountNumberHash);
        this._statementClient = this._statement.CreateClient().WithIdentity(documentHash, accountNumberHash);
    }

    public async Task DisposeAsync()
    {
        this._ledgerClient.Dispose();
        this._statementClient.Dispose();
        await this._ledger.DisposeAsync();
        await this._statement.DisposeAsync();
        await this._auth.DisposeAsync();
    }

    [Fact]
    public async Task Consolidated_balance()
    {
        var before = await this.GetStatementAsync();

        await this.PostEntryAsync("Credit", 200m);
        await this.PostEntryAsync("Debit", 75.50m);

        await TestClient.UntilAsync(
            async () => (await this.GetStatementAsync()).Balance == before.Balance + 124.50m,
            TimeSpan.FromSeconds(60),
            "os dois lancamentos deveriam chegar ao consolidado");

        var after = await this.GetStatementAsync();

        (after.Credit - before.Credit).Should().Be(200m);
        (after.Debit - before.Debit).Should().Be(75.50m);
    }

    private async Task PostEntryAsync(string type, decimal amount)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/ledger")
        {
            Content = JsonContent.Create(new { type, amount }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await this._ledgerClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<StatementView> GetStatementAsync() =>
        (await this._statementClient.GetFromJsonAsync<StatementView>("/api/statement"))!;
}
