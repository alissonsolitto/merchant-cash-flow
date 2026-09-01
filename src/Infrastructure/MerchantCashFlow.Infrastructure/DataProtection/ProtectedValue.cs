using System.Security.Cryptography;
using System.Text;

namespace MerchantCashFlow.Infrastructure.DataProtection;

public sealed class ProtectedValue
{
    // Payload do Data Protection
    public const int ValueLength = 512;
    // SHA-256 em base64.
    public const int HashLength = 44;

    private ProtectedValue()
    {
    }

    private ProtectedValue(PiiValue value, PiiHash hash)
    {
        this.Value = value;
        this.Hash = hash;
    }

    public PiiValue Value { get; private set; }

    public PiiHash Hash { get; private set; }

    public static ProtectedValue Of(string plaintext) => new(plaintext, PiiHash.Of(plaintext));
}


#region struct para tipos de dados especiais
public readonly record struct PiiValue(string Value)
{
    public override string ToString() => this.Value;

    public static implicit operator string(PiiValue value) => value.Value;

    public static implicit operator PiiValue(string value) => new(value);
}

public readonly record struct PiiHash(string Value)
{
    public static PiiHash Of(string plaintext) =>
        new(Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(plaintext.Trim()))));

    public override string ToString() => this.Value;

    public static implicit operator string(PiiHash hash) => hash.Value;
}
#endregion
