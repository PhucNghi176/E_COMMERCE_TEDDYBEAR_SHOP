using System.Text.Json;
using COMMAND.APPLICATION.Exceptions;
using CONTRACT.CONTRACT.DOMAIN.Exceptions;

namespace COMMAND.API.Middlewares;
internal sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);

            await HandleExceptionAsync(context, e);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var statusCode = GetStatusCode(exception);

        var response = new
        {
            title = GetTitle(exception),
            status = statusCode,
            detail = exception.Message,
            errors = GetErrors(exception)
        };

        httpContext.Response.ContentType = "application/json";

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            TagException.TagNotFoundException _ => StatusCodes.Status404NotFound,
            AlreadyExistedException _ => StatusCodes.Status409Conflict,
            ImageUploadFailException _ => StatusCodes.Status400BadRequest,
            NotFoundException _ => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(Exception exception)
    {
        return exception switch
        {
            DomainException applicationException => applicationException.Title,
            _ => "Server Error"
        };
    }

    private static IReadOnlyCollection<ValidationError>? GetErrors(Exception exception)
    {
        IReadOnlyCollection<ValidationError> errors = null!;

        if (exception is ValidationException validationException) errors = validationException.Errors;

        return errors;
    }
}