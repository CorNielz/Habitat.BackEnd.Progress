using Microsoft.AspNetCore.Mvc;

namespace Habitat.BackEnd.Progress.WebApi.ProblemDetails;

public static class ProblemDetailsFactory
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails Create(HttpContext httpContext, int status, string title, string detail, string? type = null)
    {
        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = type ?? ResolveType(status),
            Title = title,
            Status = status,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }

    private static string ResolveType(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        StatusCodes.Status503ServiceUnavailable => "https://tools.ietf.org/html/rfc9110#section-15.6.4",
        _ => "about:blank"
    };
}
