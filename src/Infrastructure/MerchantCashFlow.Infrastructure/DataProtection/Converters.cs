using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MerchantCashFlow.Infrastructure.DataProtection;

public sealed class PiiValueConverter: ValueConverter<PiiValue, string>
{
    public const string Purpose = "MerchantCashFlow:Pii:v1";

    public PiiValueConverter(IDataProtectionProvider provider)
        : this(provider.CreateProtector(Purpose))
    { }

    private PiiValueConverter(IDataProtector protector)
        : base(
            v => string.IsNullOrEmpty(v.Value) ? string.Empty : protector.Protect(v.Value),
            v => string.IsNullOrEmpty(v) ? new PiiValue(string.Empty) : new PiiValue(protector.Unprotect(v)))
    { }
}

public sealed class PiiHashConverter: ValueConverter<PiiHash, string>
{
    public PiiHashConverter() : base(v => v.Value, v => new PiiHash(v))
    { }
}
