using System.Text.Json;
using FloorPlanner.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FloorPlanner.Api.Middleware;

/// <summary>Converts exceptions thrown by the rest of the pipeline into HTTP responses (ProblemDetails).</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private const int ClientClosedRequestStatusCode = 499;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // The status code and headers cannot be changed once the response has started.
            if (context.Response.HasStarted)
            {
                throw;
            }

            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        string method = context.Request.Method;
        string path = context.Request.Path.Value ?? string.Empty;

        switch (ex)
        {
            case OperationCanceledException when context.RequestAborted.IsCancellationRequested:
                logger.LogInformation("Request {Method} {Path} was cancelled by the client", method, path);
                context.Response.StatusCode = ClientClosedRequestStatusCode;
                return;

            case NotFoundException:
                logger.LogWarning("Request {Method} {Path} failed: {Message}", method, path, ex.Message);
                await WriteAsync(context, CreateProblem(context, StatusCodes.Status404NotFound, "Resource not found", ex.Message));
                return;

            case DomainException domainException:
                logger.LogWarning("Request {Method} {Path} failed: {Message}", method, path, ex.Message);
                ProblemDetails problem = CreateProblem(context, StatusCodes.Status409Conflict, "Domain rule violated", ex.Message);

                if (domainException is PlacementException placementException)
                {
                    problem.Extensions["failure"] = placementException.Failure.ToString();
                    problem.Extensions["conflictingObjectId"] = placementException.ConflictingObjectId;
                }

                await WriteAsync(context, problem);
                return;

            default:
                logger.LogError(ex, "Unexpected error while processing {Method} {Path}", method, path);
                await WriteAsync(context, CreateProblem(context, StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred."));
                return;
        }
    }

    private static ProblemDetails CreateProblem(HttpContext context, int status, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path.Value,
        };
    }

    private static async Task WriteAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
    }
}
