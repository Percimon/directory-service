using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectoryService.Presentation.Middlewares;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace DirectoryService.Presentation.Middlewares;

public class RequestCorrelationIdMiddleware
{
    private const string CORRELATION_ID_HEADER_NAME = "X-Correlation-Id";

    private const string CORRELATION_ID = "CorrelationId";

    private readonly RequestDelegate _next;

    public RequestCorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        context.Request.Headers.TryGetValue(CORRELATION_ID_HEADER_NAME, out StringValues correlationIdValues);

        string correlationId = correlationIdValues.FirstOrDefault() ?? context.TraceIdentifier;

        using (LogContext.PushProperty(CORRELATION_ID, correlationId))
        {
            return _next(context);
        }
    }
}

public static class RequestCorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestCorrelationId(this IApplicationBuilder applicationBuilder)
    {
        return applicationBuilder.UseMiddleware<RequestCorrelationIdMiddleware>();
    }
}