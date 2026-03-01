using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Middleware;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger, 
    IProblemDetailsService problemDetailsService,
    IHostEnvironment env)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            logger.LogInformation("Request was cancelled by the client.");
            return true; 
        }

        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, title, customDetail) = exception switch
        {
            UnauthorizedAccessException => 
                (StatusCodes.Status401Unauthorized, "Unauthorized Access", null),
            
            KeyNotFoundException => 
                (StatusCodes.Status404NotFound, "Resource Not Found", null),
            
            Microsoft.EntityFrameworkCore.DbUpdateException dbEx 
                when dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547 
                => (StatusCodes.Status400BadRequest, 
                    "Data Integrity Violation", 
                    "This record cannot be deleted or modified because it is currently in use by another part of the system."),
        
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", (string?)null)
        };

        string detail = customDetail ?? (env.IsDevelopment() 
            ? exception.ToString() 
            : "An unexpected error occurred. Please try again later.");

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            }
        });
    }
}