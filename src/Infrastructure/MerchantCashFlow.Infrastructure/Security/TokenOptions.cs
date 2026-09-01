namespace MerchantCashFlow.Infrastructure.Security;

public sealed class TokenOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 30;
    public int ClockSkewSeconds { get; set; } = 30;
}

public static class TokenClaims
{
    public const string Scope = "scope";
    public const string DocumentHash = "document_hash";
    public const string AccountNumberHash = "account_number_hash";
}

public static class TokenClaimsHeaders
{
    public const string DocumentHashHeader = "X-Document-Hash";
    public const string AccountNumberHashHeader = "X-Account-Number-Hash";
}

public static class AccessScopes
{
    public const string Full = "full";
    public const string Ledger = "ledger";
    public const string Statement = "statement";
}
