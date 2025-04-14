using Microsoft.AspNetCore.Http;
using Serilog;
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

            var ip = context.Connection.RemoteIpAddress?.ToString();
            var method = context.Request.Method;
            var path = context.Request.Path;

            try
            {
                await _next(context);

                stopwatch.Stop();

                var userId = context.User.FindFirst(ClaimTypes.Name)?.Value;
                var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                Log.Information("Request {Method} {Path} responded {StatusCode} in {Elapsed}ms | IP: {IP} | User: {UserId} | Sub: {sub} | CorrelationId: {CorrelationId}",
                    method,
                    path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    ip,
                    userId ?? "Anonymous",
                    sub ?? "",
                    correlationId
                );
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                var userId = context.User.FindFirst(ClaimTypes.Name)?.Value;
                var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                Log.Error(ex, "Request {Method} {Path} failed in {Elapsed}ms | IP: {IP} | User: {UserId} | Sub: {sub} | CorrelationId: {CorrelationId}",
                    method,
                    path,
                    stopwatch.ElapsedMilliseconds,
                    ip,
                    userId ?? "Anonymous",
                    sub ?? "",
                    correlationId
                );

                throw;
            }
        }
    }
}
