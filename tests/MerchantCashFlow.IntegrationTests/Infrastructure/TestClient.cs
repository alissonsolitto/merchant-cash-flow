using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MerchantCashFlow.Infrastructure.Security;

namespace MerchantCashFlow.IntegrationTests.Infrastructure;

internal static class TestClient
{
    public static async Task<string> GetTokenAsync(HttpClient authClient, string document, string accountNumber)
    {
        var response = await authClient.PostAsJsonAsync("/api/auth", new { document, accountNumber });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<string>())!;
    }

    public static (string DocumentHash, string AccountNumberHash) ReadIdentity(string accessToken)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        return (
            token.Claims.First(claim => claim.Type == TokenClaims.DocumentHash).Value,
            token.Claims.First(claim => claim.Type == TokenClaims.AccountNumberHash).Value);
    }

    public static HttpClient WithIdentity(this HttpClient client, string documentHash, string accountNumberHash)
    {
        client.DefaultRequestHeaders.Remove(TokenClaimsHeaders.DocumentHashHeader);
        client.DefaultRequestHeaders.Remove(TokenClaimsHeaders.AccountNumberHashHeader);
        client.DefaultRequestHeaders.Add(TokenClaimsHeaders.DocumentHashHeader, documentHash);
        client.DefaultRequestHeaders.Add(TokenClaimsHeaders.AccountNumberHashHeader, accountNumberHash);
        return client;
    }

    public static HttpClient WithBearer(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task UntilAsync(Func<Task<bool>> condition, TimeSpan timeout, string because)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(200);
        }

        throw new TimeoutException($"Condicao nao atingida em {timeout.TotalSeconds:N0}s: {because}");
    }
}
