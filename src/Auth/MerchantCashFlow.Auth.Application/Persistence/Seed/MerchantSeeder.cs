using MerchantCashFlow.Auth.Application.Entities;
using MerchantCashFlow.Infrastructure.DataProtection;
using MerchantCashFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MerchantCashFlow.Auth.Application.Persistence.Seed;

public sealed class MerchantSeeder
{
    private static readonly (string Document, string AccountNumber, string Scope)[] _merchants =
    [
        ("11111111000191", "0001-1", AccessScopes.Full),
        ("22222222000172", "0002-2", AccessScopes.Ledger),
        ("33333333000153", "0003-3", AccessScopes.Statement),
    ];

    private readonly DbCashFlowAuthContext _context;
    private readonly ILogger<MerchantSeeder> _logger;

    public MerchantSeeder(DbCashFlowAuthContext context, ILogger<MerchantSeeder> logger)
    {
        this._context = context;
        this._logger = logger;
    }

    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var created = 0;
        var now = DateTimeOffset.UtcNow;

        foreach (var (document, accountNumber, scope) in _merchants)
        {
            var documentHash = PiiHash.Of(document);
            var accountNumberHash = PiiHash.Of(accountNumber);

            // Os dois hashes têm índice único, então checar só o documento deixaria o insert estourar.
            var exists = await this._context.Merchant.AnyAsync(
                m => m.Document.Hash == documentHash || m.AccountNumber.Hash == accountNumberHash,
                cancellationToken);

            if (exists)
            {
                continue;
            }

            this._context.Merchant.Add(new Merchant
            {
                MerchantId = Guid.CreateVersion7(),
                Document = ProtectedValue.Of(document),
                AccountNumber = ProtectedValue.Of(accountNumber),
                Scope = scope,
                CreatedAt = now,
            });

            created++;
        }

        if (created > 0)
        {
            await this._context.SaveChangesAsync(cancellationToken);
            this._logger.LogInformation("Merchants created during seed: {Count}", created);
        }

        return created;
    }
}
