using MerchantCashFlow.Infrastructure.AspNet;
using MerchantCashFlow.Infrastructure.Messaging;
using MerchantCashFlow.Infrastructure.Persistence;
using MerchantCashFlow.Statement.Api.Features.Consumers;
using MerchantCashFlow.Statement.Api.Features.Statement;
using MerchantCashFlow.Statement.Application.Features;
using MerchantCashFlow.Statement.Application.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddCashFlowApiDefaults<DbCashFlowStatementContext>();

builder.Services.AddMerchantCashFlowDbContextPool<DbCashFlowStatementContext>(builder.Configuration, builder.Configuration.GetConnectionString("Statement")!);
builder.Services.AddCashFlowMessaging(
    builder.Configuration,
    builder.Configuration.GetConnectionString("Broker")!,
    bus => bus.AddConsumer<LedgerEntryRegisteredConsumer>());

builder.Services.AddScoped<IApplyLedgerEntry, ApplyLedgerEntry>();
builder.Services.AddScoped<IGetDailyStatement, GetDailyStatement>();

var app = builder.Build();

app.UseCashFlowApiDefaults();
app.GroupStatementEndpoints();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DbCashFlowStatementContext>().Database.MigrateAsync();
}

await app.RunAsync();

namespace MerchantCashFlow.Statement.Api
{
    public sealed class MerchantCashFlowStatementApiProgram;
}
