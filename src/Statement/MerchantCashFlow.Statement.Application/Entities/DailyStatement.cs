namespace MerchantCashFlow.Statement.Application.Entities;

public class DailyStatement
{
    public string DocumentHash { get; set; } = string.Empty;
    public DateOnly StatementDate { get; set; }
    public decimal Credit { get; set; }
    public decimal Debit { get; set; }
    public decimal Balance { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
