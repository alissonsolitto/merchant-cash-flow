using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MerchantCashFlow.Auth.Application.Persistence;
using MerchantCashFlow.Infrastructure.DataProtection;
using MerchantCashFlow.Infrastructure.Diagnostics;
using MerchantCashFlow.Infrastructure.Security;
using MerchantCashFlow.Infrastructure.UseCase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MerchantCashFlow.Auth.Application.Features;

public interface IGenerateMerchantToken: IUseCase<GenerateMerchantToken.Input, GenerateMerchantToken.Output> { }

public sealed class GenerateMerchantToken: IGenerateMerchantToken
{
    public sealed record Input(string Document, string AccountNumber);
    public sealed record Output(string Token);

    private readonly ILogger<GenerateMerchantToken> _logger;
    private readonly TokenOptions _options;
    private readonly DbCashFlowAuthContext _context;

    public GenerateMerchantToken(
        DbCashFlowAuthContext context,
        IOptions<TokenOptions> options,
        ILogger<GenerateMerchantToken> logger)
    {
        this._context = context;
        this._options = options.Value;
        this._logger = logger;
    }

    public async Task<Output> ExecuteAsync(Input input, CancellationToken cancellationToken = default)
    {
        var documentHash = PiiHash.Of(input.Document);
        var accountNumberHash = PiiHash.Of(input.AccountNumber);

        var merchant = await this._context.Merchant
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.Document.Hash == documentHash && m.AccountNumber.Hash == accountNumberHash,
                cancellationToken);

        if (merchant is null)
        {
            this._logger.LogWarning("Token not issued {DocumentHash}", documentHash.Value);
            throw AppException.Unauthorized("Please check your credentials and try again!");
        }

        var token = this.GenerateJwtToken(merchant.MerchantId, documentHash.Value, accountNumberHash.Value, merchant.Scope);
        return new Output(token);
    }

    private string GenerateJwtToken(Guid merchantId, string documentHash, string accountNumberHash, string scope)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(this._options.SigningKey);
        var expiresIn = TimeSpan.FromMinutes(this._options.ExpirationMinutes);
        var issuedAt = DateTimeOffset.UtcNow;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = this._options.Issuer,
            Audience = this._options.Audience,
            IssuedAt = issuedAt.UtcDateTime,
            Expires = issuedAt.Add(expiresIn).UtcDateTime,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, merchantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                new Claim(TokenClaims.Scope, scope),
                new Claim(TokenClaims.DocumentHash, documentHash),
                new Claim(TokenClaims.AccountNumberHash, accountNumberHash),
            ]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        return jwtTokenHandler.WriteToken(token);
    }
}
