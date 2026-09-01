using MerchantCashFlow.Infrastructure.DataProtection;

namespace MerchantCashFlow.Auth.Application.Entities;

public class Merchant
{
    public Guid MerchantId { get; set; }
    public ProtectedValue Document { get; set; } = null!;
    public ProtectedValue AccountNumber { get; set; } = null!;
    public string Scope { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
