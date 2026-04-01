using System.Diagnostics;
using System.Security.Claims;

namespace WebApi.DermaImage.Middleware;

public sealed class ApiRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiRequestLoggingMiddleware> _logger;

    public ApiRequestLoggingMiddleware(RequestDelegate next, ILogger<ApiRequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        var method = request.Method;
        var path = request.Path.Value ?? string.Empty;
        var queryString = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
        var traceId = context.TraceIdentifier;
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

        _logger.LogInformation(
            "API Request START [{TraceId}] {Method} {Path}{QueryString} User:{UserId}",
            traceId,
            method,
            path,
            queryString,
            userId);

        try
        {
            await _next(context);
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    "API Request END [{TraceId}] {Method} {Path} -> {StatusCode} in {ElapsedMs}ms",
                    traceId,
                    method,
                    path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds);
                return;
            }

            if (statusCode >= StatusCodes.Status400BadRequest)
            {
                _logger.LogWarning(
                    "API Request END [{TraceId}] {Method} {Path} -> {StatusCode} in {ElapsedMs}ms",
                    traceId,
                    method,
                    path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds);
                return;
            }

            _logger.LogInformation(
                "API Request END [{TraceId}] {Method} {Path} -> {StatusCode} in {ElapsedMs}ms",
                traceId,
                method,
                path,
                statusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "API Request FAIL [{TraceId}] {Method} {Path} after {ElapsedMs}ms",
                traceId,
                method,
                path,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

public static class ApiRequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseApiRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiRequestLoggingMiddleware>();
    }
}
