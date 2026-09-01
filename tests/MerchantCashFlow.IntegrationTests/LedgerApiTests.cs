using System.Net;
using System.Net.Http.Json;
using MerchantCashFlow.IntegrationTests.Infrastructure;

namespace MerchantCashFlow.IntegrationTests;

[Collection(MerchantCashFlowCollection.Name)]
public sealed class LedgerApiTests: IAsyncLifetime
{
    private sealed record EntryResponse(Guid LedgerId);

    private readonly MerchantCashFlowEnvironment _environment;
    private MerchantCashFlowApiFactory<MerchantCashFlow.Auth.Api.MerchantCashFlowAuthApiProgram> _auth = null!;
    private MerchantCashFlowApiFactory<MerchantCashFlow.Ledger.Api.MerchantCashFlowLedgerApiProgram> _ledger = null!;
    private HttpClient _client = null!;

    public LedgerApiTests(MerchantCashFlowEnvironment environment) => this._environment = environment;

    public async Task InitializeAsync()
    {
        this._auth = ApiFactories.Auth(this._environment);
        this._ledger = ApiFactories.Ledger(this._environment);

        var token = await TestClient.GetTokenAsync(this._auth.CreateClient(), MerchantCashFlowEnvironment.FullDocument, MerchantCashFlowEnvironment.FullAccount);
        var (documentHash, accountNumberHash) = TestClient.ReadIdentity(token);

        this._client = this._ledger.CreateClient().WithIdentity(documentHash, accountNumberHash);
    }

    public async Task DisposeAsync()
    {
        this._client.Dispose();
        await this._ledger.DisposeAsync();
        await this._auth.DisposeAsync();
    }

    [Fact]
    public async Task Registering_new_ledger()
    {
        var response = await this.PostEntryAsync("Credit", 150.50m, Guid.NewGuid().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Replaying_idempotency_key()
    {
        var key = Guid.NewGuid().ToString();

        var first = await this.PostEntryAsync("Credit", 10m, key);
        var second = await this.PostEntryAsync("Credit", 10m, key);

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstBody = await first.Content.ReadFromJsonAsync<Guid>();
        var secondBody = await second.Content.ReadFromJsonAsync<Guid>();

        secondBody.Should().Be(firstBody);
    }

    [Fact]
    public async Task Concurrent_requests_same_idempotency_key()
    {
        var key = Guid.NewGuid().ToString();

        var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => this.PostEntryAsync("Debit", 25m, key)));

        responses.Should().OnlyContain(response => response.IsSuccessStatusCode);
        responses.Count(response => response.StatusCode == HttpStatusCode.Created).Should().Be(1);
    }

    [Theory]
    [InlineData("Credit", 0)]
    [InlineData("Credit", -10)]
    [InlineData("Transfer", 10)]
    public async Task Invalid_payloads(string type, decimal amount)
    {
        var response = await this.PostEntryAsync(type, amount, Guid.NewGuid().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Without_idempotency_key()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/ledger")
        {
            Content = JsonContent.Create(new { type = "Credit", amount = 10m }),
        };

        var response = await this._client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Without_auth_header()
    {
        using var anonymousClient = this._ledger.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/ledger")
        {
            Content = JsonContent.Create(new { type = "Credit", amount = 10m }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await anonymousClient.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<HttpResponseMessage> PostEntryAsync(string type, decimal amount, string idempotencyKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/ledger")
        {
            Content = JsonContent.Create(new { type, amount }),
        };
        request.Headers.Add("Idempotency-Key", idempotencyKey);

        return await this._client.SendAsync(request);
    }
}
