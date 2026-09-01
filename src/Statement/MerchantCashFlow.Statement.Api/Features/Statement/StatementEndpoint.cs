using MerchantCashFlow.Infrastructure.Diagnostics;
using MerchantCashFlow.Infrastructure.Security;
using MerchantCashFlow.Statement.Application.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;

namespace MerchantCashFlow.Statement.Api.Features.Statement;

public static class StatementEndpoint
{
    public static WebApplication GroupStatementEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/statement")
            .MapStatementEndpoints()
            .WithTags("Statement");

        return app;
    }

    private static RouteGroupBuilder MapStatementEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/",
            async Task<Ok<GetDailyStatement.Output>> (
                DateOnly? date,
                HttpContext httpContext,
                IGetDailyStatement getDailyStatement,
                IDiagnosticContext diagnosticContext,
                CancellationToken cancellationToken) =>
            {
                // Identidade vem do gateway, que apaga o que o cliente mandou antes de escrever o valor do token.
                var documentHash = httpContext.Request.Headers[TokenClaimsHeaders.DocumentHashHeader].ToString();

                if (string.IsNullOrWhiteSpace(documentHash))
                {
                    throw AppException.Unauthorized("Request is missing the merchant identification.");
                }

                var day = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

                diagnosticContext.Set("StatementDate", day);

                var output = await getDailyStatement.ExecuteAsync(
                    new GetDailyStatement.Input(documentHash, day),
                    cancellationToken);

                return TypedResults.Ok(output);
            });

        return group;
    }
}
