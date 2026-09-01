using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MerchantCashFlow.PerformanceTests;

internal static class PerformanceEnvironment
{
    public static Uri BaseUrl { get; } = new(
        Environment.GetEnvironmentVariable("PERFORMANCE_BASE_URL") ?? "http://localhost:8080");

    // escopo full => MerchantSeeder.
    private const string Document = "11111111000191";
    private const string AccountNumber = "0001-1";

    public static async Task<string> GetTokenAsync()
    {
        using var client = new HttpClient { BaseAddress = BaseUrl };

        var response = await client.PostAsJsonAsync("/api/auth", new { document = Document, accountNumber = AccountNumber });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<string>())!;
    }

    public static HttpClient CreateAuthenticatedClient(string token)
    {
        var client = new HttpClient { BaseAddress = BaseUrl };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
