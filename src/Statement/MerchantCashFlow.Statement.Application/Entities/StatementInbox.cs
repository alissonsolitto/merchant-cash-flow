namespace MerchantCashFlow.Statement.Application.Entities;

public class StatementInbox
{
    public Guid LedgerId { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}
