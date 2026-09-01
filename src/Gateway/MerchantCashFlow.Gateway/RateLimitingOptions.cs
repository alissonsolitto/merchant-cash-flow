namespace MerchantCashFlow.Gateway;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public ApiLimiterOptions Api { get; set; } = new();
    public AuthLimiterOptions Auth { get; set; } = new();

    public sealed class ApiLimiterOptions
    {
        public int PermitLimit { get; set; } = 200;
        public int QueueLimit { get; set; } = 100;
    }

    public sealed class AuthLimiterOptions
    {
        public int PermitLimit { get; set; } = 2;
        public int WindowMinutes { get; set; } = 1;
    }
}
