using System.Security.Claims;
using System.Text;
using MerchantCashFlow.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace MerchantCashFlow.Gateway.Security;

public static class GatewayAuthenticationExtensions
{
    private const string LedgerWrite = "ledger:write";
    private const string StatementRead = "statement:read";

    public static IServiceCollection AddGatewayAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TokenOptions>().Bind(configuration.GetSection(TokenOptions.SectionName));
        var jwt = configuration.GetSection(TokenOptions.SectionName).Get<TokenOptions>() ?? new TokenOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(bearer =>
            {
                // Sem o mapeamento legado os claims chegam com o nome que o emissor escreveu.
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds),
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(LedgerWrite, policy => policy.RequireAssertion(context =>
                context.User.HasScope(AccessScopes.Ledger) || context.User.HasScope(AccessScopes.Full)))
            .AddPolicy(StatementRead, policy => policy.RequireAssertion(context =>
                context.User.HasScope(AccessScopes.Statement) || context.User.HasScope(AccessScopes.Full)));

        return services;
    }

    public static bool HasScope(this ClaimsPrincipal principal, string scope) =>
        principal.FindAll(TokenClaims.Scope)
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Contains(scope, StringComparer.Ordinal);
}
