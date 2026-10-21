using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Infrastructure.Persistence;

namespace TrackManagement.Api.ExceptionHandling;

/// <summary>
/// Maps expected exceptions to ProblemDetails. Anything unmapped returns false,
/// so the built-in exception handler responds with a 500.
/// </summary>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            ValidationException validation => new ValidationProblemDetails(ToErrors(validation)),
            NotFoundException => new ProblemDetails { Status = StatusCodes.Status404NotFound, Detail = exception.Message },
            ConflictException => new ProblemDetails { Status = StatusCodes.Status409Conflict, Detail = exception.Message },
            // The uniqueness check and the INSERT aren't atomic; the unique index is the final guard.
            DbUpdateException dbUpdate when dbUpdate.IsUniqueConstraintViolation() => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Detail = "A record with the same unique value already exists."
            },
            DomainException => new ProblemDetails { Status = StatusCodes.Status422UnprocessableEntity, Detail = exception.Message },
            _ => null
        };

        if (problem is null)
            return false;

        problem.Status ??= StatusCodes.Status400BadRequest;
        httpContext.Response.StatusCode = problem.Status.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static Dictionary<string, string[]> ToErrors(ValidationException exception) =>
        exception.Errors
            .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
}
