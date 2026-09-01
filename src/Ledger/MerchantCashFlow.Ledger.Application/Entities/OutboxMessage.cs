namespace MerchantCashFlow.Ledger.Application.Entities;

public class OutboxMessage
{
    public Guid OutboxId { get; set; }
    public Guid LedgerId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
