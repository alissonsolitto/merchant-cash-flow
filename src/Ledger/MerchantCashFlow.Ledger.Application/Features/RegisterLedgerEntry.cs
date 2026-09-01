using System.Text.Json;
using MerchantCashFlow.Infrastructure.Contracts;
using MerchantCashFlow.Infrastructure.Diagnostics;
using MerchantCashFlow.Infrastructure.Persistence;
using MerchantCashFlow.Infrastructure.UseCase;
using MerchantCashFlow.Ledger.Application.Entities;
using MerchantCashFlow.Ledger.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MerchantCashFlow.Ledger.Application.Features;

public interface IRegisterLedgerEntry: IUseCase<RegisterLedgerEntry.Input, RegisterLedgerEntry.Output> { }

public sealed class RegisterLedgerEntry: IRegisterLedgerEntry
{
    public sealed record Input(
        string DocumentHash,
        string AccountNumberHash,
        EntryType Type,
        decimal Amount,
        string IdempotencyKey);

    public sealed record Output(Guid LedgerId, bool AlreadyRegistered);

    private readonly DbCashFlowLedgerContext _context;
    private readonly ILogger<RegisterLedgerEntry> _logger;

    public RegisterLedgerEntry(
        DbCashFlowLedgerContext context,
        ILogger<RegisterLedgerEntry> logger)
    {
        this._context = context;
        this._logger = logger;
    }

    public async Task<Output> ExecuteAsync(Input input, CancellationToken cancellationToken = default)
    {
        var existing = await this.FindByIdempotencyKeyAsync(input, cancellationToken);

        if (existing is not null)
        {
            return new Output(existing.LedgerId, AlreadyRegistered: true);
        }

        var entry = new LedgerEntry
        {
            LedgerId = Guid.CreateVersion7(),
            DocumentHash = input.DocumentHash,
            AccountNumberHash = input.AccountNumberHash,
            Type = input.Type,
            Amount = input.Amount,
            InsertedAt = DateTimeOffset.UtcNow,
            IdempotencyKey = input.IdempotencyKey,
        };

        var payload = new LedgerEntryRegistered
        {
            LedgerId = entry.LedgerId,
            DocumentHash = entry.DocumentHash,
            AccountNumberHash = entry.AccountNumberHash,
            Type = entry.Type.ToString(),
            Amount = entry.Amount,
            InsertedAt = DateTimeOffset.UtcNow
        };

        var outboxMessage = new OutboxMessage
        {
            OutboxId = Guid.CreateVersion7(),
            LedgerId = entry.LedgerId,
            Payload = JsonSerializer.Serialize(payload),
            OccurredAt = entry.InsertedAt,
            Attempts = 0,
        };

        try
        {
            await this._context.ExecuteTransactionWithRetryAsync(async () =>
            {
                this._context.Ledger.Add(entry);
                this._context.Outbox.Add(outboxMessage);

                await this._context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicatedIdempotencyKey(ex))
        {
            this._context.ChangeTracker.Clear();
            var winner = await this.FindByIdempotencyKeyAsync(input, cancellationToken);
            if (winner != null)
            {
                this._logger.LogWarning("Duplicate idempotency key detected. Returning existing ledger entry with LedgerId: {LedgerId}", winner.LedgerId);
                return new Output(winner.LedgerId, AlreadyRegistered: true);
            }
            else
            {
                this._logger.LogError("Duplicate idempotency key detected, but no existing ledger entry found. This should not happen. IdempotencyKey: {IdempotencyKey}", input.IdempotencyKey);
                throw new AppException("Duplicated idempotency key detected, but no existing ledger entry found.");
            }
        }

        return new Output(entry.LedgerId, AlreadyRegistered: false);
    }

    private Task<LedgerEntry?> FindByIdempotencyKeyAsync(Input input, CancellationToken cancellationToken)
    {
        return this._context.Ledger
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.DocumentHash == input.DocumentHash && e.IdempotencyKey == input.IdempotencyKey, cancellationToken);
    }

    private static bool IsDuplicatedIdempotencyKey(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: "23505",
            ConstraintName: "ux_ledger_document_idempotency_key",
        };
    }
}
