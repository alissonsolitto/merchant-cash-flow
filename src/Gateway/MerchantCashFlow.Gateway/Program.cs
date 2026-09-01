using System.Threading.RateLimiting;
using MerchantCashFlow.Gateway;
using MerchantCashFlow.Gateway.Security;
using MerchantCashFlow.Infrastructure.AspNet;
using MerchantCashFlow.Infrastructure.Security;
using Microsoft.AspNetCore.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddCashFlowApiDefaults();
builder.Services.AddGatewayAuthentication(builder.Configuration);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context => context.AddRequestTransform(transform =>
    {
        var headers = transform.ProxyRequest.Headers;

        headers.Remove(TokenClaimsHeaders.DocumentHashHeader);
        headers.Remove(TokenClaimsHeaders.AccountNumberHashHeader);

        var user = transform.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return ValueTask.CompletedTask;
        }

        headers.TryAddWithoutValidation(TokenClaimsHeaders.DocumentHashHeader, user.FindFirst(TokenClaims.DocumentHash)?.Value);
        headers.TryAddWithoutValidation(TokenClaimsHeaders.AccountNumberHashHeader, user.FindFirst(TokenClaims.AccountNumberHash)?.Value);

        return ValueTask.CompletedTask;
    }));

var rateLimiting = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();

builder.Services.AddRateLimiter(options =>
{
    options.AddConcurrencyLimiter("api", limiter =>
    {
        limiter.PermitLimit = rateLimiting.Api.PermitLimit;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = rateLimiting.Api.QueueLimit;
    });

    // Apenas para demonstrar a limitação de requisições para o endpoint de autenticação, que é público.
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = rateLimiting.Auth.PermitLimit;
        limiter.Window = TimeSpan.FromMinutes(rateLimiting.Auth.WindowMinutes);
    });
});

var app = builder.Build();

app.UseCashFlowApiDefaults();

app.UseHsts();
app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

await app.RunAsync();
