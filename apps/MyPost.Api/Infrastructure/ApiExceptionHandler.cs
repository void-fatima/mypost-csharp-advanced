using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPost.Application.Common;
using MyPost.Domain.Common;

namespace MyPost.Api.Infrastructure;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Access denied"),
            ConflictException or DomainException or DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "The request conflicts with the current state"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
        if (status == StatusCodes.Status500InternalServerError) logger.LogError(exception, "Unhandled API exception");
        else logger.LogInformation("API request rejected with {Status}: {Message}", status, exception.Message);

        ProblemDetails problem = exception is ValidationException validation
            ? new HttpValidationProblemDetails(validation.Errors) { Status = status, Title = title, Detail = exception.Message }
            : new ProblemDetails { Status = status, Title = title, Detail = status == 500 ? "The server could not process the request." : exception.Message };
        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
