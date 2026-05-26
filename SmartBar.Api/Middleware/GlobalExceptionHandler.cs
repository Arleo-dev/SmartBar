using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SmartBar.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails();

        switch (exception)
        {
            case ValidationException validationException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Validation Failed";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
                problemDetails.Detail = "One or more validation errors occurred.";

                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                problemDetails.Extensions.Add("errors", errors);
                break;

            case InvalidOperationException invalidOpException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad Request";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
                problemDetails.Detail = invalidOpException.Message;
                break;

            case DbUpdateException dbUpdateException when dbUpdateException.InnerException is PostgresException postgresException
                                                      && postgresException.SqlState == PostgresErrorCodes.UniqueViolation:
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Conflict";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8";
                problemDetails.Detail = "An entity with the same unique value already exists.";

                problemDetails.Extensions.Add("constraint", postgresException.ConstraintName);

                if (!string.IsNullOrEmpty(postgresException.TableName))
                {
                    problemDetails.Extensions.Add("table", postgresException.TableName);
                }
                break;
            default:
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
                problemDetails.Detail = "An unexpected error occurred on the server.";
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}