using System.Text.Json;
using MassTransit;
using MerchantCashFlow.Infrastructure.Contracts;
using MerchantCashFlow.Infrastructure.Persistence;
using MerchantCashFlow.Infrastructure.UseCase;
using MerchantCashFlow.Ledger.Application.Entities;
using MerchantCashFlow.Ledger.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MerchantCashFlow.Ledger.Application.Features;

public interface IPublishLedgerOutbox: IUseCase<PublishLedgerOutbox.Input, PublishLedgerOutbox.Output> { }

public sealed class PublishLedgerOutbox: IPublishLedgerOutbox
{
    public sealed record Input(int BatchSize);
    public sealed record Output(int Published, int Failed);

    private readonly DbCashFlowLedgerContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PublishLedgerOutbox> _logger;

    public PublishLedgerOutbox(
        DbCashFlowLedgerContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<PublishLedgerOutbox> logger)
    {
        this._context = context;
        this._publishEndpoint = publishEndpoint;
        this._logger = logger;
    }

    public Task<Output> ExecuteAsync(Input input, CancellationToken cancellationToken = default)
    {
        return this._context.ExecuteTransactionWithRetryAsync(async () =>
        {
            var pending = await this.LedgerPendingAsync(input.BatchSize, cancellationToken);

            if (pending.Count == 0)
            {
                return new Output(0, 0);
            }

            var published = 0;
            var failed = 0;

            foreach (var message in pending)
            {
                try
                {
                    await this.PublishAsync(message, cancellationToken);
                    message.PublishedAt = DateTimeOffset.UtcNow;
                    published++;
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Error publishing outbox message {OutboxId}. Attempts: {Attempts}", message.OutboxId, message.Attempts);

                    message.Attempts++;
                    message.LastError = ex.Message;
                    failed++;

                    break;
                }
            }

            await this._context.SaveChangesAsync(cancellationToken);

            return new Output(published, failed);

        }, cancellationToken);
    }


    private Task<List<OutboxMessage>> LedgerPendingAsync(int batchSize, CancellationToken cancellationToken) =>
        this._context.Outbox
            .FromSql($"""
                SELECT * FROM outbox
                WHERE published_at IS NULL
                ORDER BY occurred_at
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .AsTracking()
            .ToListAsync(cancellationToken);

    private Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<LedgerEntryRegistered>(message.Payload)
            ?? throw new InvalidOperationException($"Failed to deserialize outbox message {message.OutboxId} payload.");

        return this._publishEndpoint.Publish(@event, context =>
        {
            context.MessageId = message.OutboxId;
            context.CorrelationId = message.LedgerId;
        }, cancellationToken);
    }
}
