using FluentValidation;

namespace MerchantCashFlow.Auth.Api.Features.Tokens;

public class TokenRequest
{
    public string Document { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;

    public class Validator: AbstractValidator<TokenRequest>
    {
        public Validator()
        {
            this.RuleFor(x => x.Document)
                .NotEmpty();

            this.RuleFor(x => x.AccountNumber)
                .NotEmpty();
        }
    }
}
