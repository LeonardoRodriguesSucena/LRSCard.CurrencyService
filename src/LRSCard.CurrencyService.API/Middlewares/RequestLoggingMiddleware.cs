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

            //Creating the correlationID that will be added to the log & context.Response.Headers
            string correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            var userId = context.User.FindFirst(ClaimTypes.Name)?.Value;
            var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var method = context.Request.Method;
            var path = context.Request.Path;
                        
            //attaching user information to the log
            using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))
            using (LogContext.PushProperty("Sub", sub ?? ""))
            using (LogContext.PushProperty("CorrelationId", correlationId ?? ""))
            using (LogContext.PushProperty("IP", ip ?? ""))
            {
                //log template
                string logTemplate = "Request:{Method} {Path}| ResponseCode:{StatusCode}| ResponseTime:{Elapsed}ms";
                try
                {
                    await _next(context);
                    stopwatch.Stop();
                    
                    Log.Information(logTemplate,
                        method,
                        path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds
                    );
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    Log.Error(ex, logTemplate,
                       method,
                       path,
                       context.Response.StatusCode,
                       stopwatch.ElapsedMilliseconds
                    );

                    throw;
                }
            }
        }
    }
}
