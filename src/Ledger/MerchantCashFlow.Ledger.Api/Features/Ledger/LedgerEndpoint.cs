using MerchantCashFlow.Infrastructure.AspNet;
using MerchantCashFlow.Infrastructure.Diagnostics;
using MerchantCashFlow.Infrastructure.Security;
using MerchantCashFlow.Ledger.Application.Entities;
using MerchantCashFlow.Ledger.Application.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MerchantCashFlow.Ledger.Api.Features.Entries;

public static class LedgerEndpoint
{
    public static WebApplication GroupEntryEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/ledger")
            .MapEntryEndpoints()
            .WithTags("Ledger");

        return app;
    }

    private static RouteGroupBuilder MapEntryEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/",
            async Task<Results<Ok<Guid>, Created<Guid>, ValidationProblem>> (
                LedgerRequest request,
                [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
                HttpContext httpContext,
                IRegisterLedgerEntry registerLedgerEntry,
                IDiagnosticContext diagnosticContext,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    throw new AppException("Idempotency-Key", "Header is required.");
                }

                diagnosticContext.Set("IdempotencyKey", idempotencyKey);

                // Identidade vem do gateway, que apaga o que o cliente mandou antes de escrever o valor do token.
                var documentHash = httpContext.Request.Headers[TokenClaimsHeaders.DocumentHashHeader].ToString();
                var accountNumberHash = httpContext.Request.Headers[TokenClaimsHeaders.AccountNumberHashHeader].ToString();

                if (string.IsNullOrWhiteSpace(documentHash) || string.IsNullOrWhiteSpace(accountNumberHash))
                {
                    throw AppException.Unauthorized("Request is missing the merchant identification.");
                }

                var input = new RegisterLedgerEntry.Input(
                    documentHash,
                    accountNumberHash,
                    Enum.Parse<EntryType>(request.Type, ignoreCase: true),
                    request.Amount,
                    idempotencyKey);

                var output = await registerLedgerEntry.ExecuteAsync(input, cancellationToken);
                var response = output.LedgerId;

                diagnosticContext.Set("LedgerId", output.LedgerId);

                return output.AlreadyRegistered
                    ? TypedResults.Ok(response)
                    : TypedResults.Created($"/api/ledger/{output.LedgerId}", response);
            })
        .AddEndpointFilter<ValidationFilter<LedgerRequest>>();

        return group;
    }
}
