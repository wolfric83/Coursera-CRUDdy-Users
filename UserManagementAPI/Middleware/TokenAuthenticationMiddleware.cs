using System.Text.Json;
using Microsoft.Net.Http.Headers;

namespace UserManagementAPI.Middleware;

public class TokenAuthenticationMiddleware
{
    private const string BearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public TokenAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<TokenAuthenticationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var expectedToken = _configuration["ApiAuthentication:Token"];
        if (string.IsNullOrWhiteSpace(expectedToken))
        {
            _logger.LogError(
                "API authentication token configuration is missing for {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            await WriteJsonResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Authentication configuration error.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader))
        {
            await WriteUnauthorizedResponseAsync(context);
            return;
        }

        var authorizationValue = authorizationHeader.ToString();
        if (!authorizationValue.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await WriteUnauthorizedResponseAsync(context);
            return;
        }

        var suppliedToken = authorizationValue[BearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(suppliedToken) || suppliedToken != expectedToken)
        {
            await WriteUnauthorizedResponseAsync(context);
            return;
        }

        await _next(context);
    }

    private static async Task WriteUnauthorizedResponseAsync(HttpContext context)
    {
        await WriteJsonResponseAsync(
            context,
            StatusCodes.Status401Unauthorized,
            "Unauthorized.");
    }

    private static async Task WriteJsonResponseAsync(HttpContext context, int statusCode, string error)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error,
            statusCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
