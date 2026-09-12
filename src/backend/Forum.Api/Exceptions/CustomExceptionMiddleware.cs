using Forum.Application.Exceptions;
using Forum.Domain.Exceptions;

namespace Forum.Api.Exceptions;

public sealed class CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["Cache-Control"] = "no-store";
        try
        {
            await next(context);
        }
        catch (RequestValidationException ex)
        {
            await Results.ValidationProblem(ex.Errors).ExecuteAsync(context);
        }
        catch (ForumException ex)
        {
            var status = ex.Kind switch
            {
                ForumError.NotFound => StatusCodes.Status404NotFound,
                ForumError.Conflict => StatusCodes.Status409Conflict,
                ForumError.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };
            await Results.Problem(statusCode: status, title: ex.Message).ExecuteAsync(context);
        }
        catch (BadHttpRequestException)
        {
            await Results.Problem(statusCode: 400, title: "Invalid request.").ExecuteAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request failed: {TraceId}", context.TraceIdentifier);
            await Results.Problem(statusCode: 500, title: "An unexpected error occurred.",
                extensions: new Dictionary<string, object?> { ["traceId"] = context.TraceIdentifier }).ExecuteAsync(context);
        }
    }
}
