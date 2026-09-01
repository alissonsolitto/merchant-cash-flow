using Microsoft.AspNetCore.Http;

namespace MerchantCashFlow.Infrastructure.Diagnostics;

public class AppException: Exception
{
    public AppException(string title, params string[] errors)
        : this(StatusCodes.Status400BadRequest, title, null, errors)
    {
    }

    public AppException(int statusCode, string title, Exception? innerException = null, params string[] errors)
        : base(title, innerException)
    {
        this.StatusCode = statusCode;
        this.Title = title;
        this.Errors = errors;
    }

    public int StatusCode { get; }

    public string Title { get; }

    public IReadOnlyList<string> Errors { get; }

    public static AppException Unauthorized(string title) => new(StatusCodes.Status401Unauthorized, title);
}
