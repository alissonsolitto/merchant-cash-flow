using MerchantCashFlow.Infrastructure.Diagnostics;
using MerchantCashFlow.Infrastructure.Persistence;
using MerchantCashFlow.Infrastructure.UseCase;
using MerchantCashFlow.Statement.Application.Entities;
using MerchantCashFlow.Statement.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MerchantCashFlow.Statement.Application.Features;

public interface IApplyLedgerEntry: IUseCase<ApplyLedgerEntry.Input> { }

public sealed class ApplyLedgerEntry: IApplyLedgerEntry
{
    public sealed record Input(Guid LedgerId, string DocumentHash, string Type, decimal Amount, DateTimeOffset InsertedAt);

    private readonly DbCashFlowStatementContext _context;

    public ApplyLedgerEntry(DbCashFlowStatementContext context) => this._context = context;

    public Task ExecuteAsync(Input input, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<EntryType>(input.Type, ignoreCase: true, out var type))
        {
            throw new AppException($"Unknown ledger entry type: {input.Type}.");
        }

        var credit = type == EntryType.Credit ? input.Amount : 0m;
        var debit = type == EntryType.Debit ? input.Amount : 0m;
        var statementDate = DateOnly.FromDateTime(input.InsertedAt.UtcDateTime);

        return this._context.ExecuteTransactionWithRetryAsync(async () =>
        {
            if (await this.TryRegisterAsync(input.LedgerId, cancellationToken))
            {
                await this.AccumulateAsync(input.DocumentHash, statementDate, credit, debit, cancellationToken);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// ON CONFLICT DO NOTHING em vez de capturar a violação de chave: no PostgreSQL uma exceção
    /// aborta a transação inteira, e o saldo ainda precisa ser gravado dentro dela.
    /// </summary>
    private async Task<bool> TryRegisterAsync(Guid ledgerId, CancellationToken cancellationToken)
    {
        var rows = await this._context.Database.ExecuteSqlAsync($"""
            INSERT INTO statement_inbox (ledger_id, processed_at)
            VALUES ({ledgerId}, {DateTimeOffset.UtcNow})
            ON CONFLICT (ledger_id) DO NOTHING
            """, cancellationToken);

        return rows == 1;
    }

    private async Task AccumulateAsync(string documentHash, DateOnly statementDate, decimal credit, decimal debit, CancellationToken cancellationToken) =>
        await this._context.Database.ExecuteSqlAsync($"""
            INSERT INTO statement_daily (document_hash, statement_date, credit, debit, updated_at)
            VALUES ({documentHash}, {statementDate}, {credit}, {debit}, {DateTimeOffset.UtcNow})
            ON CONFLICT (document_hash, statement_date) DO UPDATE SET
                credit = statement_daily.credit + excluded.credit,
                debit = statement_daily.debit + excluded.debit,
                updated_at = excluded.updated_at
            """, cancellationToken);
}
