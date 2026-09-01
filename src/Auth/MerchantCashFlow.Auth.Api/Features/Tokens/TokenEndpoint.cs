using MerchantCashFlow.Auth.Application.Features;
using MerchantCashFlow.Infrastructure.AspNet;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MerchantCashFlow.Auth.Api.Features.Tokens;

public static class TokenEndpoint
{
    public static WebApplication GroupTokenEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/auth")
            .MapTokenEndpoints()
            .AllowAnonymous()
            .WithTags("Auth");

        return app;
    }

    private static RouteGroupBuilder MapTokenEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/",
            async Task<Results<Ok<string>, ValidationProblem>> (
                TokenRequest request,
                IGenerateMerchantToken generateMerchantToken,
                CancellationToken cancellationToken) =>
            {
                var output = await generateMerchantToken.ExecuteAsync(
                    new GenerateMerchantToken.Input(request.Document, request.AccountNumber),
                    cancellationToken);

                return TypedResults.Ok(output.Token);
            })
        .AddEndpointFilter<ValidationFilter<TokenRequest>>();

        return group;
    }
}
