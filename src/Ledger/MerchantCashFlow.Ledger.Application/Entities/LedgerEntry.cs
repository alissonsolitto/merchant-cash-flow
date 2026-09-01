namespace MerchantCashFlow.Ledger.Application.Entities;

public class LedgerEntry
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public Guid LedgerId { get; set; }
    public string DocumentHash { get; set; } = string.Empty;
    public string AccountNumberHash { get; set; } = string.Empty;
    public EntryType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset InsertedAt { get; set; }
}
