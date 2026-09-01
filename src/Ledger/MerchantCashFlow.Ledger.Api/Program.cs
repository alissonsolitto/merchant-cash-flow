using FluentValidation;
using MerchantCashFlow.Infrastructure.AspNet;
using MerchantCashFlow.Infrastructure.Messaging;
using MerchantCashFlow.Infrastructure.Persistence;
using MerchantCashFlow.Ledger.Api;
using MerchantCashFlow.Ledger.Api.Features.Entries;
using MerchantCashFlow.Ledger.Api.Features.Outbox;
using MerchantCashFlow.Ledger.Application.Features;
using MerchantCashFlow.Ledger.Application.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddCashFlowApiDefaults<DbCashFlowLedgerContext>();

builder.Services.AddValidatorsFromAssemblyContaining<MerchantCashFlowLedgerApiProgram>();
builder.Services.AddMerchantCashFlowDbContextPool<DbCashFlowLedgerContext>(builder.Configuration, builder.Configuration.GetConnectionString("Ledger")!);
builder.Services.AddCashFlowMessaging(builder.Configuration, builder.Configuration.GetConnectionString("Broker")!);

builder.Services.AddOptions<OutboxOptions>().Bind(builder.Configuration.GetSection(OutboxOptions.SectionName));
builder.Services.AddHostedService<OutboxPublisherWorker>();

builder.Services.AddScoped<IRegisterLedgerEntry, RegisterLedgerEntry>();
builder.Services.AddScoped<IPublishLedgerOutbox, PublishLedgerOutbox>();

var app = builder.Build();

app.UseCashFlowApiDefaults();
app.GroupEntryEndpoints();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DbCashFlowLedgerContext>().Database.MigrateAsync();
}

await app.RunAsync();

namespace MerchantCashFlow.Ledger.Api
{
    public sealed class MerchantCashFlowLedgerApiProgram;
}
