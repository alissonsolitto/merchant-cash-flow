using MassTransit;
using MerchantCashFlow.Infrastructure.Contracts;
using MerchantCashFlow.Statement.Application.Features;

namespace MerchantCashFlow.Statement.Api.Features.Consumers;

public sealed class LedgerEntryRegisteredConsumer: IConsumer<LedgerEntryRegistered>
{
    private readonly IApplyLedgerEntry _applyLedgerEntry;

    public LedgerEntryRegisteredConsumer(IApplyLedgerEntry applyLedgerEntry) => this._applyLedgerEntry = applyLedgerEntry;

    public Task Consume(ConsumeContext<LedgerEntryRegistered> context)
    {
        var input = new ApplyLedgerEntry.Input(
            context.Message.LedgerId,
            context.Message.DocumentHash,
            context.Message.AccountNumberHash,
            context.Message.Type,
            context.Message.Amount,
            context.Message.InsertedAt);

        return this._applyLedgerEntry.ExecuteAsync(input, context.CancellationToken);
    }

}
