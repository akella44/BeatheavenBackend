namespace WebApp.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    
    private static readonly string ApiKey = Environment.GetEnvironmentVariable("API_KEY")!;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Headers.TryGetValue(ApiKeyHeaderName, out
            var extractedApiKey);
        if (!extractedApiKey.Any() || !ApiKey.Equals(extractedApiKey)) {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(
                new { StatusCode = StatusCodes.Status401Unauthorized, 
                    Message = "Unauthorized. Required right API key." 
                });
            return;
        }
        await _next(context);
    }
}