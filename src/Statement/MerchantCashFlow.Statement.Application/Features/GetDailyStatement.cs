using MerchantCashFlow.Infrastructure.UseCase;
using MerchantCashFlow.Statement.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MerchantCashFlow.Statement.Application.Features;

public interface IGetDailyStatement: IUseCase<GetDailyStatement.Input, GetDailyStatement.Output> { }

public sealed class GetDailyStatement: IGetDailyStatement
{
    public sealed record Input(string DocumentHash, DateOnly Date);
    public sealed record Output(DateOnly Date, decimal Credit, decimal Debit, decimal Balance);

    private readonly DbCashFlowStatementContext _context;

    public GetDailyStatement(DbCashFlowStatementContext context) => this._context = context;

    public async Task<Output> ExecuteAsync(Input input, CancellationToken cancellationToken = default)
    {
        var daily = await this._context.Daily
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DocumentHash == input.DocumentHash && d.StatementDate == input.Date, cancellationToken);

        return daily is null
            ? new Output(input.Date, 0m, 0m, 0m)
            : new Output(input.Date, daily.Credit, daily.Debit, daily.Balance);
    }
}
