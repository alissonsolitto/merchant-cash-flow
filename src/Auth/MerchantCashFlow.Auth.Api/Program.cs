using FluentValidation;
using MerchantCashFlow.Auth.Api;
using MerchantCashFlow.Auth.Api.Features.Tokens;
using MerchantCashFlow.Auth.Application.Features;
using MerchantCashFlow.Auth.Application.Persistence;
using MerchantCashFlow.Auth.Application.Persistence.Seed;
using MerchantCashFlow.Infrastructure.AspNet;
using MerchantCashFlow.Infrastructure.DataProtection;
using MerchantCashFlow.Infrastructure.Persistence;
using MerchantCashFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddCashFlowApiDefaults<DbCashFlowAuthContext>();
builder.Services.AddOptions<TokenOptions>().Bind(builder.Configuration.GetSection(TokenOptions.SectionName));
builder.Services.AddValidatorsFromAssemblyContaining<MerchantCashFlowAuthApiProgram>();

builder.Services.AddCashFlowDataProtection(builder.Configuration["DataProtection:KeyRingPath"]!);
builder.Services.AddMerchantCashFlowDbContextPool<DbCashFlowAuthContext>(builder.Configuration, builder.Configuration.GetConnectionString("Auth")!);

builder.Services.AddScoped<MerchantSeeder>();
builder.Services.AddScoped<IGenerateMerchantToken, GenerateMerchantToken>();

var app = builder.Build();

app.UseCashFlowApiDefaults();
app.GroupTokenEndpoints();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<DbCashFlowAuthContext>().Database.MigrateAsync();
await scope.ServiceProvider.GetRequiredService<MerchantSeeder>().SeedAsync();

await app.RunAsync();

namespace MerchantCashFlow.Auth.Api
{
    public sealed class MerchantCashFlowAuthApiProgram;
}
