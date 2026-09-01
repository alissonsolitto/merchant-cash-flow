namespace MerchantCashFlow.Infrastructure.Contracts;

public sealed class LedgerEntryRegistered
{
    public Guid LedgerId { get; set; }
    public string DocumentHash { get; set; } = string.Empty;
    public string AccountNumberHash { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset InsertedAt { get; set; }
}
