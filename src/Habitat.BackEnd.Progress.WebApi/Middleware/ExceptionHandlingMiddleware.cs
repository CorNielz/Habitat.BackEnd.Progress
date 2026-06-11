using System.Text.Json;
using Habitat.BackEnd.Progress.WebApi.ProblemDetails;
using MySqlConnector;

namespace Habitat.BackEnd.Progress.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Database failure while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status503ServiceUnavailable, "Service Unavailable", "The database is temporarily unavailable. Please try again shortly.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = ProblemDetailsFactory.Create(context, status, title, detail);
        await JsonSerializer.SerializeAsync(context.Response.Body, problem, new JsonSerializerOptions(JsonSerializerDefaults.Web), context.RequestAborted);
    }
}
