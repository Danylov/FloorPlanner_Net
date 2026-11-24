using FloorPlanner.Api.Middleware;
using FloorPlanner.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace FloorPlanner.Tests.Middleware;

/// <summary>Tests for <see cref="ExceptionHandlingMiddleware"/>.</summary>
public sealed class ExceptionHandlingMiddlewareTests
{
    private static async Task<(DefaultHttpContext Context, string Body)> InvokeAsync(Exception exception, CancellationToken requestAborted = default)
    {
        DefaultHttpContext context = new()
        {
            RequestAborted = requestAborted,
            Response = { Body = new MemoryStream() },
        };

        ExceptionHandlingMiddleware middleware = new(_ => throw exception, NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        string body = await new StreamReader(context.Response.Body).ReadToEndAsync(CancellationToken.None);

        return (context, body);
    }

    /// <summary>NotFoundException is converted to 404.</summary>
    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        (DefaultHttpContext context, string body) = await InvokeAsync(new NotFoundException("Room", 5));

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Contains("Room with id 5 was not found.", body);
    }

    /// <summary>DomainException is converted to 409.</summary>
    [Fact]
    public async Task InvokeAsync_DomainException_Returns409()
    {
        (DefaultHttpContext context, _) = await InvokeAsync(new DomainException("Rule violated"));

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    /// <summary>A malformed request (BadHttpRequestException) is converted to 400, because it is a client error and not a server error.</summary>
    [Fact]
    public async Task InvokeAsync_BadHttpRequestException_Returns400()
    {
        (DefaultHttpContext context, string body) = await InvokeAsync(new BadHttpRequestException("Required parameter was not provided."));

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("Required parameter was not provided.", body);
    }

    /// <summary>An unexpected exception is converted to 500 and its text is not exposed to the client.</summary>
    [Fact]
    public async Task InvokeAsync_UnexpectedException_Returns500WithoutExceptionText()
    {
        (DefaultHttpContext context, string body) = await InvokeAsync(new Exception("secret"));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.DoesNotContain("secret", body);
    }

    /// <summary>A canceled request is converted to 499 and the exception does not escape.</summary>
    [Fact]
    public async Task InvokeAsync_CancelledByClient_Returns499()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        (DefaultHttpContext context, string body) = await InvokeAsync(new OperationCanceledException(), cts.Token);

        Assert.Equal(499, context.Response.StatusCode);
        Assert.Equal(string.Empty, body);
    }
}
