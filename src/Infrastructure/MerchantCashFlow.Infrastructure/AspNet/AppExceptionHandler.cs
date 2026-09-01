using MerchantCashFlow.Infrastructure.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MerchantCashFlow.Infrastructure.AspNet;

public sealed class AppExceptionHandler: IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;

    public AppExceptionHandler(IProblemDetailsService problemDetails) => this._problemDetails = problemDetails;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AppException appException)
        {
            return false;
        }

        httpContext.Response.StatusCode = appException.StatusCode;

        ProblemDetails problemDetails = appException.Errors.Count > 0
            ? new ValidationProblemDetails(new Dictionary<string, string[]> { ["ValidationErrors"] = [.. appException.Errors] })
            : new ProblemDetails();

        problemDetails.Title = appException.Title;
        problemDetails.Status = appException.StatusCode;

        return await this._problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });
    }
}
