using MerchantCashFlow.Ledger.Application.Features;
using Microsoft.Extensions.Options;

namespace MerchantCashFlow.Ledger.Api.Features.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";
    public int BatchSize { get; set; } = 100;
    public int PollIntervalMs { get; set; } = 1000;
}

public sealed class OutboxPublisherWorker: BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxOptions> options,
        ILogger<OutboxPublisherWorker> logger)
    {
        this._scopeFactory = scopeFactory;
        this._options = options.Value;
        this._logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var idleDelay = TimeSpan.FromMilliseconds(this._options.PollIntervalMs);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = this._scopeFactory.CreateScope();
                var publishLedgerOutbox = scope.ServiceProvider.GetRequiredService<IPublishLedgerOutbox>();

                var output = await publishLedgerOutbox.ExecuteAsync(
                    new PublishLedgerOutbox.Input(this._options.BatchSize),
                    stoppingToken);

                if (output.Published == 0 || output.Failed > 0)
                {
                    await Task.Delay(idleDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error in outbox publisher worker cycle");
                await Task.Delay(idleDelay, stoppingToken);
            }
        }
    }
}
