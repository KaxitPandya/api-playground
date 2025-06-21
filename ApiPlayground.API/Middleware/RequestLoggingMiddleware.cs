using System.Diagnostics;

namespace ApiPlayground.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start timing
        var stopwatch = Stopwatch.StartNew();
        
        // Capture the request path and method
        var path = context.Request.Path;
        var method = context.Request.Method;
        
        // Log the beginning of the request
        _logger.LogInformation("Starting {Method} {Path}", method, path);
        
        try
        {
            // Call the next middleware in the pipeline
            await _next(context);
            
            // Log the successful completion
            stopwatch.Stop();
            _logger.LogInformation(
                "Completed {Method} {Path} with status code {StatusCode} in {ElapsedMilliseconds}ms",
                method, 
                path, 
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            // Log any exceptions
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error during {Method} {Path} after {ElapsedMilliseconds}ms",
                method,
                path,
                stopwatch.ElapsedMilliseconds);
            
            // Re-throw to allow error handling middleware to catch it
            throw;
        }
    }
}
