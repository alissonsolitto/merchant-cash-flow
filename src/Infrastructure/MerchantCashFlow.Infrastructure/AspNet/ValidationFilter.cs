using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace MerchantCashFlow.Infrastructure.AspNet;

public sealed class ValidationFilter<TRequest>: IEndpointFilter where TRequest : class
{
    private readonly IValidator<TRequest> _validator;

    public ValidationFilter(IValidator<TRequest> validator) => this._validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is not null)
        {
            var validationResult = await this._validator.ValidateAsync(request, context.HttpContext.RequestAborted);

            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }
        }

        return await next(context);
    }
}
