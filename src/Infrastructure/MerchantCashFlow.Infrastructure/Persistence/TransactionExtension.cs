using Microsoft.EntityFrameworkCore;

namespace MerchantCashFlow.Infrastructure.Persistence;

public static class TransactionExtension
{
    public static async Task ExecuteTransactionWithRetryAsync(this DbContext dbContext, Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await operation();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public static async Task<TResult> ExecuteTransactionWithRetryAsync<TResult>(this DbContext dbContext, Func<Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
