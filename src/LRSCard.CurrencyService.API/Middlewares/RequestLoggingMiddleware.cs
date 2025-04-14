using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Security.Claims;

namespace LRSCard.CurrencyService.API.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            string correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            var userId = context.User.FindFirst(ClaimTypes.Name)?.Value;
            var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var method = context.Request.Method;
            var path = context.Request.Path;        

            using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))
            using (LogContext.PushProperty("Sub", sub ?? ""))
            using (LogContext.PushProperty("CorrelationId", correlationId ?? ""))
            using (LogContext.PushProperty("IP", ip ?? ""))
            {
                try
                {
                    await _next(context);
                    stopwatch.Stop();

                    Log.Information("Request {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                        method,
                        path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds,
                        ip
                    );
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    Log.Error(ex, "Request {Method} {Path} failed in {Elapsed}ms",
                        method,
                        path,
                        stopwatch.ElapsedMilliseconds,
                        ip
                    );

                    throw;
                }
            }
        }
    }
}
