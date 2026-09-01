namespace MerchantCashFlow.Infrastructure.UseCase;

public interface IUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

public interface IUseCase<in TInput>
{
    Task ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}

public interface IUseCase<in TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}
