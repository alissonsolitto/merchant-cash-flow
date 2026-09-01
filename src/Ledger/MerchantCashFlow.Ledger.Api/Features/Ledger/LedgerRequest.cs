using FluentValidation;
using MerchantCashFlow.Ledger.Application.Entities;

namespace MerchantCashFlow.Ledger.Api.Features.Entries;

public class LedgerRequest
{
    public string Type { get; set; } = null!;
    public decimal Amount { get; set; }

    public class Validator: AbstractValidator<LedgerRequest>
    {
        public Validator()
        {
            this.RuleFor(x => x.Type)
                .NotEmpty()
                .Must(type => Enum.TryParse<EntryType>(type, ignoreCase: true, out _))
                .WithMessage("Type must be Credit or Debit.");

            this.RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");

            this.RuleFor(x => x.Amount)
                .Must(amount => decimal.Round(amount, 2) == amount)
                .WithMessage("Amount must have at most two decimal places.");
        }
    }
}
